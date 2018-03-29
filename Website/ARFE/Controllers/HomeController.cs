using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ARFE.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var publicContent = new PublicContentController();
            return publicContent.Index();
        }
    }
}