using System.Web.Mvc;

namespace LinqToLdap.Examples.Mvc.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/

        public ActionResult About()
        {
            return View();
        }

        public ActionResult ServerInfo()
        {
            return View();
        }
    }
}
