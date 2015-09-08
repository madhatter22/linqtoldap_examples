using System;
using System.DirectoryServices.Protocols;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using LinqToLdap.Examples.Models;
using LinqToLdap.Examples.Mvc.App_Start;
using LinqToLdap.Logging;
using LinqToLdap.Mapping;
using SimpleInjector;
using SimpleInjector.Integration.Web;
using SimpleInjector.Integration.Web.Mvc;
using SimpleInjector.Integration.WebApi;

namespace LinqToLdap.Examples.Mvc
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801
    public class MvcApplication : HttpApplication
    {
        private const string ConextRequestKey = "ldap.directorycontext";

        protected MvcApplication()
        {
        }

        protected void Application_Error()
        {
            var exception = Server.GetLastError();
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BootstrapBundleConfig.RegisterBundles();

            var container = new Container();

            container.Options.DefaultScopedLifestyle = new WebRequestLifestyle();

            container.RegisterSingleton<ILinqToLdapLogger>(new SimpleTextLogger());

            container.RegisterSingleton<ILdapConfiguration>(() =>
            {
                var config = new LdapConfiguration()
                    .MaxPageSizeIs(50)
                    .LogTo(container.GetInstance<ILinqToLdapLogger>());

                //Note the optional parameters available on AddMapping.
                //We can perform "late" mapping on certain values, 
                //even for auto and attribute based mapping.
                config.AddMapping(new OrganizationalUnitMap())
                      .AddMapping(new AttributeClassMap<User>());

                // I explicitly mapped User, but I can also let it 
                // get mapped the first time we query for users.
                // This only applies to auto and attribute-based mapping.

                config.ConfigurePooledFactory("directory.utexas.edu")
                      .AuthenticateBy(AuthType.Anonymous)
                      .MinPoolSizeIs(0)
                      .MaxPoolSizeIs(5)
                      .UsePort(389)
                      .ProtocolVersion(3);

                return config;
            });

            //simple context per request only when requested
            container.Register<IDirectoryContext>(() => container.GetInstance<ILdapConfiguration>().CreateContext(), Lifestyle.Scoped);

            container.RegisterWebApiControllers(GlobalConfiguration.Configuration);
            container.RegisterMvcControllers(Assembly.GetExecutingAssembly());
            

            GlobalConfiguration.Configuration.DependencyResolver = new SimpleInjectorWebApiDependencyResolver(container);
            DependencyResolver.SetResolver(new SimpleInjectorDependencyResolver(container));
            
            container.Verify();
        }
    }
}