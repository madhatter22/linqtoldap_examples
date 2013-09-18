using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using LinqToLdap.Examples.Models;
using LinqToLdap.Examples.Mvc.App_Start;
using LinqToLdap.Logging;
using LinqToLdap.Mapping;
using SimpleInjector;
using SimpleInjector.Integration.Web.Mvc;

namespace LinqToLdap.Examples.Mvc
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801
    public class MvcApplication : System.Web.HttpApplication
    {
        private const string ConextRequestKey = "ldap.directorycontext";

        protected MvcApplication()
        {
            EndRequest += delegate
                              {
                                  var context =
                                      HttpContext.Current.Items[ConextRequestKey] as IDirectoryContext;

                                  if (context != null) context.Dispose();
                              };
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BootstrapBundleConfig.RegisterBundles();

            var container = new Container();

            container.RegisterSingle<ILdapConfiguration>(() =>
            {
                var config = new LdapConfiguration()
                    .MaxPageSizeIs(500);

                //note the optional parameters available on AddMapping.
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
                      .MinPoolSizeIs(1)
                      .MaxPoolSizeIs(5)
                      .UsePort(389)
                      .ProtocolVersion(3);

                return config;
            });

            //simple session per request
            container.Register<IDirectoryContext>(() => 
                (HttpContext.Current.Items[ConextRequestKey] as IDirectoryContext) ??
                    (HttpContext.Current.Items[ConextRequestKey] = container.GetInstance<ILdapConfiguration>().CreateContext()) as IDirectoryContext);

            // This is an extension method from the integration package.
            container.RegisterMvcControllers(Assembly.GetExecutingAssembly());

            // This is an extension method from the integration package as well.
            container.RegisterMvcAttributeFilterProvider();

            GlobalConfiguration.Configuration.DependencyResolver = new SimpleInjectorWebApiDependencyResolver(container);
            DependencyResolver.SetResolver(new SimpleInjectorDependencyResolver(container));

        }
    }
}