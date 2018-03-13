﻿using QRCoder;
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
        public ActionResult Index()
        {
            int index;
            BlobManager blob = new BlobManager();

            var privatefileList = blob.ListBlobNamesInUserContainer(User.Identity.Name);
            List<string> privatenames = new List<string>();
            foreach (string x in privatefileList)
            {
                index = x.LastIndexOf(".");
                privatenames.Add(x.Substring(0, index));
            }

            var publicfileList = blob.ListBlobNamesInPublicContainer();
            List<string> publicnames = new List<string>();
            foreach (string x in publicfileList)
            {
                index = x.LastIndexOf(".");
                publicnames.Add(x.Substring(0, index));
            }

            ViewBag.privatefilenames = privatenames;
            ViewBag.publicfilenames = publicnames;
            ViewBag.iterator = 0;

            // Let's get all states that we need for a DropDownList
            var filestypes = GetAllFileTypes();

            var model = new FileTypeModel();

            // Create a list of SelectListItems so these can be rendered on the page
            model.FileTypes = GetSelectListItems(filestypes);

            return View(model);
        }

        //
        // 2. Action method for handling user-entered data when 'Sign Up' button is pressed.
        //
        [HttpPost]
        public ActionResult PrivateContentSelect(FileTypeModel model, string downloadType)
        {
            int index = downloadType.LastIndexOf("--");

            string filename = downloadType.Substring(0, index) + ".fbx";

            string selectionType = downloadType.Substring(index + 2);

            BlobManager blobManager = new BlobManager();

            string downloadLink = blobManager.ConvertAndDownloadBlobFromUserContainer(User.Identity.Name, filename, model.FileType, Server.MapPath("~/UploadedFiles"));

            ViewBag.DownloadLink = downloadLink;
           
            if(selectionType == "Download")
            {
                Response.Redirect(downloadLink);
            }
            else if(selectionType == "QRCode")
            {
                return DisplayQRCode(downloadLink);
            }
            
            return View();
        }


        public ActionResult PublicContentSelect(FileTypeModel model, string downloadType)
        {
            int index = downloadType.LastIndexOf("--");

            string filename = downloadType.Substring(0, index) + ".fbx";

            string selectionType = downloadType.Substring(index + 2);

            BlobManager blobManager = new BlobManager();

            string downloadLink = blobManager.ConvertAndDownloadBlobFromUserContainer(User.Identity.Name, filename, model.FileType, Server.MapPath("~/UploadedFiles"));

            ViewBag.DownloadLink = downloadLink;

            if (selectionType == "Download")
            {
                Response.Redirect(downloadLink);
            }
            else if (selectionType == "QRCode")
            {
                return DisplayQRCode(downloadLink);
            }

            return View();
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

        // This is one of the most important parts in the whole example.
        // This function takes a list of strings and returns a list of SelectListItem objects.
        // These objects are going to be used later in the SignUp.html template to render the
        // DropDownList.
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