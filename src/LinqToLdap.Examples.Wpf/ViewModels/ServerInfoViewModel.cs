using System.Collections.ObjectModel;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Messaging;
using LinqToLdap.Examples.Wpf.Messages;

namespace LinqToLdap.Examples.Wpf.ViewModels
{
    public class ServerInfoViewModel : ViewModel
    {
        private IMessenger _messenger;

        public ServerInfoViewModel() : this(Get<IMessenger>())
        {
        }

        public ServerInfoViewModel(IMessenger messenger)
        {
            _messenger = messenger;
            _messenger.Send(new ToggleBusyMessage());

            ServerSettings = new ObservableCollection<KeyValueViewModel>();
            PopulateData();
        }

        public ObservableCollection<KeyValueViewModel> ServerSettings { get; private set; }

        private void PopulateData()
        {
            Task.Factory
                .StartNew(
                    () =>
                        {
                            using (var directoryContext = Get<IDirectoryContext>())
                            {
                                //you can call ListServerAttributes to get this information, 
                                //but I wanted to give an example of a dynamic query

                                return directoryContext.ListServerAttributes("altServer", "objectClass", "namingContexts",
                                                                             "supportedControl", "supportedExtension",
                                                                             "supportedLDAPVersion",
                                                                             "supportedSASLMechanisms", "vendorName",
                                                                             "vendorVersion",
                                                                             "supportedAuthPasswordSchemes");
                            }
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

                            foreach (var directoryAttribute in t.Result)
                            {
                                ServerSettings.Add(new KeyValueViewModel(directoryAttribute.Key,
                                                                         directoryAttribute.Value));
                            }
                        }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public override void Cleanup()
        {
            _messenger = null;
            base.Cleanup();
        }
    }
}
