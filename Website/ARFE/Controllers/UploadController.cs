using System;
using System.IO;
using System.Web;
using System.Web.Mvc;

/// <summary>
/// This namespaces is a sub-namespace of the ARFE project namespace specifically
/// for the ASP.NET Controllers.
/// </summary>
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
        public ActionResult UploadFile(HttpPostedFileBase baseFile, HttpPostedFileBase matFile, bool publicFile, string altFileName, string fileDescription)
        {
            string subDir = string.Empty;
            string uploadMessage = "File Uploaded Successfully.";
            UploadedFileCache uploadedFiles = UploadedFileCache.GetInstance();

            if (string.IsNullOrEmpty(s_BasePath))
                s_BasePath = uploadedFiles.BasePath;

            if (baseFile.ContentLength > 0)
            {
                string matFileName = string.Empty;
                string fileName = Path.GetFileName(baseFile.FileName);
                string fileExt = fileName.Substring(fileName.LastIndexOf('.'));
                string uploadFileName = (string.IsNullOrEmpty(altFileName) ? fileName : altFileName);

                if (matFile != null)
                    matFileName = Path.GetFileName(matFile.FileName);

                try
                {
                    if (uploadedFiles.SaveFile(baseFile, User.Identity.Name, fileName, altFileName, fileDescription, publicFile))
                    {
                        if (!string.IsNullOrEmpty(matFileName))
                            if (!uploadedFiles.SaveFile(matFile, User.Identity.Name, matFileName, "", fileDescription, publicFile))
                            { ViewBag.Message = $"Unable to save {matFileName} prior to upload.  {fileName} was uploaded without it."; }

                        BlobManager blobManager = new BlobManager();
                        subDir = uploadedFiles.FindContainingFolderGUIDForFile(User.Identity.Name, uploadFileName).ToString();
                        if (Guid.Parse(subDir) == Guid.Empty)
                            ViewBag.Message = $"{fileName} was not found to upload.  It may have expired from temporary storage.";
                        else
                        {
                            if (fileExt == ".fbx")
                            {   //no conversion necessary - upload
                                UploadToBlob(fileName, altFileName, subDir, fileDescription, publicFile);
                            }
                            else
                            {   //uploaded file is not fbx -- convert
                                ConvertAndUploadToBlob(fileName, altFileName, subDir, fileDescription, publicFile);
                            }
                        }
                    }
                    else { ViewBag.Message = $"Failed to save {fileName} prior to upload."; }
                }
                catch (Exception ex) { uploadMessage = ex.ToString(); }

                //give user change to overwrite
                if (string.IsNullOrEmpty(ViewBag.FileExists))
                {
                    //if file wasn't saved - does nothing, 
                    //else marks subdirectory containing file for deletion
                    uploadedFiles.MarkForDelete(User.Identity.Name, fileName);
                }
            }

            if(string.IsNullOrEmpty(ViewBag.Message))
                ViewBag.Message = uploadMessage;
            return View();
        }


        public ActionResult OverWriteUpload(string overWrite, string uploadFileName)
        {
            string userName = User.Identity.Name;
            UploadedFileCache uploadedFiles = UploadedFileCache.GetInstance();

            if (string.IsNullOrEmpty(s_BasePath))
                s_BasePath = uploadedFiles.BasePath;

            if (overWrite == "Yes")
            {
                //files are stored in subdir named by GUIDs
                string subDir = uploadedFiles.FindContainingFolderGUIDForFile(userName, uploadFileName).ToString();
                if (Guid.Parse(subDir) == Guid.Empty)
                    ViewBag.Message = $"{uploadFileName} was not found to upload.  It may have expired from temporary storage.";
                else
                {
                    string description = uploadedFiles.GetFileDescription(userName, uploadFileName);
                    bool isPublic = uploadedFiles.IsSavedFilePublic(userName, uploadFileName);

                    UploadToBlob(uploadFileName, "", subDir, description, isPublic, true);
                    ViewBag.Message = "File uploaded successfully";
                }
            }
            else
            {
                uploadedFiles.DeleteAndRemoveFile(User.Identity.Name, uploadFileName);
                ViewBag.Message = "File upload failed.";
            }

            return View("UploadFile");
        }



        #region Private Methods

        private void UploadToBlob(string fileName, string altFileName, string subDir, string description, bool publicFile, bool overwrite = false)
        {
            BlobManager blobManager = new BlobManager();
            string path = Path.Combine(s_BasePath, subDir);
            string fileNoExt = fileName.Substring(0, fileName.LastIndexOf('.'));

            try
            {
                //convert, upload, delete converted, delete original
                if (!string.IsNullOrEmpty(altFileName))
                {
                    //rename converted file to provided altFileName
                    System.IO.File.Move(Path.Combine(path, $"{fileNoExt}.fbx"), Path.Combine(path, $"{altFileName}.fbx"));
                    fileNoExt = altFileName;
                }

                if (publicFile)
                {
                    if (!blobManager.UploadBlobToPublicContainer(User.Identity.Name, $"{fileNoExt}.fbx", path, description, overwrite))
                    { ViewBag.FileExists = fileName; }
                }
                else
                {
                    if (!blobManager.UploadBlobToUserContainer(User.Identity.Name, $"{fileNoExt}.fbx", path, description, overwrite))
                    { ViewBag.FileExists = fileName; }
                }
            }
            catch { ViewBag.Message = "File upload failed."; }
        }

        private void ConvertAndUploadToBlob(string fileName, string altFileName, string subDir, string description, bool publicFile)
        {
            char sep = Path.DirectorySeparatorChar;
            FileConverter converter = new FileConverter($"UploadedFiles{sep}{subDir}", $"UploadedFiles{sep}{subDir}");

            if (converter.ConvertToFBX(fileName))
            {   //Call upload with converted file
                UploadToBlob(fileName, altFileName, subDir, description, publicFile);
            }
            else
            {
                ViewBag.Message = "Upload Failed. Accepted file types: FBX, DAE, OBJ, STL, PLY.";
            }
        }

        #endregion
    }
}