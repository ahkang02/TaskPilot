using System.Web;
using System.Web.Optimization;

namespace TaskManagementApp
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new Bundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.bundle.js",
                      "~/Scripts/DataTables/jquery.dataTables.js",
                      "~/Scripts/DataTables/dataTables.select.js",
                      "~/Scripts/moment.js",
                      "~/Scripts/app.min.js",
                      "~/Scripts/sidebarmenu.js",
                      "~/Scripts/DataTables/dataTables.buttons.js",
                      "~/Scripts/DataTables/dataTables.responsive.js",
                      "~/Scripts/DataTables/responsive.bootstrap4.js",
                      "~/Scripts/DataTables/dataTables.bootstrap4.js",
                      "~/Scripts/DataTables/buttons.html5.js",
                      "~/Scripts/DataTables/buttons.print.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/Site.css",
                      "~/Content/DataTables/css/dataTables.bootstrap4.css",
                      "~/Content/DataTables/css/select.bootstrap.css",
                      "~/Content/DataTables/css/dataTables.jqueryui.css",
                      "~/Content/style.min.css",
                      "~/Content/DataTables/css/responsive.bootstrap4.css",
                      "~/Content/DataTables/css/buttons.dataTables.css"));
        }
    }
}
