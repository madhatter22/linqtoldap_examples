using System;
using System.Linq;
using System.Web.Http;
using LinqToLdap.Examples.Models;

namespace LinqToLdap.Examples.Mvc.Controllers.API
{
    public class UsersController : ApiController
    {
        private IDirectoryContext _context;

        public UsersController(IDirectoryContext context)
        {
            _context = context;
        }

        public object Get(string q, bool custom, string filter, string nextPage)
        {
            int pageSize = 5;
            var query = _context.Query<User>();

            //if there's a filter we should reuse it and bypass the searching
            if (!string.IsNullOrWhiteSpace(q) && string.IsNullOrWhiteSpace(filter))
            {
                if (custom)
                {
                    //by default filters passed to the Where clause are not cleaned.
                    //if your users don't understand valid filters I would go with fixed search options.
                    query = query.Where(q);
                }
                else
                {
                    //very basic support for searching by first and last name
                    var split = q.Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries);
                    
                    var expression = PredicateBuilder.Create<User>();
                    expression = split.Length == 2 
                        ? expression.And(s => s.FirstName.StartsWith(split[0]) && s.LastName.StartsWith(split[1])) 
                        : split.Aggregate(expression, (current, t) => current.Or(s => s.FirstName.StartsWith(t) || s.LastName.StartsWith(t)));

                    query = query.Where(expression);
                }
            }

            //in order for paging to work correctly the request must be identical each time with the exception of the page size.
            var results = !string.IsNullOrWhiteSpace(nextPage)
                              ? query.Select(s => new {s.DistinguishedName, Name = string.Format("{0} {1}", s.FirstName, s.LastName)})
                                     .ToPage(pageSize, Convert.FromBase64String(nextPage), filter)
                              : query.Select(s => new {s.DistinguishedName, Name = string.Format("{0} {1}", s.FirstName, s.LastName)})
                                     .ToPage(pageSize);

            return new {Items = results.ToList(), NextPage = results.NextPage != null ? Convert.ToBase64String(results.NextPage) : "", results.Filter};
        }
    }
}
