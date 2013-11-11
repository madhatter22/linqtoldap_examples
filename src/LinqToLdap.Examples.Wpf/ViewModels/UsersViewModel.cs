using System;
using System.Collections.Generic;
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
            LoadUsersAsync()
                .ContinueWith(LoadUsersComplete, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private Task<List<UserListViewModel>> LoadUsersAsync()
        {
            return Task.Run(
                    () =>
                        {
                            var query = _context.Query<User>();
                            if (!string.IsNullOrWhiteSpace(SearchText))
                            {
                                if (CustomFilter)
                                {
                                    //by default filters passed to the Where clause are not cleaned.
                                    //you can attempte to clean them yourselves using the CleanFilterValue method, but it has its limits.
                                    //if your users don't understand valid filters I would go with fixed search options.
                                    query = query.Where(SearchText);
                                }
                                else
                                {
                                    var split = SearchText.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                                    var expression = PredicateBuilder.Create<User>();
                                    expression = split.Length == 2
                                                     ? expression.And(
                                                         s =>
                                                         s.FirstName.StartsWith(split[0]) &&
                                                         s.LastName.StartsWith(split[1]))
                                                     : split.Aggregate(expression,
                                                                       (current, t) =>
                                                                       current.Or(
                                                                           s =>
                                                                           s.UserId == t ||
                                                                           s.FirstName.StartsWith(t) ||
                                                                           s.LastName.StartsWith(t)));

                                    query = query.Where(expression);
                                }
                            }

                            return query.Select(
                                s => new UserListViewModel(s.DistinguishedName, s.UserId, s.FirstName, s.LastName,
                                    LoadUser))
                                .ToList();
                        });
        }

        private void LoadUsersComplete(Task<List<UserListViewModel>> task)
        {
            _messenger.Send(new ToggleBusyMessage());
            _isBusy = false;
            if (task.Exception != null)
            {
                _messenger.Send(new ErrorMessage(task.Exception));
                return;
            }

            Users.Clear();
            foreach (var userListViewModel in task.Result)
            {
                Users.Add(userListViewModel);
            }
            ChangeView(this);
        }

        public void LoadUser(string id)
        {
            _messenger.Send(new ToggleBusyMessage());
            LoadUserAsync(id)
                .ContinueWith(t => LoadUserComplete(t, id), TaskScheduler.FromCurrentSynchronizationContext());
        }

        public Task<User> LoadUserAsync(string id)
        {
            return Task.Run(() => _context.Query<User>().FirstOrDefault(u => u.UserId == id));
        }

        public void LoadUserComplete(Task<User> task, string id)
        {
            _messenger.Send(new ToggleBusyMessage());
            if (task.Exception != null)
            {
                _messenger.Send(new ErrorMessage(task.Exception));
                return;
            }

            var user = task.Result;

            if (user == null)
            {
                _messenger.Send(new Messages.DialogMessage(string.Format("{0} not found", id), "Not Found", DialogType.Error));
            }
            else
            {
                ChangeView(new UserItemViewModel(user, () => ChangeView(this)));
            }
        }

        private void ChangeView(ViewModel vm)
        {
            if (CurrentContent != null && CurrentContent != this) CurrentContent.Cleanup();

            CurrentContent = vm;
        }

        public override void Cleanup()
        {
            _messenger = null;
            _context.Dispose();
            _context = null;
            if (CurrentContent != null && CurrentContent != this) CurrentContent.Cleanup();

            base.Cleanup();
        }
    }
}
