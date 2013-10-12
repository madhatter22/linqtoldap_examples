using System.Web.Mvc;
using LinqToLdap.Logging;

namespace LinqToLdap.Examples.Mvc.Controllers
{
    public class HomeController : Controller
    {
        private SimpleTextLogger _logger;

        public HomeController(SimpleTextLogger logger)
        {
            _logger = logger;
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult ServerInfo()
        {
            return View();
        }

        public ActionResult Users()
        {
            return View();
        }

        [HttpPost]
        public JsonResult TraceEnabled(bool traceEnabled)
        {
            _logger.TraceEnabled = traceEnabled;
            return Json(new {success = true});
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            ViewBag.TraceEnabled = _logger.TraceEnabled;
            base.OnActionExecuting(filterContext);
        }
    }
}
