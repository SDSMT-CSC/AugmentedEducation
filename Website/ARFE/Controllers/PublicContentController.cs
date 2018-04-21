//system .dll's
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
    /// A class derived from the <see cref="Controller"/> class that has all
    /// of the controller actions to dispaly and interact with the public file content 
    /// on the website.
    /// </summary>
    public class PublicContentController : Controller
    {
        /// <summary>
        /// The Index controller action is the default action called when the UserContent page is
        /// browsed to.
        /// </summary>
        /// <returns>
        ///     A view to the "Index.cshtml" page in the Views/PublicContent/ folder.
        /// </returns>
        public ActionResult Index()
        {
            int index;
            var model = new FileTypeModel();
            BlobManager blob = new BlobManager();
            var fileList = blob.ListPublicBlobInfoForUI();
            List<FileUIInfo> fileObjects = new List<FileUIInfo>();

            foreach (FileUIInfo info in fileList)
            {
                if(!info.FileName.Contains(".zip"))
                {
                    //remove file extension from file
                    index = info.FileName.LastIndexOf(".");
                    if (index >= 0)
                        info.FileName = info.FileName.Substring(0, index);

                    index = info.Author.IndexOf('@');
                    if (index >= 0)
                        info.Author = info.Author.Substring(0, index);
                    info.UploadDate = info.UploadDate.ToLocalTime();

                    fileObjects.Add(info);
                }
            }

            ViewBag.fileObjects = fileObjects;
            // Create a list of SelectListItems so these can be rendered on the page
            model.FileTypes = GetSelectListItems();

            return View("Index", model);
        }


        /// <summary>
        /// The controller action responsible for the sub-menu button clicks on the menu
        /// that corresponds to the content that the user owns but is publicly available.
        /// The [HttpPost] assembly tag is used to register this method as elligible for POST
        /// requests only.
        /// </summary>
        /// <param name="model"> 
        /// A <see cref="FileTypeModel"/> object representing the file selected.
        /// </param>
        /// <param name="downloadType">
        /// The selected file type for potential conversion
        /// </param>
        /// <returns>
        /// A view redirection to refresh the content of the Public Content page.
        /// </returns>
        [HttpPost]
        public ActionResult ContentSelect(FileTypeModel model, string downloadType)
        {
            if (model.FileType != null)
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

}



public class FileTypeModel
{
    // This property will hold all available states for selection

    [Required]
    [Display(Name = "File Type")]
    public string FileType { get; set; }
    public IEnumerable<SelectListItem> FileTypes { get; set; }
}

