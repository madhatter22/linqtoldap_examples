using System;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;

namespace LinqToLdap.Examples.Wpf.ViewModels
{
    public class UserListViewModel : ViewModel
    {
        private Action<string> _show;

        public UserListViewModel(string dn, string userId, string firstName, string lastName, Action<string> show)
        {
            DistinguishedName = dn;
            UserId = userId;
            Name = string.Format("{0} {1}", firstName, lastName);

            _show = show;
            ShowCommand = new RelayCommand(() => _show(UserId));
        }

        public ICommand ShowCommand { get; private set; }

        private string _distinguishedName;
        public string DistinguishedName
        {
            get { return _distinguishedName; }
            set
            {
                if (_distinguishedName == value) return;
                _distinguishedName = value;
                RaisePropertyChanged("DistinguishedName");
            }
        }

        private string _userId;
        public string UserId
        {
            get { return _userId; }
            set
            {
                if (_userId == value) return;
                _userId = value;
                RaisePropertyChanged("UserId");
            }
        }

        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                if (_name == value) return;
                _name = value;
                RaisePropertyChanged("Name");
            }
        }

        public override void Cleanup()
        {
            _show = null;
            base.Cleanup();
        }
    }
}
