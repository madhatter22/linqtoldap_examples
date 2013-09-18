using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace LinqToLdap.Examples.Mvc.Controllers.API
{
    public class ServerInfoController : ApiController
    {
        private IDirectoryContext _context;

        public ServerInfoController(IDirectoryContext context)
        {
            _context = context;
        }

        // GET api/<controller>
        public IEnumerable<object> Get()
        {
            return _context.ListServerAttributes("altServer", "objectClass", "namingContexts",
                                                                             "supportedControl", "supportedExtension",
                                                                             "supportedLDAPVersion",
                                                                             "supportedSASLMechanisms", "vendorName",
                                                                             "vendorVersion",
                                                                             "supportedAuthPasswordSchemes")
                    .Select(kvp =>
                                {
                                    if (kvp.Value is string)
                                    {
                                        return new {kvp.Key, Value = kvp.Value.ToString()};
                                    }
                                    if (kvp.Value is IEnumerable<string>)
                                    {
                                        return new { kvp.Key, Value = string.Join(", ", kvp.Value as IEnumerable<string>) };
                                    }
                                    if (kvp.Value is IEnumerable<byte>)
                                    {
                                        return new { kvp.Key, Value = string.Join(", ", (kvp.Value as IEnumerable<byte>)) };
                                    }
                                    if (kvp.Value is IEnumerable<byte[]>)
                                    {
                                        return
                                            new
                                                {
                                                    kvp.Key,
                                                    Value =
                                                        string.Join(", ",
                                                                    (kvp.Value as IEnumerable<byte[]>).Select(
                                                                        b => string.Format("({0})", string.Join(", ", b))))
                                                };

                                    }

                                    return new { kvp.Key, Value = kvp.Value == null ? "" : kvp.Value.ToString() };
                                })
                .ToArray();

            //under the covers this actually executes a query that looks something like this:
            //
            //_context.Query(null, SearchScope.Base)
            //        .Where("(objectClass=*)")
            //        .Select("namingContexts", "supportedControl", "supportedExtension", "supportedLDAPVersion",
            //                "supportedSASLMechanisms", "vendorName", "vendorVersion", "supportedAuthPasswordSchemes")
            //        .FirstOrDefault();
            //
            //null or empty naming contexts are not supported using the normal Query method.
        }
    }
}