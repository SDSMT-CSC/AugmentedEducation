using System;
using System.IO;
using System.Drawing;
using System.Web.Mvc;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using Microsoft.WindowsAzure.Storage.Blob;

using QRCoder;

namespace ARFE.Controllers
{
    public class PublicContentController : Controller
    {
        // GET: PublicContent
        public ActionResult Index()
        {
            var model = new FileTypeModel();
            var filestypes = GetAllFileTypes();

            ViewBag.fileObjects = DateSearch(DateTime.MinValue,DateTime.MaxValue, (int)OrderingOption.NewestFirst);
            // Create a list of SelectListItems so these can be rendered on the page
            model.FileTypes = GetSelectListItems(filestypes);

            return View("Index", model);
        }


        [Authorize]
        [HttpPost]
        public ActionResult Search(int OrderType, string SearchType, string TextCriteria, string StartDateCrit, string EndDateCrit)
        {
            var model = new FileTypeModel();
            var filestypes = GetAllFileTypes();
            model.FileTypes = GetSelectListItems(filestypes);

            DateTime min = ConvertDateString(StartDateCrit);
            DateTime max = ConvertDateString(EndDateCrit);

            if (max < min)
            {
                DateTime temp = min;
                min = max;
                max = temp;
            }

            if (SearchType == "name")
            {
                ViewBag.fileObjects = NameSearch(TextCriteria, OrderType);
            }
            else if (SearchType == "date")
            {
                ViewBag.fileObjects = DateSearch(min, max, OrderType);
            }

            return View("Index", model);
        }

        private List<Common.FileUIInfo> DateSearch(DateTime startDate, DateTime endDate, int OrderCriteria)
        {
            List<Common.FileUIInfo> publicFileObjects = GetPublicFiles();

            List<Common.FileUIInfo> searchedList = new List<Common.FileUIInfo>();

            if (OrderCriteria == (int)OrderingOption.Alphabetical)
            {
                searchedList = SearchQueries.FilterDateOrderByNameAscending(publicFileObjects, startDate, endDate);
            }
            else if (OrderCriteria == (int)OrderingOption.ReverseAlphabetical)
            {
                searchedList = SearchQueries.FilterDateOrderByNameDescending(publicFileObjects, startDate, endDate);
            }
            else if (OrderCriteria == (int)OrderingOption.NewestFirst)
            {
                searchedList = SearchQueries.FilterDateOrderByDateDescending(publicFileObjects, startDate, endDate);
            }
            else if (OrderCriteria == (int)OrderingOption.OldestFirst)
            {
                searchedList = SearchQueries.FilterDateOrderByDateAscending(publicFileObjects, startDate, endDate);
            }

            return searchedList;
        }

        private List<Common.FileUIInfo> NameSearch(string SearchCriteria, int OrderCriteria)
        {
            List<Common.FileUIInfo> publicFileObjects = GetPublicFiles();

            List<Common.FileUIInfo> searchedList = new List<Common.FileUIInfo>();

            if (OrderCriteria == (int)OrderingOption.Alphabetical)
            {
                searchedList = SearchQueries.FilterByFileNameOrderByNameAscending(publicFileObjects, SearchCriteria);
            }
            else if (OrderCriteria == (int)OrderingOption.ReverseAlphabetical)
            {
                searchedList = SearchQueries.FilterByFileNameOrderByNameDescending(publicFileObjects, SearchCriteria);
            }
            else if (OrderCriteria == (int)OrderingOption.NewestFirst)
            {
                searchedList = SearchQueries.FilterByFileNameOrderByNameDescending(publicFileObjects, SearchCriteria);
            }
            else if (OrderCriteria == (int)OrderingOption.OldestFirst)
            {
                searchedList = SearchQueries.FilterByFileNameOrderByNameAscending(publicFileObjects, SearchCriteria);
            }

            return searchedList;
        }

        private List<Common.FileUIInfo> GetPublicFiles()
        {
            int index;
            BlobManager blob = new BlobManager();
            var fileList = blob.ListPublicBlobInfoForUI();
            List<Common.FileUIInfo> fileObjects = new List<Common.FileUIInfo>();

            foreach (Common.FileUIInfo x in fileList)
            {
                if (!x.FileName.Contains(".zip"))
                {
                    //remove author name from file
                    string formattedAuthor = blob.FormatBlobContainerName(x.Author);
                    x.FileName = x.FileName.Replace($"{formattedAuthor}-", "");
                    //remove file extension from file
                    index = x.FileName.LastIndexOf(".");
                    if (index >= 0)
                        x.FileName = x.FileName.Substring(0, index);

                    index = x.Author.IndexOf('@');
                    if (index >= 0)
                        x.Author = x.Author.Substring(0, index);
                    x.UploadDate = x.UploadDate.ToLocalTime();

                    fileObjects.Add(x);
                }
            }

            return fileObjects;
        }

        [HttpPost]
        public ActionResult ContentSelect(FileTypeModel model, string downloadType)
        {
            if (model.FileType != null)
            {
                string user = User.Identity.Name; 
                int index = downloadType.LastIndexOf("--");
                string filename = downloadType.Substring(0, index) + ".fbx";
                string selectionType = downloadType.Substring(index + 2);

                BlobManager blobManager = new BlobManager();

                filename = $"{blobManager.FormatBlobContainerName(user)}-{filename}";

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

        private DateTime ConvertDateString(string date)
        {
            string[] dateTokens = date.Split('-');
            int year = Convert.ToInt32(dateTokens[0], 10);
            int month = Convert.ToInt32(dateTokens[1], 10);
            int day = Convert.ToInt32(dateTokens[2], 10);

            return new DateTime(year, month, day);
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

