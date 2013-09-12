using System;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;

namespace LinqToLdap.Examples.Wpf.ViewModels
{
    public class UserListViewModel : ViewModel
    {
        private Action<string> _show;

        public UserListViewModel(string dn, string firstName, string lastName, Action<string> show)
        {
            Refresh(dn, firstName, lastName);
            _show = show;

            ShowCommand = new RelayCommand(() => _show(DistinguishedName));
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

        public void Refresh(string dn, string firstName, string lastName)
        {
            DistinguishedName = dn;
            Name = string.Format("{0} {1}", firstName, lastName);
        }

        public override void Cleanup()
        {
            _show = null;
            base.Cleanup();
        }
    }
}
