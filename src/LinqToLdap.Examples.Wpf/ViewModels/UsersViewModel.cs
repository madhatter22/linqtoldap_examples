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
        private bool _isBusy;

        public UsersViewModel() : this(Get<IMessenger>())
        {
        }

        public UsersViewModel(IMessenger messenger)
        {
            _messenger = messenger;

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
                            
                            using (var context = Get<IDirectoryContext>())
                            {
                                var query = context.Query<User>();
                                if (!string.IsNullOrWhiteSpace(SearchText))
                                {
                                    if (CustomFilter)
                                    {
                                        query = query.Where(SearchText);
                                    }
                                    else
                                    {
                                        //not really necessary but a good example of dynamic query building.
                                        Expression<Func<User, bool>> expression = PredicateBuilder.Create<User>()
                                            .And(s => s.FirstName == SearchText || Filter.Equal(s, "sn", SearchText.CleanFilterValue()));

                                        query = query.Where(expression);
                                    }
                                }

                                return query.Select(s => new UserListViewModel(s.DistinguishedName, s.FirstName, s.LastName, ShowUser))
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
                        }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void ShowUser(string dn)
        {
            CurrentUser = new UserItemViewModel(dn, CloseUser);
            Type = UserDisplayType.User;
        }

        private void CloseUser()
        {
            Type = UserDisplayType.List;
            CurrentUser = null;
        }

        public override void Cleanup()
        {
            _messenger = null;
            base.Cleanup();
        }
    }
}
