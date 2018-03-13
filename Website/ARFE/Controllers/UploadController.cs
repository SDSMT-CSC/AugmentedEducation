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
using System.Threading;

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
        public ActionResult UploadFile(HttpPostedFileBase file, bool publicFile)
        {
            string uploadMessage = "File Uploaded Successfully.";
            if (file.ContentLength > 0)
            {
                string basePath = Server.MapPath("~/UploadedFiles");
                string fileName = Path.GetFileName(file.FileName);
                string fileNameExtention = fileName.Substring(fileName.LastIndexOf('.'));
                string fileNameWithoutExtension = fileName.Replace(fileNameExtention, "");

                try
                {
                    while (System.IO.File.Exists(Path.Combine(basePath, fileName)))
                    {   //remove other file if not in use
                        try
                        {
                            System.IO.File.Delete(Path.Combine(basePath, fileName));
                        }
                        //sleep 1.5 seconds - don't waste resources just looping
                        catch { Thread.Sleep(50); }
                    }

                    //save file as uniqueName
                    file.SaveAs(Path.Combine(basePath, fileName));
                    BlobManager blobManager = new BlobManager();

                    //uploaded file is fbx
                    if (fileNameExtention == ".fbx")
                    {   //upload and delete
                        if(publicFile)
                        {
                            blobManager.UploadBlobToPublicContainer(User.Identity.Name, fileName, basePath);
                        }
                        else
                        {
                            blobManager.UploadBlobToUserContainer(User.Identity.Name, fileName, basePath);
                        }
                        System.IO.File.Delete(Path.Combine(basePath, fileName));
                    }
                    //uploaded file is not fbx -- convert
                    else
                    {
                        FileConverter converter = new FileConverter("UploadedFiles", "UploadedFiles");

                        if (converter.ConvertToFBX(fileName))
                        {   //convert, upload, delete converted, delete original
                            if (publicFile)
                            {
                                blobManager.UploadBlobToPublicContainer(User.Identity.Name, $"{fileNameWithoutExtension}.fbx", basePath);
                            }
                            else
                            {
                                blobManager.UploadBlobToUserContainer(User.Identity.Name, $"{fileNameWithoutExtension}.fbx", basePath);
                            }
                            System.IO.File.Delete(Path.Combine(basePath, $"{fileNameWithoutExtension}.fbx"));
                            System.IO.File.Delete(Path.Combine(basePath, fileName));
                        }
                        else
                        {   //didn't convert - delete from how was originally saved
                            uploadMessage = "Common file type conversion failed.";
                            System.IO.File.Delete(Path.Combine(basePath, fileName));
                        }
                    }
                }
                catch
                {
                    uploadMessage = "File Upload Failed.";
                    if (System.IO.File.Exists(Path.Combine(basePath, fileName)))
                    {
                        System.IO.File.Delete(Path.Combine(basePath, fileName));
                    }
                    if (System.IO.File.Exists(Path.Combine(basePath, $"{fileNameWithoutExtension}.fbx")))
                    {
                        System.IO.File.Delete(Path.Combine(basePath, $"{fileNameWithoutExtension}.fbx"));
                    }
                }
            }

            ViewBag.Message = uploadMessage;
            return View();
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
    }
}