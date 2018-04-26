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
    /// <summary>
    /// A class derived from the <see cref="Controller"/> class that has all
    /// of the controller actions to manage user file uploads.  
    /// </summary>
    public class UploadController : Controller
    {
        #region Members

        /// <summary> The path to the temporary file storage directory ~/UploadedFiles. </summary>
        private static string s_BasePath;

        #endregion

        /// <summary>
        /// The call to load the index page of the file upload section of the website.
        /// </summary>
        /// <returns> A view that refreshes the web page to the current url location. </returns>
        [Authorize]
        public ActionResult Index() => View();


        /// <summary>
        /// This method is functionally the same as calling the <see cref="Index"/> method.
        /// If Upload() is called as a get action - do nothing.
        /// The [HttpGet] assembly tag is used to register this method as elligible for GET
        /// requests only.
        /// The [Authorize] assembly tag is used in collaboration with ASP.NET Identity.
        /// If the action is attempted to be browsed to without the browser being correctly signed in
        /// with Identity, the request is denied.
        /// </summary>
        /// <returns> A view that refreshes the web page to the current url location. </returns>
        [HttpGet]
        [Authorize]
        public ActionResult UploadFile() => View();


        /// <summary>
        /// Allows a user to upload a file from their computer via the web browser.  The file is checked for 
        /// validity and if found valid, is saved to temporary local storage and makes a request to upload the file
        /// to Azure Blob storage. If the file is not valid or already exists in blob storage, an error message 
        /// is displayed to the user.
        /// The [HttpPost] assembly tag is used to register this method as elligible for POST
        /// requests only.
        /// The [Authorize] assembly tag is used in collaboration with ASP.NET Identity.
        /// If the action is attempted to be browsed to without the browser being correctly signed in
        /// with Identity, the request is denied.
        /// </summary>
        /// <param name="baseFile">
        ///     The file to be uploaded to Azure Blob storage.
        /// </param>
        /// <param name="matFile">
        ///     If <paramref name="baseFile"/> is a '.obj' file, this may be an accompanying '.mat' materials file.
        /// </param>
        /// <param name="publicFile">
        ///     A boolean indicator for if the user wants this file to be publicly available via the website interface.
        /// </param>
        /// <param name="altFileName">
        ///     If supplied, will be the alternate file name that the uploaded file will be saved as.
        /// </param>
        /// <param name="fileDescription">
        ///     A user-provided description of the file.
        /// </param>
        /// <returns> The current web page view with a message as to the status of the file upload. </returns>
        [HttpPost]
        [Authorize]
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
                            if (fileExt == ".fbx") //no conversion necessary - upload
                                UploadToBlob(fileName, altFileName, subDir, fileDescription, publicFile);
                            else //uploaded file is not fbx -- convert
                                ConvertAndUploadToBlob(fileName, altFileName, subDir, fileDescription, publicFile);
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


        /// <summary>
        /// This controller action is only to be called after the <see cref="UploadFile(HttpPostedFileBase, HttpPostedFileBase, bool, string, string)"/>
        /// controller action has been called and has returned an error that the file already exists in Azure Blob storage.
        /// This controller action gives the user the option to overwrite the previously existing file with the current one.
        /// If the value of <paramref name="overWrite"/> is true, this method performs the same action as
        /// <see cref="UploadFile(HttpPostedFileBase, HttpPostedFileBase, bool, string, string)"/>.
        /// The [HttpPost] assembly tag is used to register this method as elligible for POST
        /// requests only.
        /// The [Authorize] assembly tag is used in collaboration with ASP.NET Identity.
        /// If the action is attempted to be browsed to without the browser being correctly signed in
        /// with Identity, the request is denied.
        /// </summary>
        /// <param name="overWrite">
        ///     A boolean status indicating whether a file that has the same name as the new one should be overwritten in order to save the new one.
        /// </param>
        /// <param name="uploadFileName">
        ///     The name of the file saved in temporary storage from the <see cref="UploadFile(HttpPostedFileBase, HttpPostedFileBase, bool, string, string)"/> 
        ///     call prior.  This will either be the baseFile.FileName or the altFileName if one was provided.
        /// </param>
        /// <returns> The current web page view with a message as to the status of the file upload. </returns>
        [HttpPost]
        [Authorize]
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

        /// <summary>
        /// Attempt to upload the file to Azure Blob storage from temporary local storage.
        /// </summary>
        /// <param name="fileName">The name of the file stored in temporary local storage.</param>
        /// <param name="altFileName">The name to save the file as in Azure Blob storage.</param>
        /// <param name="subDir">The subdirectory that the file has been temporarily stored in.</param>
        /// <param name="description">The user-provided description of the file.</param>
        /// <param name="publicFile">Whether the file should be made publicly available or not.</param>
        /// <param name="overwrite">
        ///     Whether a file with the same name in Azure Blob storage should be overwritten or not. 
        ///     The default value for this parameter is false.
        /// </param>
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


        /// <summary>
        /// Perform a file conversion before calling the <see cref="UploadToBlob(string, string, string, string, bool, bool)"/> method.
        /// Only .fbx files are stored in Azure Blob storage long term as they are the smallest available supported file type.
        /// </summary>
        /// <param name="fileName">The name of the file stored in temporary local storage.</param>
        /// <param name="altFileName">The name to save the file as in Azure Blob storage.</param>
        /// <param name="subDir">The subdirectory that the file has been temporarily stored in.</param>
        /// <param name="description">The user-provided description of the file.</param>
        /// <param name="publicFile">Whether the file should be made publicly available or not.</param>
        private void ConvertAndUploadToBlob(string fileName, string altFileName, string subDir, string description, bool publicFile)
        {
            char sep = Path.DirectorySeparatorChar;
            //get an instance of the file converter in the directory that the file has been temporarily stored
            FileConverter converter = new FileConverter($"UploadedFiles{sep}{subDir}", $"UploadedFiles{sep}{subDir}");

            if (converter.ConvertToFBX(fileName)) //Call upload with converted file
                UploadToBlob(fileName, altFileName, subDir, description, publicFile);
            else
                ViewBag.Message = "Upload Failed. Accepted file types: DAE, FBX, OBJ, PLY, STL.";
        }

        #endregion
    }
}