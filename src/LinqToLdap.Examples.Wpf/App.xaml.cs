using System;
using System.DirectoryServices.Protocols;
using System.Windows;
using System.Windows.Threading;
using GalaSoft.MvvmLight.Messaging;
using LinqToLdap.Examples.Models;
using LinqToLdap.Examples.Wpf.Helpers;
using LinqToLdap.Examples.Wpf.Messages;
using LinqToLdap.Examples.Wpf.ViewModels;
using LinqToLdap.Examples.Wpf.Views;
using LinqToLdap.Logging;
using LinqToLdap.Mapping;
using SimpleInjector;

namespace LinqToLdap.Examples.Wpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static Container Container { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            this.DispatcherUnhandledException += OnDispatcherUnhandledException;

            base.OnStartup(e);
        }

        private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs args)
        {
            Messenger.Default.Send(new ErrorMessage(args.Exception));
        }

        private void CreateContainer(object sender, StartupEventArgs e)
        {
            var container = new Container();
            container.RegisterSingle(() => new CustomTextLogger(Console.Out));

            container.RegisterSingle<IMessenger>(() => Messenger.Default);

            container.RegisterSingle<ILdapConfiguration>(() =>
            {
                var config = new LdapConfiguration()
                    .MaxPageSizeIs(500)
                    .LogTo(container.GetInstance<CustomTextLogger>());

                //note the optional parameters on AddMapping.
                //We can perform "late" mapping on certain values, 
                //even for auto and attribute based mapping.
                config.AddMapping(new AttributeClassMap<User>())
                    .AddMapping(new OrganizationalUnitMap());

                config.ConfigurePooledFactory("ldap.testathon.net")
                      .AuthenticateBy(AuthType.Basic)
                      .AuthenticateAs(
                          new System.Net.NetworkCredential(
                              "CN=stuart,OU=Users,DC=testathon,DC=net",
                              "stuart"))
                      .MinPoolSizeIs(0)
                      .MaxPoolSizeIs(5)
                      .UsePort(389)
                      .ProtocolVersion(3);

                return config;
            });

            container.Register<IDirectoryContext>(() => new DirectoryContext(container.GetInstance<ILdapConfiguration>()));

            Container = container;
            var view = new MainView(new MainViewModel());
            view.Show();
        }
    }
}
