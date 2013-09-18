using System;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using LinqToLdap.Examples.Models;
using LinqToLdap.Examples.Wpf.Messages;

namespace LinqToLdap.Examples.Wpf.ViewModels
{
    public class UserItemViewModel : ViewModel
    {
        private User _user;
        private Action _close;
        
        public UserItemViewModel(User user, Action close)
        {
            _user = user;
            _close = close;

            CloseCommand = new RelayCommand(_close);
        }

        public ICommand CloseCommand { get; private set; }

        public string DistinguishedName
        {
            get { return _user == null ? "" : _user.DistinguishedName; }
            set
            {
                if (_user == null || _user.DistinguishedName == value) return;
                _user.CommonName = value;
                RaisePropertyChanged("DistinguishedName");
            }
        }

        public string CommonName
        {
            get { return _user == null ? "" : _user.CommonName; }
            set
            {
                if (_user == null || _user.CommonName == value) return;
                _user.CommonName = value;
                RaisePropertyChanged("CommonName");
            }
        }

        public string UserId
        {
            get { return _user == null ? "" : _user.UserId; }
            set
            {
                if (_user == null || _user.UserId == value) return;
                _user.UserId = value;
                RaisePropertyChanged("UserId");
            }
        }

        public string FirstName
        {
            get { return _user == null ? "" : _user.FirstName; }
            set
            {
                if (_user == null || _user.FirstName == value) return;
                _user.FirstName = value;
                RaisePropertyChanged("FirstName");
            }
        }

        public string LastName
        {
            get { return _user == null ? "" : _user.LastName; }
            set
            {
                if (_user == null || _user.LastName == value) return;
                _user.LastName = value;
                RaisePropertyChanged("LastName");
            }
        }

        public string TelephoneNumber
        {
            get { return _user == null ? "" : _user.TelephoneNumber; }
            set
            {
                if (_user == null || _user.TelephoneNumber == value) return;
                _user.TelephoneNumber = value;
                RaisePropertyChanged("TelephoneNumber");
            }
        }
        
        public override void Cleanup()
        {
            _close = null;
            base.Cleanup();
        }
    }
}
