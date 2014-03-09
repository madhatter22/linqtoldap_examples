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
using SimpleInjector.Integration.Web.Mvc;

namespace LinqToLdap.Examples.Mvc
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801
    public class MvcApplication : HttpApplication
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

            container.RegisterSingle<ILinqToLdapLogger>(new SimpleTextLogger());

            container.RegisterSingle<ILdapConfiguration>(() =>
            {
                var config = new LdapConfiguration()
                    .MaxPageSizeIs(500)
                    .LogTo(container.GetInstance<ILinqToLdapLogger>());

                //Note the optional parameters available on AddMapping.
                //We can perform "late" mapping on certain values, 
                //even for auto and attribute based mapping.
                config.AddMapping(new OrganizationalUnitMap())
                      .AddMapping(new AttributeClassMap<User>());

                // I explicitly mapped User, but I can also let it 
                // get mapped the first time we query for users.
                // This only applies to auto and attribute-based mapping.

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

            //simple context per request only when requested
            container.Register<IDirectoryContext>(() => 
                (HttpContext.Current.Items[ConextRequestKey] as IDirectoryContext) ??
                    (HttpContext.Current.Items[ConextRequestKey] = new DirectoryContext(container.GetInstance<ILdapConfiguration>())) as IDirectoryContext);

            container.RegisterMvcControllers(Assembly.GetExecutingAssembly());
            container.RegisterMvcAttributeFilterProvider();

            GlobalConfiguration.Configuration.DependencyResolver = new SimpleInjectorWebApiDependencyResolver(container);
            DependencyResolver.SetResolver(new SimpleInjectorDependencyResolver(container));
        }
    }
}