﻿using System;
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
                    string _FileName = Path.GetFileName(file.FileName);
                    int index = _FileName.LastIndexOf(".");
                    string noExtension = _FileName.Substring(0, index);
                    string fbxExtension = noExtension + ".fbx";

                    string _path = Path.Combine(Server.MapPath("~/UploadedFiles"), _FileName);
                    file.SaveAs(_path);

                    Process process = new Process();
                    process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    process.StartInfo.FileName = Server.MapPath("~/Content/FileConversion.exe");
                    process.StartInfo.Arguments = Server.MapPath("~/UploadedFiles/" + _FileName);
                    process.Start();
                    process.Close();

                    BlobsController blobsController = new BlobsController();
                    blobsController.UploadBlobToContainer(User.Identity.Name, fbxExtension, Server.MapPath("~/UploadedFiles"));

                    System.Threading.Thread.Sleep(2000);

                    var l = blobsController.ListBlobNamesInContainer(User.Identity.Name);

#warning remove this line - just for testing download functionality
                    return blobsController.DownloadBlobFromContainer(User.Identity.Name, fbxExtension);

                }
                ViewBag.Message = "File Uploaded Successfully!!";
                return View();
            }
            catch(Exception ex)
            {
                ViewBag.Message = "File upload failed!!";
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