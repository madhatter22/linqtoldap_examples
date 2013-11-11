using System.Collections.ObjectModel;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Messaging;
using LinqToLdap.Examples.Wpf.Messages;

namespace LinqToLdap.Examples.Wpf.ViewModels
{
    public class ServerInfoViewModel : ViewModel
    {
        private IMessenger _messenger;
        private IDirectoryContext _context;

        public ServerInfoViewModel() : this(Get<IMessenger>(), Get<IDirectoryContext>())
        {
        }

        public ServerInfoViewModel(IMessenger messenger, IDirectoryContext context)
        {
            _messenger = messenger;
            _context = context;

            ServerSettings = new ObservableCollection<KeyValueViewModel>();
            PopulateData();
        }

        public ObservableCollection<KeyValueViewModel> ServerSettings { get; private set; }

        private void PopulateData()
        {
            _messenger.Send(new ToggleBusyMessage());
            Task.Run(
                () =>
                {
                    return _context.ListServerAttributes("altServer", "objectClass", "namingContexts",
                        "supportedControl", "supportedExtension",
                        "supportedLDAPVersion",
                        "supportedSASLMechanisms", "vendorName",
                        "vendorVersion",
                        "supportedAuthPasswordSchemes");

                    /********************************************************************************
                            under the covers this actually executes a query that looks something like this:
                            
                            _context.Query(null, SearchScope.Base)
                                    .Where("(objectClass=*)")
                                    .Select("namingContexts", "supportedControl", "supportedExtension", "supportedLDAPVersion",
                                            "supportedSASLMechanisms", "vendorName", "vendorVersion", "supportedAuthPasswordSchemes")
                                    .FirstOrDefault();
                            
                            null or empty naming contexts are not supported using the normal Query method, however.
                            *********************************************************************************/
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
            _context.Dispose();
            _context = null;

            base.Cleanup();
        }
    }
}
