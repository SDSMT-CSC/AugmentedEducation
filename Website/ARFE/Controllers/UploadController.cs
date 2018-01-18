using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QRCoder;

namespace ARFE.Controllers
{
    public class UploadController : Controller
    {
        // GET: Upload  
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public ActionResult UploadFile()
        {
            return View();
        }

        [HttpPost]
        public ActionResult UploadFile(HttpPostedFileBase file)
        {
            try
            {
                if (file.ContentLength > 0)
                {
                    string _FileName = Path.GetFileName(file.FileName);
                    int index = _FileName.LastIndexOf(".");
                    string noExtension = _FileName.Substring(0, index);
                    string fbxExtension = noExtension + ".fbx";

                    string _path = Path.Combine(Server.MapPath("~/UploadedFiles"), _FileName);
                    file.SaveAs(_path);

                    Process process = new System.Diagnostics.Process();
                    process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    process.StartInfo.FileName = Server.MapPath("~/Content/FileConversion.exe");
                    process.StartInfo.Arguments = Server.MapPath("~/UploadedFiles/" + _FileName);
                    process.Start();
                    process.Close();

                    System.Threading.Thread.Sleep(5000);

                    String FilePath = AppDomain.CurrentDomain.BaseDirectory + "\\UploadedFiles\\" + fbxExtension;
                    System.Web.HttpResponse response = System.Web.HttpContext.Current.Response;
                    response.ClearContent();
                    response.Clear();
                    response.ContentType = "application/octet-stream";
                    response.AddHeader("Content-Disposition", "attachment; filename=" + fbxExtension + ";");
                    response.TransmitFile(FilePath);
                    response.Flush();
                    response.End();

                }
                ViewBag.Message = "File Uploaded Successfully!!";
                return View();
            }
            catch
            {
                ViewBag.Message = "File upload failed!!";
                return View();
            }
        }

        public Bitmap GenerateQRCode(String address)
        {

            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(address, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(20);
            return qrCodeImage;

        }

    }
}