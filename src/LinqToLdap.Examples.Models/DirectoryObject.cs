using LinqToLdap.Mapping;

namespace LinqToLdap.Examples.Models
{
    public class DirectoryObject : DirectoryObjectBase
    {
        [DistinguishedName]
        public string DistinguishedName { get; set; }
    }
}
