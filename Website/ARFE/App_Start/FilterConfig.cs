using System.Web.Mvc;

namespace ARFE
{
    /// <summary>
    /// A class for defining filtered attributes to the ASP.NET web request and response
    /// communication.  For example, the [RequireHttps] filter could be applied here
    /// to demand all traffic is only allowed if is transferred over SSL.
    /// </summary>
    public class FilterConfig
    {
        /// <summary>
        /// Register the filters to the ASP.NET application
        /// </summary>
        /// <param name="filters"> The collection of filters to add to the application. </param>
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
