using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LinqToLdap.Examples.Mvc.Models
{
    public class UserModel
    {
        public string CommonName { get; set; }
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string TelephoneNumber { get; set; }
    }
}