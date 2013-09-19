using System.Diagnostics;
using System.DirectoryServices;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using LinqToLdap.Examples.Wpf.Messages;
using SearchScope = System.DirectoryServices.SearchScope;

namespace LinqToLdap.Examples.Wpf.ViewModels
{
    public class PerformanceViewModel : ViewModel
    {
        private int _directoryEntryRunCount = 1;
        private long _directoryEntryRunTime;
        private long _ldapRunTime;

        private IMessenger _messenger;

        public PerformanceViewModel() : this(Get<IMessenger>())
        {
        }

        public PerformanceViewModel(IMessenger messenger)
        {
            _messenger = messenger;
            DirectoryEntryCommand = new RelayCommand(DirEntryCompareWork);
        }

        public string DirectoryEntryRunText { get; set; }

        public ICommand DirectoryEntryCommand { get; private set; }

        private void DirEntryCompareWork()
        {
            _messenger.Send(new ToggleBusyMessage());

            var dirEntryThread = new Thread(() =>
                {
                    Stopwatch watch = Stopwatch.StartNew();
                    using (var entry = new DirectoryEntry("LDAP://ldap.testathon.net/OU=Users,DC=testathon,DC=net",
                                                          "CN=stuart,OU=Users,DC=testathon,DC=net", "stuart",
                                                          AuthenticationTypes.None))
                    {
                        using (var s = new DirectorySearcher(entry, "(cn=stuart)", new[] {"cn", "uid"},
                                                             SearchScope.Subtree))
                        {
                            var result = s.FindOne();
                            var match =
                                new {cn = result.Properties["cn"].ToString(), uid = result.Properties["uid"].ToString()};
                        }
                    }

                    watch.Stop();


                    _directoryEntryRunTime = watch.ElapsedMilliseconds;
                });

            var ldapThread = new Thread(() =>
                {
                    var watch = Stopwatch.StartNew();
                    using (var connection = new LdapConnection(new LdapDirectoryIdentifier("ldap.testathon.net"),
                                                               new NetworkCredential(
                                                                   "CN=stuart,OU=Users,DC=testathon,DC=net", "stuart"),
                                                               AuthType.Basic))
                    {
                        connection.SessionOptions.ProtocolVersion = 3;
                        //will only look for cn and uid
                        var match = connection.Query("OU=Users,DC=testathon,DC=net")
                                              .Select(
                                                  da =>
                                                  new {CommonName = da.GetString("cn"), UserId = da.GetString("uid")})
                                              .FirstOrDefault(_ => Filter.Equal(_, "cn", "stuart"));
                    }
                    watch.Stop();

                    _ldapRunTime = watch.ElapsedMilliseconds;
                });

            Task.Factory.StartNew(() =>
                {
                    dirEntryThread.Start();
                    while (dirEntryThread.IsAlive)
                    {
                    }
                    ldapThread.Start();
                    while (ldapThread.IsAlive)
                    {
                    }
                }, TaskCreationOptions.LongRunning)
                           .ContinueWith(t =>
                               {
                                   _messenger.Send(new ToggleBusyMessage());
                                   var sb = new StringBuilder();

                                   sb.AppendLine("=================================================");
                                   sb.AppendFormat("Directory Entry vs LINQ to LDAP Run #{0}", _directoryEntryRunCount);
                                   sb.AppendLine();
                                   sb.AppendFormat("Directory Entry time: {0} ms", _directoryEntryRunTime);
                                   sb.AppendLine();
                                   sb.AppendFormat("LINQ to LDAP time: {0} ms", _ldapRunTime);
                                   sb.AppendLine();
                                   sb.AppendLine("=================================================");
                                   sb.AppendLine();

                                   DirectoryEntryRunText += sb.ToString();
                                   RaisePropertyChanged("DirectoryEntryRunText");
                               }, TaskScheduler.FromCurrentSynchronizationContext());

        }
    }
}
