﻿using System.Web.Mvc;

/// <summary>
/// This namespaces is a sub-namespace of the ARFE project namespace specifically
/// for the ASP.NET Controllers.
/// </summary>
namespace ARFE.Controllers
{
    /// <summary>
    /// A class derived from the <see cref="Controller"/> class that has all
    /// of the controller actions to dispaly the website's help pages.
    /// </summary>
    public class HelpController : Controller
    {
        /// <summary>
        /// The Index controller action is the default action called when the UserContent page is
        /// browsed to.
        /// </summary>
        /// <returns>
        ///     A view to the "Index.cshtml" page in the Views/Help/ folder.
        /// </returns>
        public ActionResult Help() => View();
    }
}