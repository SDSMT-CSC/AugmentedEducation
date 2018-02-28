using System;
using System.IO;
using System.Web;
using System.Linq;
using System.Drawing;
using System.Web.Mvc;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Collections.Generic;

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
                    string fileName = Path.GetFileName(file.FileName);
                    string noExtension = fileName.Substring(0, fileName.LastIndexOf("."));
                    file.SaveAs(Path.Combine(Server.MapPath("~/UploadedFiles"), fileName));

                    FileConverter converter = new FileConverter("UploadedFiles", "UploadedFiles");

                    if (converter.ConvertToFBX(fileName))
                    {
                        BlobManager blobManager = new BlobManager();
                        blobManager.UploadBlobToUserContainer(User.Identity.Name, $"{noExtension}.fbx", Server.MapPath("~/UploadedFiles"));
                    }
                    else
                    {
                        ViewBag.Message = "Common file type conversion failed.";
                        return View();
                    }
                }
                ViewBag.Message = "File Uploaded Successfully.";
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Message = "File Upload Failed.";
                return View();
            }
        }

        public ActionResult GetMessage()
        {
            return View();
        }

        public ActionResult DisplayQR(string Message)
        {

            using (MemoryStream ms = new MemoryStream())
            {
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(Message, QRCodeGenerator.ECCLevel.Q);
                QRCode qrCode = new QRCode(qrCodeData);
                using (Bitmap bitmap = qrCode.GetGraphic(20))
                {
                    bitmap.Save(ms, ImageFormat.Png);
                    ViewBag.QRCodeImage = "data:image/png;base64," + Convert.ToBase64String(ms.ToArray());
                }
            }
            return View();
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