using LinqToLdap.Mapping;

namespace LinqToLdap.Examples.Models
{
    [DirectorySchema("dc=directory,dc=utexas,dc=edu", ObjectClass = "inetOrgPerson")]
    public class User : DirectoryObject
    {
        [DirectoryAttribute("cn", ReadOnly = true)]
        public string CommonName { get; set; }

        [DirectoryAttribute("uid", ReadOnly = true)]
        public string UserId { get; set; }

        [DirectoryAttribute("utexasedupersonprimarypubaffiliation", ReadOnly = true)]
        public string PrimaryAffiliation { get; set; }

        [DirectoryAttribute("givenname", ReadOnly = true)]
        public string FirstName { get; set; }

        [DirectoryAttribute("sn", ReadOnly = true)]
        public string LastName { get; set; }

        [DirectoryAttribute]
        public string[] ObjectClasses { get; set; }

    }
}
