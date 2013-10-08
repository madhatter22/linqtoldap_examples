using LinqToLdap.Mapping;

namespace LinqToLdap.Examples.Models
{
    public abstract class DirectoryObject
    {
        [DistinguishedName]
        public string DistinguishedName { get; set; }
    }
}
