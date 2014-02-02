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
        private const string Server = "ldap.utexas.edu";
        private const string NamingContext = "ou=people,dc=directory,dc=utexas,dc=edu";
        private const int LoopCount = 20;

        private int _directoryEntryRunCount;
        private int _factoryRunCount;
        private long _dynamicRunTime;
        private long _anonymousRunTime;
        private long _classRunTime;
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
            DirectoryEntryCommand = new RelayCommand(DirectoryEntryCompareWork);
            ConnectionPoolCommand = new RelayCommand(ConnectionFactoryCompareWork);
        }

        public string DirectoryEntryRunText { get; set; }
        public string FactoryRunText { get; set; }

        public ICommand DirectoryEntryCommand { get; private set; }
        public ICommand ConnectionPoolCommand { get; private set; }

        private void DirectoryEntryCompareWork()
        {
            //test is pointless without DirectoryEntry comparison

            //_messenger.Send(new ToggleBusyMessage());

            ////paging doesn't work againast open ldap using S.DS
            ////var dirEntryThread = new Thread(() =>
            ////    {
            ////        for (int i = 0; i < LoopCount; i++)
            ////        {
            ////            var watch = Stopwatch.StartNew();
            ////            using (var entry = new DirectoryEntry(string.Format("LDAP://{0}/{1}", Server, NamingContext)))
            ////            {
            ////                entry.AuthenticationType = AuthenticationTypes.Anonymous;
            ////                using (var s = new DirectorySearcher(entry, "!(uid=*)", new[]{"ou", "objectclass"}))
            ////                {
            ////                    s.SearchScope = SearchScope.Subtree;
            ////                    s.PageSize = 10;
            ////                    s.SizeLimit = 10;
            ////                    var result = s.FindOne();
            ////                    var match = new
            ////                                    {
            ////                                        Ou = result.Properties["ou"][0].ToString(),
            ////                                        ObjectClass = result.Properties["objectclass"][0].ToString(),
            ////                                        DistinguishedName = result.Path
            ////                                    };
            ////                }
            ////            }

            ////            watch.Stop();
            ////            _directoryEntryRunTime += watch.ElapsedMilliseconds;
            ////        }
            ////    });

            //var dynamicMappingThread = new Thread(() =>
            //    {
            //        for (int i = 0; i < LoopCount; i++)
            //        {
            //            var watch = Stopwatch.StartNew();
            //            using (var connection = new LdapConnection(Server))
            //            {
            //                connection.SessionOptions.ProtocolVersion = 3;
            //                connection.AuthType = AuthType.Anonymous;

            //                var match = connection.Query(NamingContext, System.DirectoryServices.Protocols.SearchScope.Subtree)
            //                              .Where(_ => !Filter.EqualAnything(_, "uid"))
            //                              .Select(da => new
            //                              {
            //                                  Ou = da.GetString("ou"), 
            //                                  ObjectClass = da.GetString("objectclass"), 
            //                                  da.DistinguishedName
            //                              })
            //                              .FirstOrDefault();
            //            }
            //            watch.Stop();

            //            _dynamicRunTime += watch.ElapsedMilliseconds;
            //        }
            //    });

            //var anonymousMappingThread = new Thread(() =>
            //    {
            //        for (int i = 0; i < LoopCount; i++)
            //        {
            //            var watch = Stopwatch.StartNew();
            //            using (var connection = new LdapConnection(Server))
            //            {
            //                connection.SessionOptions.ProtocolVersion = 3;
            //                connection.AuthType = AuthType.Anonymous;

            //                using (var context = new DirectoryContext(connection))
            //                {
            //                    var example = new { Ou = "", ObjectClass = "", Uid = "", DistinguishedName = "" };
            //                    var match = context.Query(example, System.DirectoryServices.Protocols.SearchScope.Subtree, NamingContext)
            //                        .FirstOrDefault(e => e.Uid == null);
            //                }
            //            }
            //            watch.Stop();
            //            _anonymousRunTime += watch.ElapsedMilliseconds;
            //        }
            //    });

            //var classMappingThread = new Thread(() =>
            //{
            //    for (int i = 0; i < LoopCount; i++)
            //    {
            //        var watch = Stopwatch.StartNew();
            //        using (var connection = new LdapConnection(Server))
            //        {
            //            connection.SessionOptions.ProtocolVersion = 3;
            //            connection.AuthType = AuthType.Anonymous;

            //            using (var context = new DirectoryContext(connection))
            //            {
            //                var match = context.Query<SimpleClass>(System.DirectoryServices.Protocols.SearchScope.Subtree, NamingContext)
            //                    .FirstOrDefault(e => e.Uid != null);
            //            }
            //        }
            //        watch.Stop();
            //        _classRunTime += watch.ElapsedMilliseconds;
            //    }
            //});

            //var sdspThread = new Thread(() =>
            //    {
            //        for (int i = 0; i < LoopCount; i++)
            //        {
            //            var watch = Stopwatch.StartNew();
            //            using (var connection = new LdapConnection(Server))
            //            {
            //                connection.SessionOptions.ProtocolVersion = 3;
            //                connection.AuthType = AuthType.Anonymous;

            //                var request = new SearchRequest
            //                {
            //                    Filter = "!(uid=*)",
            //                    DistinguishedName = NamingContext,
            //                    Scope = System.DirectoryServices.Protocols.SearchScope.Subtree,
            //                };
            //                request.Attributes.AddRange(new[] { "ou", "objectclass" });
            //                request.Controls.Add(new PageResultRequestControl(1));
            //                var response = connection.SendRequest(request) as SearchResponse;
            //                var entry = response.Entries[0];
            //                var match = new
            //                                {
            //                                    Ou = (string) entry.Attributes["ou"].GetValues(typeof (string))[0],
            //                                    ObjectClass = (string) entry.Attributes["objectclass"].GetValues(typeof (string))[0], 
            //                                    entry.DistinguishedName
            //                                };
            //            }
            //            watch.Stop();
            //            _sdspRunTime += watch.ElapsedMilliseconds;
            //        }
            //    });

            //Task.Factory.StartNew(() =>
            //    {
            //        _dynamicRunTime = 0;
            //        _anonymousRunTime = 0;
            //        _sdspRunTime = 0;
            //        _classRunTime = 0;
            //        _directoryEntryRunCount++;
                    
            //        sdspThread.Start();
            //        while (sdspThread.IsAlive)
            //        {
            //        }
                    
            //        dynamicMappingThread.Start();
            //        while (dynamicMappingThread.IsAlive)
            //        {
            //        }
                    
            //        anonymousMappingThread.Start();
            //        while (anonymousMappingThread.IsAlive)
            //        {
            //        }

            //        classMappingThread.Start();
            //        while (classMappingThread.IsAlive)
            //        {
            //        }
            //    }, TaskCreationOptions.LongRunning)
            //               .ContinueWith(t =>
            //                   {
            //                       _messenger.Send(new ToggleBusyMessage());
            //                       var sb = new StringBuilder();

            //                       sb.AppendLine("=================================================");
            //                       sb.AppendFormat("LINQ to LDAP Run #{0}", _directoryEntryRunCount);
            //                       sb.AppendLine();
            //                       if (t.Exception != null)
            //                       {
            //                           _messenger.Send(new ErrorMessage(t.Exception));
            //                           sb.AppendLine("Error");
            //                       }
            //                       else
            //                       {
            //                           sb.AppendFormat("Raw S.DS.P time: {0} ms averaged over {1} runs", _sdspRunTime / LoopCount, LoopCount);
            //                           sb.AppendLine();
            //                           sb.AppendFormat("LINQ to LDAP dynamic time: {0} ms averaged over {1} runs", _dynamicRunTime / LoopCount, LoopCount);
            //                           sb.AppendLine();
            //                           sb.AppendFormat("LINQ to LDAP anonymous time: {0} ms averaged over {1} runs", _anonymousRunTime / LoopCount, LoopCount);
            //                           sb.AppendLine();
            //                           sb.AppendFormat("LINQ to LDAP class time: {0} ms averaged over {1} runs", _classRunTime / LoopCount, LoopCount);
            //                       }
            //                       sb.AppendLine();
            //                       sb.AppendLine("=================================================");
            //                       sb.AppendLine();

            //                       DirectoryEntryRunText += sb.ToString();
            //                       RaisePropertyChanged("DirectoryEntryRunText");
            //                   }, TaskScheduler.FromCurrentSynchronizationContext());

        }

        private void ConnectionFactoryCompareWork()
        {
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
                _factoryRunCount++;
                singleFactoryThread1.Start();
                while (singleFactoryThread1.IsAlive)
                {
                }
                Thread.Sleep(1000);
                pooledFactoryThread1.Start();
                while (pooledFactoryThread1.IsAlive)
                {
                }
                Thread.Sleep(1000);
                directoryEntryThread1.Start();
                while (directoryEntryThread1.IsAlive)
                {
                }
            }, TaskCreationOptions.LongRunning)
                           .ContinueWith(t =>
                           {
                               _messenger.Send(new ToggleBusyMessage());
                               var sb = new StringBuilder();

                               sb.AppendLine("=================================================");
                               sb.AppendFormat("Pooled Factory vs. Standard Factory #{0}", _factoryRunCount);
                               sb.AppendLine();

                               if (t.Exception != null)
                               {
                                   _messenger.Send(new ErrorMessage(t.Exception));
                                   sb.AppendLine("Error");
                               }
                               else
                               {
                                   sb.AppendFormat("Pooled Factory time: {0} ms averaged over {1} runs", _pooledFactoryRunTime / LoopCount, LoopCount);
                                   sb.AppendLine();
                                   sb.AppendFormat("Standard Factory time: {0} ms averaged over {1} runs", _singleFactoryRunTime / LoopCount, LoopCount);
                                   sb.AppendLine();
                                   sb.AppendFormat("Directory Entry time: {0} ms averaged over {1} runs", _directoryEntryFactoryRunTime / LoopCount, LoopCount);
                               }

                               sb.AppendLine();
                               sb.AppendLine("=================================================");
                               sb.AppendLine();

                               FactoryRunText += sb.ToString();
                               RaisePropertyChanged("FactoryRunText");
                           }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void ConnectionFactoryWork(ILdapConfiguration configuration, int sleepTime)
        {
            for (int i = 0; i < LoopCount; i++)
            {
                var watch = Stopwatch.StartNew();
                using (var context = configuration.CreateContext())
                {
                    var match = context.Query(NamingContext, System.DirectoryServices.Protocols.SearchScope.Base)
                                       .Select("ou", "objectclass")
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
            for (int i = 0; i < LoopCount; i++)
            {
                var watch = Stopwatch.StartNew();
                using (var entry = new DirectoryEntry(string.Format("LDAP://{0}/{1}", Server, NamingContext)))
                {
                    entry.AuthenticationType = AuthenticationTypes.Anonymous;

                    using (var searcher = new DirectorySearcher(entry))
                    {
                        searcher.PropertiesToLoad.AddRange(new[]{"ou", "objectclass"});
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

        private class SimpleClass
        {
            public string DistinguishedName { get; set; }
            public string Ou { get; set; }
            public string Uid { get; set; }
        }
    }
}
