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
        [Authorize]
        // GET: Upload  
        public ActionResult Index()
        {
            return View();
        }

        [Authorize]
        [HttpGet]
        public ActionResult UploadFile()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public ActionResult UploadFile(HttpPostedFileBase BaseFile, HttpPostedFileBase MatFile, bool publicFile, string AltFileName, string FileDescription)
        {
            string uploadMessage = "File Uploaded Successfully.";
            if (BaseFile.ContentLength > 0)
            {
                string basePath = Server.MapPath("~/UploadedFiles");
                string fileName = Path.GetFileName(BaseFile.FileName);
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
                    BaseFile.SaveAs(Path.Combine(basePath, fileName));
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
                            uploadMessage = "Upload Failed. Accepted file types: FBX, DAE, OBJ, STL, PLY.";
                            System.IO.File.Delete(Path.Combine(basePath, fileName));
                        }
                    }
                }
                catch
                {
                    uploadMessage = "Account error, please contact administrator.";
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

        public ActionResult OverWriteUpload(string overWrite, string filename)
        {
            if(overWrite == "Yes")
            {
                
            }
            else
            {

            }

            return View();
        }


    }
}