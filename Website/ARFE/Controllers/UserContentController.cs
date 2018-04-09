using Microsoft.WindowsAzure.Storage.Blob;
using QRCoder;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ARFE.Controllers
{
    public class UserContentController : Controller
    {
        [Authorize]
        public ActionResult Index()
        {
            int index;
            BlobManager blob = new BlobManager();

            var model = new FileTypeModel();
            List<Common.FileUIInfo> publicFileList = blob.ListPublicBlobInfoForUI();
            List<Common.FileUIInfo> publicFileObjects = new List<Common.FileUIInfo>();
            List<Common.FileUIInfo> privateFileObjects = new List<Common.FileUIInfo>();
            List<Common.FileUIInfo> privateFileList = blob.ListPrivateBlobInfoForUI(User.Identity.Name);

            foreach (Common.FileUIInfo x in privateFileList)
            {
                if (!x.FileName.Contains(".zip"))
                {
                    index = x.FileName.LastIndexOf(".");
                    x.FileName = x.FileName.Substring(0, index);

                    index = x.Author.IndexOf('@');
                    x.Author = x.Author.Substring(0, index);
                    x.UploadDate = x.UploadDate.ToLocalTime();

                    privateFileObjects.Add(x);
                }

            }

            foreach (Common.FileUIInfo x in publicFileList)
            {
                if (!x.FileName.Contains(".zip"))
                {
                    if (x.Author == User.Identity.Name)
                    {
                        index = x.FileName.LastIndexOf(".");
                        x.FileName = x.FileName.Substring(0, index);

                        index = x.Author.IndexOf('@');
                        x.Author = x.Author.Substring(0, index);
                        x.UploadDate = x.UploadDate.ToLocalTime();

                        publicFileObjects.Add(x);
                    }
                }
            }

            var filestypes = GetAllFileTypes();
            ViewBag.publicFiles = publicFileObjects;
            ViewBag.privateFiles = privateFileObjects;

            // Create a list of SelectListItems so these can be rendered on the page
            model.FileTypes = GetSelectListItems(filestypes);

            return View("Index", model);
        }

        [Authorize]
        [HttpPost]
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


        [Authorize]
        [HttpPost]
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

        private IEnumerable<string> GetAllFileTypes()
        {
            return new List<string>
            {
                ".fbx",
                ".dae",
                ".obj",
                ".ply",
                ".stl",
            };
        }

        private IEnumerable<SelectListItem> GetSelectListItems(IEnumerable<string> elements)
        {
            // Create an empty list to hold result of the operation
            var selectList = new List<SelectListItem>();

            // For each string in the 'elements' variable, create a new SelectListItem object
            // that has both its Value and Text properties set to a particular value.
            // This will result in MVC rendering each item as:
            //     <option value="State Name">State Name</option>
            foreach (var element in elements)
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

    public class FileTypeModel
    {
        // This property will hold all available states for selection

        [Required]
        [Display(Name = "File Type")]
        public string FileType { get; set; }
        public IEnumerable<SelectListItem> FileTypes { get; set; }
    }
}