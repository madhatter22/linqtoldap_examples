using System.Web.Optimization;

namespace LinqToLdap.Examples.Mvc.App_Start
{
	public class BootstrapBundleConfig
	{
		public static void RegisterBundles()
		{
            BundleTable.Bundles.Add(new StyleBundle("~/Content/site").Include("~/Content/site.css"));
			BundleTable.Bundles.Add(new StyleBundle("~/Content/bootstrap/base").Include("~/Content/bootstrap/bootstrap.css"));
            BundleTable.Bundles.Add(new StyleBundle("~/Content/bootstrap/responsive").Include("~/Content/bootstrap/bootstrap-responsive.css"));
            BundleTable.Bundles.Add(new StyleBundle("~/Content/alertify/core").Include("~/Content/alertify/alertify.core.css"));
            BundleTable.Bundles.Add(new StyleBundle("~/Content/alertify/theme").Include("~/Content/alertify/alertify.default.css"));

            BundleTable.Bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include("~/Scripts/bootstrap.js"));
            BundleTable.Bundles.Add(new ScriptBundle("~/bundles/jquery").Include("~/Scripts/jquery-{version}.js"));
            BundleTable.Bundles.Add(new ScriptBundle("~/bundles/knockout").Include("~/Scripts/knockout-{version}.js"));
            BundleTable.Bundles.Add(new ScriptBundle("~/bundles/alertify").Include("~/Scripts/alertify/alertify.js"));

            BundleTable.Bundles.Add(new ScriptBundle("~/bundles/linqtoldap").Include(
                "~/Scripts/app/framework.js",
                "~/Scripts/app/serverInfoViewModel.js",
                "~/Scripts/app/usersViewModel.js"));

            //// Use the development version of Modernizr to develop with and learn from. Then, when you're
            //// ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            //bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
            //            "~/Scripts/modernizr-*"));
		}
	}
}
