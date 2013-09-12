using LinqToLdap.Mapping;

namespace LinqToLdap.Examples.Models
{
    [DirectorySchema("OU=users,DC=testathon,DC=net", ObjectClass = "inetOrgPerson")]
    public class User : DirectoryObject
    {
        //All attributes marked as read only to 

        [DirectoryAttribute("cn", ReadOnly = true)]
        public string CommonName { get; set; }

        [DirectoryAttribute("uid", ReadOnly = true)]
        public string UserId { get; set; }

        [DirectoryAttribute("givenname", ReadOnly = true)]
        public string FirstName { get; set; }

        [DirectoryAttribute("sn", ReadOnly = true)]
        public string LastName { get; set; }

        [DirectoryAttribute(ReadOnly = true)]
        public string TelephoneNumber { get; set; }
    }
}
