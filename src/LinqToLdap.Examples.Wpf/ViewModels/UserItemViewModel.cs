using System;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight.Messaging;
using LinqToLdap.Examples.Models;
using LinqToLdap.Examples.Wpf.Messages;

namespace LinqToLdap.Examples.Wpf.ViewModels
{
    public class UserItemViewModel : ViewModel
    {
        private User _user;
        private Action _close;
        private IMessenger _messenger;

        public UserItemViewModel(string dn, Action close) : this (dn, close, Get<IMessenger>())
        {
        }

        public UserItemViewModel(string dn, Action close, IMessenger messenger)
        {
            DistinguishedName = dn;
            _close = close;
            _messenger = messenger;
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

        private void LoadData(string dn)
        {
            Task.Factory
                .StartNew(
                    () =>
                        {
                            using (var context = Get<IDirectoryContext>())
                            {
                                //You can use GetByDN or a Query.  I chose to use a query here because
                                //GetByDN uses the DN as the search root so if it doesn't exist then an error will be thrown.
                                //Query will not have that problem.
                                return context.Query<User>(SearchScope.OneLevel)
                                               .SingleOrDefault(u => u.DistinguishedName == DistinguishedName);
                            }
                        })
                .ContinueWith(
                    t =>
                        {
                            if (t.Exception != null)
                            {
                                _messenger.Send(new ErrorMessage(t.Exception));
                                return;
                            }

                            _user = t.Result;
                            if (_user == null)
                            {
                                _messenger.Send(new Messages.DialogMessage(string.Format("{0} not found", dn), "Not Found", DialogType.Error));
                            }
                        });
        }

        public override void Cleanup()
        {
            _close = null;
            _messenger = null;
            base.Cleanup();
        }
    }
}
