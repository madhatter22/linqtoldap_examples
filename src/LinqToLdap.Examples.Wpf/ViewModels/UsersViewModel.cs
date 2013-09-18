using System;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
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
        private IDirectoryContext _context;
        private bool _isBusy;

        public UsersViewModel() : this(Get<IMessenger>(), Get<IDirectoryContext>())
        {
        }

        public UsersViewModel(IMessenger messenger, IDirectoryContext context)
        {
            _messenger = messenger;
            _context = context;

            Users = new ObservableCollection<UserListViewModel>();

            SearchCommand = new RelayCommand(LoadData, () => !_isBusy);
            LoadData();
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

        private UserDisplayType _type;
        public UserDisplayType Type
        {
            get { return _type; }
            set
            {
                if (_type == value) return;
                _type = value;
                RaisePropertyChanged("Type");
            }
        }

        public ICommand SearchCommand { get; private set; }
        public ObservableCollection<UserListViewModel> Users { get; private set; }

        private UserItemViewModel _currentUser;
        public UserItemViewModel CurrentUser
        {
            get { return _currentUser; }
            set
            {
                if (_currentUser != null)
                {
                    _currentUser.Cleanup();
                }

                _currentUser = value;
                RaisePropertyChanged("CurrentUser");
            }
        }

        private void LoadData()
        {
            _messenger.Send(new ToggleBusyMessage());
            _isBusy = true;
            Task.Factory
                .StartNew(
                    () =>
                        {
                            var query = _context.Query<User>();
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
                                    //very basic support for searching by first and last name
                                    var split = SearchText.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                                    var expression = PredicateBuilder.Create<User>();
                                    expression = split.Length == 2
                                        ? expression.And(s => s.FirstName.StartsWith(split[0]) && s.LastName.StartsWith(split[1]))
                                        : split.Aggregate(expression, (current, t) => current.Or(s => s.FirstName.StartsWith(t) || s.LastName.StartsWith(t)));

                                    query = query.Where(expression);
                                }
                            }

                            return query.Select(s => new UserListViewModel(s.DistinguishedName, s.FirstName, s.LastName, ShowUser))
                                     .ToList();
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
                        }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void ShowUser(string dn)
        {
            
        }

        private void LoadUser(string id)
        {
            _messenger.Send(new ToggleBusyMessage());
            Task.Factory
                .StartNew(
                    () => _context.Query<User>().FirstOrDefault(u => u.UserId == id))
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
                            if (CurrentUser != null) CurrentUser.Cleanup();
                            CurrentUser = new UserItemViewModel(user, CloseUser);
                            Type = UserDisplayType.User;
                        }
                    });
        }

        private void CloseUser()
        {
            Type = UserDisplayType.List;
            CurrentUser = null;
        }

        public override void Cleanup()
        {
            _messenger = null;
            _context.Dispose();
            base.Cleanup();
        }
    }
}
