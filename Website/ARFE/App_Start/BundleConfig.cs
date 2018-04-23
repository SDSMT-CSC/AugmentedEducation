using System.Web.Optimization;

/// <summary>
/// This is the over-arching namespace for all website related code.
/// </summary>
namespace ARFE
{
    /// <summary>
    /// Configure all of the javascript and css include bundles.  Register
    /// these with the entire application for simple inclusion in the .cshtml files.
    /// </summary>
    public class BundleConfig
    {
        /// <summary>
        /// The collection of script bundles to register with the application.
        /// For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        /// </summary>
        /// <param name="bundles"></param>
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

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css"));
        }
    }
}
