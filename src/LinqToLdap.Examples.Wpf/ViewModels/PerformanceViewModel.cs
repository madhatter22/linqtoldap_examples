using System.Diagnostics;
using System.DirectoryServices;
using System.DirectoryServices.Protocols;
using System.Linq;
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
        private const string Server = "ldap.virginia.edu";
        private const string NamingContext = "o=University of Virginia,c=US";

        private int _directoryEntryRunCount;
        private int _factoryRunCount;
        private long _directoryEntryRunTime;
        private long _ldapRunTime;
        private long _sdspRunTime;

        private long _singleFactoryRunTime;
        private long _pooledFactoryRunTime;
        private long _directoryEntryFactoryRunTime;
        private readonly object _factoryLock = new object();

        private IMessenger _messenger;

        public PerformanceViewModel() : this(Get<IMessenger>())
        {
        }

        public PerformanceViewModel(IMessenger messenger)
        {
            _messenger = messenger;
            DirectoryEntryCommand = new RelayCommand(DirEntryCompareWork);
            ConnectionPoolCommand = new RelayCommand(ConnectionFactoryCompareWork);
        }

        public string DirectoryEntryRunText { get; set; }
        public string FactoryRunText { get; set; }

        public ICommand DirectoryEntryCommand { get; private set; }
        public ICommand ConnectionPoolCommand { get; private set; }

        private void DirEntryCompareWork()
        {
            _messenger.Send(new ToggleBusyMessage());

            var dirEntryThread = new Thread(() =>
                {
                    Stopwatch watch = Stopwatch.StartNew();
                    using (var entry = new DirectoryEntry(string.Format("LDAP://{0}/{1}", Server, NamingContext)))
                    {
                        entry.AuthenticationType = AuthenticationTypes.Anonymous;
                        using (var s = new DirectorySearcher(entry, "(objectclass=*)"))
                        {
                            s.SearchScope = SearchScope.Base;
                            var result = s.FindOne();
                            var match = new { o = result.Properties["o"][0].ToString(), };
                        }
                    }

                    watch.Stop();

                    _directoryEntryRunTime = watch.ElapsedMilliseconds;
                });

            var ldapThread = new Thread(() =>
                {
                    var watch = Stopwatch.StartNew();
                    using (var connection = new LdapConnection(Server))
                    {
                        connection.SessionOptions.ProtocolVersion = 3;
                        connection.AuthType = AuthType.Anonymous;

                        var match = connection.Query(NamingContext, System.DirectoryServices.Protocols.SearchScope.Base)
                                      .Where(_ => Filter.Equal(_, "objectclass", "*"))
                                      .WithoutPaging()
                                      .Select(da => new { o = da.GetString("o") })
                                      .FirstOrDefault();
                    }
                    watch.Stop();

                    _ldapRunTime = watch.ElapsedMilliseconds;
                });

            var sdspThread = new Thread(() =>
                {
                    var watch = Stopwatch.StartNew();
                    using (var connection = new LdapConnection(Server))
                    {
                        connection.SessionOptions.ProtocolVersion = 3;
                        connection.AuthType = AuthType.Anonymous;

                        var request = new SearchRequest
                            {
                                Filter = "(objectclass=*)",
                                DistinguishedName = NamingContext,
                                Scope = System.DirectoryServices.Protocols.SearchScope.Base
                            };
                        var response = connection.SendRequest(request) as SearchResponse;
                        var entry = response.Entries[0];
                        var match = new { o = (string)entry.Attributes["o"].GetValues(typeof(string))[0] };
                    }
                    watch.Stop();

                    _sdspRunTime = watch.ElapsedMilliseconds;
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
                    sdspThread.Start();
                    while (sdspThread.IsAlive)
                    {
                    }
                    _directoryEntryRunCount++;
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
                                   sb.AppendFormat("Raw S.DS.P time: {0} ms", _sdspRunTime);
                                   sb.AppendLine();
                                   sb.AppendLine("=================================================");
                                   sb.AppendLine();

                                   DirectoryEntryRunText += sb.ToString();
                                   RaisePropertyChanged("DirectoryEntryRunText");
                               }, TaskScheduler.FromCurrentSynchronizationContext());

        }

        private void ConnectionFactoryCompareWork()
        {
            const int totalRuns = 5;
            _singleFactoryRunTime = 0;
            _pooledFactoryRunTime = 0;
            _directoryEntryFactoryRunTime = 0;

            _messenger.Send(new ToggleBusyMessage());
            var singleConfig = new LdapConfiguration();
            singleConfig
                .ConfigureFactory(Server)
                .AuthenticateBy(AuthType.Anonymous);

            var pooledConfig = new LdapConfiguration();
            pooledConfig
                .ConfigurePooledFactory(Server)
                .AuthenticateBy(AuthType.Anonymous);

            //removed multiple threads since something DirectoryEntry uses under the covers isn't thread safe and hangs.
            var singleFactoryThread1 = new Thread(() => ConnectionFactoryWork(singleConfig, 0));
            var pooledFactoryThread1 = new Thread(() => ConnectionFactoryWork(pooledConfig, 0));
            var directoryEntryThread1 = new Thread(() => DirectoryFactoryMultipleWork(0));

            Task.Factory.StartNew(() =>
            {
                singleFactoryThread1.Start();

                while (singleFactoryThread1.IsAlive)
                {
                }

                pooledFactoryThread1.Start();

                while (pooledFactoryThread1.IsAlive)
                {
                }

                directoryEntryThread1.Start();

                while (directoryEntryThread1.IsAlive)
                {
                }
                _factoryRunCount++;
            }, TaskCreationOptions.LongRunning)
                           .ContinueWith(t =>
                           {
                               _messenger.Send(new ToggleBusyMessage());
                               var sb = new StringBuilder();

                               sb.AppendLine("=================================================");
                               sb.AppendFormat("Pooled Factory vs. Standard Factory #{0}", _factoryRunCount);
                               sb.AppendLine();
                               sb.AppendFormat("Pooled Factory time: {0} ms over {1} runs", _pooledFactoryRunTime, totalRuns);
                               sb.AppendLine();
                               sb.AppendFormat("Standard Factory time: {0} ms over {1} runs", _singleFactoryRunTime, totalRuns);
                               sb.AppendLine();
                               sb.AppendFormat("Directory Entry time: {0} ms over {1} runs", _directoryEntryFactoryRunTime, totalRuns);
                               sb.AppendLine();
                               sb.AppendLine("=================================================");
                               sb.AppendLine();

                               FactoryRunText += sb.ToString();
                               RaisePropertyChanged("FactoryRunText");
                           }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void ConnectionFactoryWork(ILdapConfiguration configuration, int sleepTime)
        {
            for (int i = 0; i <= 4; i++)
            {
                var watch = Stopwatch.StartNew();
                using (var context = configuration.CreateContext())
                {
                    var first = context.Query(NamingContext, System.DirectoryServices.Protocols.SearchScope.Base)
                           .WithoutPaging()
                           .FirstOrDefault();
                }
                watch.Stop();
                lock (_factoryLock)
                {
                    if (configuration.ConnectionFactory is LdapConnectionFactory)
                    {
                        _singleFactoryRunTime += watch.ElapsedMilliseconds;
                    }
                    else
                    {
                        _pooledFactoryRunTime += watch.ElapsedMilliseconds;
                    }
                }
                if (sleepTime > 0) Thread.Sleep(sleepTime);
            }
        }

        private void DirectoryFactoryMultipleWork(int sleepTime)
        {
            for (int i = 0; i <= 4; i++)
            {
                var watch = Stopwatch.StartNew();
                using (var entry = new DirectoryEntry(string.Format("LDAP://{0}/{1}", Server, NamingContext)))
                {
                    entry.AuthenticationType = AuthenticationTypes.Anonymous;

                    using (var searcher = new DirectorySearcher(entry))
                    {
                        searcher.SearchScope = SearchScope.Base;
                        var match = searcher.FindOne();
                    }
                }
                watch.Stop();
                lock (_factoryLock)
                {
                    _directoryEntryFactoryRunTime += watch.ElapsedMilliseconds;
                }
                if (sleepTime > 0) Thread.Sleep(sleepTime);
            }
        }

        public override void Cleanup()
        {
            _messenger = null;
            base.Cleanup();
        }
    }
}
