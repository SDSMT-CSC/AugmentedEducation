using System.Web.Mvc;
using System.Web.Routing;

/// <summary>
/// This is the over-arching namespace for all website related code.
/// </summary>
namespace ARFE
{
    /// <summary>
    /// This class is for defining the valid url rout patterns for the application.
    /// </summary>
    public class RouteConfig
    {
        /// <summary>
        /// The configuration for url route patterns used within the application.
        /// </summary>
        /// <param name="routes">The collection of acceptable url route patterns.</param>
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "PublicContent", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
