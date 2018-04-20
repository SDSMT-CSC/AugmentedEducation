using System.Web.Mvc;

/// <summary>
/// This namespaces is a sub-namespace of the ARFE project namespace specifically
/// for the ASP.NET Controllers.
/// </summary>
namespace ARFE.Controllers
{
    public class DownloadController : Controller
    {
        // GET: Download
        public ActionResult Index()
        {
            return View();
        }

        //public ActionResult Download()
        //{
        //    return View();
        //}


        public ActionResult Download()
        {

            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.FileName = Server.MapPath("~/Content/FileConversion.exe");
            process.StartInfo.Arguments = Server.MapPath("~/UploadedFiles/sphere.obj");
            process.Start();
            process.Close();


            //String FileName = "sphere.obj";
            //String FilePath = AppDomain.CurrentDomain.BaseDirectory + "\\UploadedFiles\\sphere.obj";
            //System.Web.HttpResponse response = System.Web.HttpContext.Current.Response;
            //response.ClearContent();
            //response.Clear();
            //response.ContentType = "application/octet-stream";
            //response.AddHeader("Content-Disposition", "attachment; filename=" + FileName + ";");
            //response.TransmitFile(FilePath);
            //response.Flush();
            //response.End();

            return null;

        }
    }
}