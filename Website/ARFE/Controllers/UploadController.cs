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
        #region Members

        private static string s_BasePath;

        #endregion

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
            string subDir = string.Empty;
            s_BasePath = Server.MapPath("~/UploadedFiles");
            string uploadMessage = "File Uploaded Successfully.";
            UploadedFileCache uploadedFiles = UploadedFileCache.GetInstance(s_BasePath);

            if (BaseFile.ContentLength > 0)
            {
                string matFileName = string.Empty;
                string fileName = Path.GetFileName(BaseFile.FileName);
                string fileExt = fileName.Substring(fileName.LastIndexOf('.'));

                if (MatFile != null)
                    matFileName = Path.GetFileName(MatFile.FileName);

                try
                {
                    if (uploadedFiles.SaveFile(BaseFile, User.Identity.Name, fileName, FileDescription, publicFile))
                    {
                        if (!string.IsNullOrEmpty(matFileName))
                            if (!uploadedFiles.SaveFile(MatFile, User.Identity.Name, matFileName, FileDescription, publicFile))
                            { /*ViewBag message*/ }

                        BlobManager blobManager = new BlobManager();
                        subDir = uploadedFiles.FindContainingFolderGUIDForFile(User.Identity.Name, fileName).ToString();

                        if (fileExt == ".fbx")
                        {   //no conversion necessary - upload
                            UploadToBlob(fileName, subDir, FileDescription, publicFile);
                        }
                        else
                        {   //uploaded file is not fbx -- convert
                            ConvertAndUploadToBlob(fileName, AltFileName, subDir, FileDescription, publicFile);
                        }
                    }
                    else { /*ViewBag message*/ }
                }
                catch { uploadMessage = "Account error, please contact administrator."; }

                //if file wasn't saved - does nothing, 
                //else marks subdirectory containing file for deletion
                uploadedFiles.MarkForDelete(User.Identity.Name, fileName);
            }

            ViewBag.Message = uploadMessage;
            return View();
        }

        public ActionResult OverWriteUpload(string overWrite, string fileName)
        {
            string userName = User.Identity.Name;
            UploadedFileCache uploadedFiles = UploadedFileCache.GetInstance(s_BasePath);

            if (overWrite == "Yes")
            {
                //files are stored in subdir named by GUIDs
                string subDir = uploadedFiles.FindContainingFolderGUIDForFile(userName, fileName).ToString();
                string description = uploadedFiles.GetFileDescription(userName, fileName);
                bool isPublic = uploadedFiles.IsSavedFilePublic(userName, fileName);

                UploadToBlob(fileName, subDir, description, isPublic, true);
            }
            else
            {
                uploadedFiles.DeleteAndRemoveFile(User.Identity.Name, fileName);
            }

            return View();
        }



        #region Private Methods
        
        private string UploadToBlob(string fileName, string subDir, string description, bool publicFile, bool overwrite = false)
        {
            BlobManager blobManager = new BlobManager();
            string path = Path.Combine(s_BasePath, subDir);

            if (publicFile)
            {
                if (!blobManager.UploadBlobToPublicContainer(User.Identity.Name, fileName, path, description, overwrite))
                { return fileName; }
            }
            else
            {
                if (!blobManager.UploadBlobToUserContainer(User.Identity.Name, fileName, path, description, overwrite))
                { return fileName; }
            }

            return string.Empty;
        }

        private string ConvertAndUploadToBlob(string fileName, string altFileName, string subDir, string description, bool publicFile)
        {
            char sep = Path.DirectorySeparatorChar;
            string path = Path.Combine(s_BasePath, subDir);
            string fileNoExt = fileName.Substring(0, fileName.LastIndexOf('.') + 1);
            FileConverter converter = new FileConverter($"UploadedFiles{sep}{subDir}", $"UploadedFiles{sep}{subDir}");

            if (converter.ConvertToFBX(fileName))
            {   //convert, upload, delete converted, delete original
                if (!string.IsNullOrEmpty(altFileName))
                {
                    //rename converted file to provided altFileName
                    System.IO.File.Move(Path.Combine(path, $"{fileNoExt}.fbx"), Path.Combine(path, $"{altFileName}.fbx"));
                    fileNoExt = altFileName;
                }

                //Call upload with converted file
                return UploadToBlob($"{fileNoExt}.fbx", subDir, description, publicFile);
            }
            else
            { 
                return "Upload Failed. Accepted file types: FBX, DAE, OBJ, STL, PLY.";
            }
        }
        
        #endregion
    }
}