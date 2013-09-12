using System.Collections.Generic;
using LinqToLdap.Mapping;

namespace LinqToLdap.Examples.Models
{
    public class OrganizationalUnitMap : ClassMap<OrganizationalUnit>
    {
        public override IClassMap PerformMapping(string namingContext = null, string objectCategory = null, bool includeObjectCategory = true,
                                                 IEnumerable<string> objectClasses = null, bool includeObjectClasses = true)
        {
            //you can ignore the passed in parameters if you want.
            //the parameters are there in case you want to do "late" mapping during configuration.

            NamingContext("OU=users,DC=testathon,DC=net");
            ObjectClasses(new[] { "organizationalUnit" }, includeObjectClasses: true);

            DistinguishedName(ou => ou.DistinguishedName);

            Map(ou => ou.Name)
                .Named("ou")
                .ReadOnly();

            return this;
        }
    }
}
