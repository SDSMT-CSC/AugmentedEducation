//System .dll's
using System;
using System.IO;
using System.Drawing;
using System.Web.Mvc;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

//NuGet
using QRCoder;
using Microsoft.WindowsAzure.Storage.Blob;

//other classes
using Common;

/// <summary>
/// This namespaces is a sub-namespace of the ARFE project namespace specifically
/// for the ASP.NET Controllers.
/// </summary>
namespace ARFE.Controllers
{
    /// <summary>
    /// A class derived from the <see cref="System.Web.Mvc.Controller"/> class that has all
    /// of the controller actions to dispaly and interact with the user's file content 
    /// on the website.
    /// </summary>
    public class UserContentController : Controller
    {
        /// <summary>
        /// The Index controller action is the default action called when the UserContent page is
        /// browsed to.  The [Authorize] assembly tag is used in collaboration with ASP.NET Identity.
        /// If the action is attempted to be browsed to without the browser being correctly signed in
        /// with Identity, the request is denied.
        /// </summary>
        /// <returns>
        ///     A view to the "Index.cshtml" page in the Views/UserContent/ folder.
        /// </returns>
        [Authorize]
        public ActionResult Index()
        {
            int index;
            BlobManager blob = new BlobManager();
            FileTypeModel model = new FileTypeModel();
            List<FileUIInfo> publicFileList = blob.ListPublicBlobInfoForUI();
            List<FileUIInfo> publicFileObjects = new List<FileUIInfo>();
            List<FileUIInfo> privateFileObjects = new List<FileUIInfo>();
            List<FileUIInfo> privateFileList = blob.ListPrivateBlobInfoForUI(User.Identity.Name);

            //display user's private files
            foreach (FileUIInfo x in privateFileList)
            {
                if (!x.FileName.Contains(".zip"))
                {
                    index = x.FileName.LastIndexOf(".");
                    if (index >= 0)
                        x.FileName = x.FileName.Substring(0, index);

                    index = x.Author.IndexOf('@');
                    if (index >= 0)
                        x.Author = x.Author.Substring(0, index);
                    x.UploadDate = x.UploadDate.ToLocalTime();

                    privateFileObjects.Add(x);
                }

            }

            //display user's public files
            foreach (FileUIInfo x in publicFileList)
            {
                if (!x.FileName.Contains(".zip"))
                {
                    if (x.Author == User.Identity.Name)
                    {
                        index = x.FileName.LastIndexOf(".");
                        if(index >= 0)
                            x.FileName = x.FileName.Substring(0, index);

                        index = x.Author.IndexOf('@');
                        if(index >= 0)
                            x.Author = x.Author.Substring(0, index);
                        x.UploadDate = x.UploadDate.ToLocalTime();

                        publicFileObjects.Add(x);
                    }
                }
            }

            // Create a list of SelectListItems so these can be rendered on the page
            model.FileTypes = GetSelectListItems();
            ViewBag.publicFiles = publicFileObjects;
            ViewBag.privateFiles = privateFileObjects;

            return View("Index", model);
        }


        /// <summary>
        /// The controller action responsible for the sub-menu button clicks on the menu
        /// that corresponds to the user's privately owned content.
        /// </summary>
        /// <param name="model"> 
        /// A <see cref="FileTypeModel"/> object representing the file selected.
        /// </param>
        /// <param name="downloadType">
        /// The selected file type for potential conversion
        /// </param>
        /// <returns>
        /// A view redirection to refresh the content of the User Content page.
        /// </returns>
        [HttpPost]
        [Authorize]
        public ActionResult PrivateContentSelect(FileTypeModel model, string downloadType)
        {
            if (model.FileType != null || downloadType.LastIndexOf("--Delete") >= 0)
            {
                int index = downloadType.LastIndexOf("--");
                string filename = downloadType.Substring(0, index) + ".fbx";
                string selectionType = downloadType.Substring(index + 2);

                BlobManager blobManager = new BlobManager();

                if (selectionType == "Download")
                {
                    string downloadLink = blobManager.ConvertAndDownloadBlobFromUserContainer(User.Identity.Name, filename, model.FileType, Server.MapPath("~/UploadedFiles"));
                    Response.Redirect(downloadLink);
                }
                else if (selectionType == "GeneralQR")
                {
                    string downloadLink = blobManager.ConvertAndDownloadBlobFromUserContainer(User.Identity.Name, filename, model.FileType, Server.MapPath("~/UploadedFiles"));
                    ViewBag.FileName = filename.Substring(0, filename.Length - 4) + model.FileType;
                    return DisplayQRCode(downloadLink);
                }
                else if (selectionType == "MobileQR")
                {
                    CloudBlobContainer container = blobManager.GetOrCreateBlobContainer(User.Identity.Name);
                    CloudBlockBlob blob = container.GetBlockBlobReference(filename);

                    string mobileLink = string.Empty;
                    if (blob.Exists())
                        mobileLink = blob.Uri.ToString();
                    ViewBag.FileName = filename.Substring(0, filename.Length - 4) + model.FileType;
                    return DisplayQRCode(mobileLink);
                }
                else if (selectionType == "Delete")
                {
                    bool deleted = blobManager.DeleteBlobByNameInUserContainer(User.Identity.Name, filename);
                    return Index();
                }

                return View();
            }
            else
            {
                ViewBag.Invalid = true;
                return Index();
            }
        }
        

        /// <summary>
        /// The controller action responsible for the sub-menu button clicks on the menu
        /// that corresponds to the content that the user owns but is publicly available.
        /// </summary>
        /// <param name="model"> 
        /// A <see cref="FileTypeModel"/> object representing the file selected.
        /// </param>
        /// <param name="downloadType">
        /// The selected file type for potential conversion
        /// </param>
        /// <returns>
        /// A view redirection to refresh the content of the User Content page.
        /// </returns>
        [HttpPost]
        [Authorize]
        public ActionResult PublicContentSelect(FileTypeModel model, string downloadType)
        {
            if (model.FileType != null || downloadType.LastIndexOf("--Delete") >= 0)
            {
                int index = downloadType.LastIndexOf("--");
                string filename = downloadType.Substring(0, index) + ".fbx";
                string selectionType = downloadType.Substring(index + 2);

                BlobManager blobManager = new BlobManager();

                if (selectionType == "Download")
                {
                    string downloadLink = blobManager.ConvertAndDownloadBlobFromUserContainer("public", filename, model.FileType, Server.MapPath("~/UploadedFiles"));
                    Response.Redirect(downloadLink);
                }
                else if (selectionType == "GeneralQR")
                {
                    string downloadLink = blobManager.ConvertAndDownloadBlobFromUserContainer("public", filename, model.FileType, Server.MapPath("~/UploadedFiles"));
                    return DisplayQRCode(downloadLink);
                }
                else if (selectionType == "MobileQR")
                {
                    CloudBlobContainer container = blobManager.GetOrCreateBlobContainer("public");
                    CloudBlockBlob blob = container.GetBlockBlobReference(filename);

                    string mobileLink = string.Empty;
                    if (blob.Exists())
                        mobileLink = blob.Uri.ToString();

                    return DisplayQRCode(mobileLink);
                }
                else if (selectionType == "Delete")
                {
                    bool deleted = blobManager.DeleteBlobByNameInUserContainer("public", filename);
                    return Index();
                }

                return View();
            }
            else
            {
                ViewBag.Invalid = true;
                return Index();
            }
        }


        /// <summary>
        /// Create a QR code from the file download link returned from blob storage.
        /// </summary>
        /// <param name="downloadLink">the file download link returned from blob storage</param>
        /// <returns>
        /// A page redirection that will display the QR code .PNG file.
        /// </returns>
        private ActionResult DisplayQRCode(string downloadLink)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(downloadLink, QRCodeGenerator.ECCLevel.Q);
                QRCode qrCode = new QRCode(qrCodeData);
                using (Bitmap bitmap = qrCode.GetGraphic(20))
                {
                    bitmap.Save(ms, ImageFormat.Png);
                    ViewBag.QRCodeImage = "data:image/png;base64," + Convert.ToBase64String(ms.ToArray());
                }
            }
            return View("DisplayQRCode");
        }


        /// <summary>
        /// For each string in the <see cref="SupportedFileTypes.FileList"/> list, create a new 
        /// <see cref="SelectListItem"/> object that has both its Value and Text properties 
        /// set to a particular value. This will result in MVC rendering each item as: 
        ///     <!--<option value='State Name'>State Name</option>-->
        /// </summary>
        /// <returns>
        /// A list of <see cref="SelectListItem"/> items full of the <see cref="SupportedFileTypes.FileList"/> items.
        /// </returns>
        private List<SelectListItem> GetSelectListItems()
        {
            // Create an empty list to hold result of the operation
            List<SelectListItem> selectList = new List<SelectListItem>();

            foreach (string element in SupportedFileTypes.FileList)
            {
                selectList.Add(new SelectListItem
                {
                    Value = element,
                    Text = element
                });
            }

            return selectList;
        }
    }

    /// <summary>
    ///  A POCO class for the file type combo box
    /// </summary>
    public class FileTypeModel
    {
        /// <summary> The selected file type </summary>
        [Required]
        [Display(Name = "File Type")]
        public string FileType { get; set; }

        /// <summary> The list of file type options. </summary>
        public List<SelectListItem> FileTypes { get; set; }
    }
}