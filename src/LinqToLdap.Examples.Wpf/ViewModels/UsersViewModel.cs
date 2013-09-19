using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using LinqToLdap.Examples.Models;
using LinqToLdap.Examples.Wpf.Messages;
using System.Linq;

namespace LinqToLdap.Examples.Wpf.ViewModels
{
    public enum UserDisplayType
    {
        List,
        User
    }

    public class UsersViewModel : ViewModel
    {
        private IMessenger _messenger;
        private bool _isBusy;

        public UsersViewModel() : this(Get<IMessenger>())
        {
        }

        public UsersViewModel(IMessenger messenger)
        {
            _messenger = messenger;

            Users = new ObservableCollection<UserListViewModel>();

            SearchCommand = new RelayCommand(() => { if (!_isBusy) LoadUsers(); });
            LoadUsers();
        }

        private string _searchText;
        public string SearchText
        {
            get { return _searchText; }
            set
            {
                if (_searchText == value) return;
                _searchText = value;
                RaisePropertyChanged("SearchText");
            }
        }

        private bool _customFilter;
        public bool CustomFilter
        {
            get { return _customFilter; }
            set
            {
                if (_customFilter == value) return;
                _customFilter = value;
                RaisePropertyChanged("CustomFilter");
            }
        }

        public ICommand SearchCommand { get; private set; }
        public ObservableCollection<UserListViewModel> Users { get; private set; }

        private ViewModel _currentContent;
        public ViewModel CurrentContent
        {
            get { return _currentContent; }
            set
            {
                if (_currentContent == value) return;
                _currentContent = value;
                RaisePropertyChanged("CurrentContent");
            }
        }

        private void LoadUsers()
        {
            _messenger.Send(new ToggleBusyMessage());
            _isBusy = true;
            Task.Factory
                .StartNew(
                    () =>
                        {
                            using (var context = Get<IDirectoryContext>())
                            {
                                var query = context.Query<User>();
                                if (!string.IsNullOrWhiteSpace(SearchText))
                                {
                                    if (CustomFilter)
                                    {
                                        //by default filters passed to the Where clause are not cleaned.
                                        //if your users don't understand valid filters I would go with fixed search options.
                                        query = query.Where(SearchText);
                                    }
                                    else
                                    {
                                        var split = SearchText.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                                        var expression = PredicateBuilder.Create<User>();
                                        expression = split.Length == 2
                                            ? expression.And(s => s.FirstName.StartsWith(split[0]) && s.LastName.StartsWith(split[1]))
                                            : split.Aggregate(expression, (current, t) => current.Or(s => s.UserId == t || s.FirstName.StartsWith(t) || s.LastName.StartsWith(t)));

                                        query = query.Where(expression);
                                    }
                                }

                                return query.Select(s => new UserListViewModel(s.DistinguishedName, s.UserId, s.FirstName, s.LastName, LoadUser))
                                         .ToList();
                            }
                        })
                .ContinueWith(
                    t =>
                        {
                            _messenger.Send(new ToggleBusyMessage());
                            _isBusy = false;
                            if (t.Exception != null)
                            {
                                _messenger.Send(t.Exception);
                                return;
                            }

                            Users.Clear();
                            foreach (var userListViewModel in t.Result)
                            {
                                Users.Add(userListViewModel);
                            }
                            ChangeView(this);
                        }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void LoadUser(string id)
        {
            _messenger.Send(new ToggleBusyMessage());
            Task.Factory
                .StartNew(() =>
                    {
                        using (var context = Get<IDirectoryContext>())
                            return context.Query<User>().FirstOrDefault(u => u.UserId == id);
                    })
                .ContinueWith(
                    t =>
                    {
                        _messenger.Send(new ToggleBusyMessage());
                        if (t.Exception != null)
                        {
                            _messenger.Send(new ErrorMessage(t.Exception));
                            return;
                        }

                        var user = t.Result;

                        if (user == null)
                        {
                            _messenger.Send(new Messages.DialogMessage(string.Format("{0} not found", id), "Not Found", DialogType.Error));
                        }
                        else
                        {
                            ChangeView(new UserItemViewModel(user, () => ChangeView(this)));
                        }
                    });
        }

        private void ChangeView(ViewModel vm)
        {
            if (CurrentContent != null && CurrentContent != this) CurrentContent.Cleanup();

            CurrentContent = vm;
        }

        public override void Cleanup()
        {
            _messenger = null;
            if (CurrentContent != null && CurrentContent != this) CurrentContent.Cleanup();

            base.Cleanup();
        }
    }
}
