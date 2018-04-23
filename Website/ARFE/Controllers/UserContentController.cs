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
    public class UserContentController : Controller
    {
        [Authorize]
        public ActionResult Index()
        {
            // Create a list of SelectListItems so these can be rendered on the page
            var model = new FileTypeModel();
            var filestypes = GetAllFileTypes();
            model.FileTypes = GetSelectListItems(filestypes);

            ViewBag.publicFiles = PublicDateSearch(DateTime.MinValue, DateTime.MaxValue, (int)OrderingOption.NewestFirst);
            ViewBag.privateFiles = PrivateDateSearch(DateTime.MinValue, DateTime.MaxValue, (int)OrderingOption.NewestFirst);

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


        [Authorize]
        [HttpPost]
        public ActionResult PrivateSearch(int prOrderType, string prSearchType, string prTextCriteria, string prStartDateCrit, string prEndDateCrit)
        {
            var model = new FileTypeModel();
            var filestypes = GetAllFileTypes();
            model.FileTypes = GetSelectListItems(filestypes);

            DateTime min = ConvertDateString(prStartDateCrit);
            DateTime max = ConvertDateString(prEndDateCrit);

            if(max < min)
            {
                DateTime temp = min;
                min = max;
                max = temp;
            }

            if (prSearchType == "name")
            {
                ViewBag.privateFiles = PrivateNameSearch(prTextCriteria, prOrderType);
            }
            else if(prSearchType == "date")
            {
                ViewBag.privateFiles = PrivateDateSearch(min, max, prOrderType);
            }
            
            ViewBag.publicFiles = PublicDateSearch(DateTime.MinValue, DateTime.MaxValue, (int)OrderingOption.NewestFirst);

            return View("Index", model);
        }

        [Authorize]
        [HttpPost]
        public ActionResult PublicSearch(int puOrderType, string puSearchType, string puTextCriteria, string puStartDateCrit, string puEndDateCrit)
        {
            var model = new FileTypeModel();
            var filestypes = GetAllFileTypes();
            model.FileTypes = GetSelectListItems(filestypes);

            DateTime min = ConvertDateString(puStartDateCrit);
            DateTime max = ConvertDateString(puEndDateCrit);

            if (max < min)
            {
                DateTime temp = min;
                min = max;
                max = temp;
            }

            if (puSearchType == "name")
            {
                ViewBag.publicFiles = PublicNameSearch(puTextCriteria, puOrderType);
            }
            else if (puSearchType == "date")
            {
                ViewBag.publicFiles = PublicDateSearch(min, max, puOrderType);
            }

            ViewBag.privateFiles = PrivateDateSearch(DateTime.MinValue, DateTime.MaxValue, (int)OrderingOption.NewestFirst);

            return View("Index", model);
        }

        private List<Common.FileUIInfo> PrivateNameSearch(string SearchCriteria, int OrderCriteria)
        {

            List<Common.FileUIInfo> privateFileObjects = GetPrivateFiles();

            List<Common.FileUIInfo> searchedList = new List<Common.FileUIInfo>();

            if (OrderCriteria == (int)OrderingOption.Alphabetical)
            {
                searchedList = SearchQueries.FilterByFileNameOrderByNameAscending(privateFileObjects, SearchCriteria);
            }
            else if (OrderCriteria == (int)OrderingOption.ReverseAlphabetical)
            {
                searchedList = SearchQueries.FilterByFileNameOrderByNameDescending(privateFileObjects, SearchCriteria);
            }
            else if (OrderCriteria == (int)OrderingOption.NewestFirst)
            {   
                searchedList = SearchQueries.FilterByFileNameOrderByDateDescending(privateFileObjects, SearchCriteria);
            }
            else if (OrderCriteria == (int)OrderingOption.OldestFirst)
            {
                searchedList = SearchQueries.FilterByFileNameOrderByDateAscending(privateFileObjects, SearchCriteria);
            }

            return searchedList;
        }

        private List<Common.FileUIInfo> PublicNameSearch(string SearchCriteria, int OrderCriteria)
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

        private List<Common.FileUIInfo> PrivateDateSearch(DateTime startDate, DateTime endDate, int OrderCriteria)
        {
            List<Common.FileUIInfo> privateFileObjects = GetPrivateFiles();

            List<Common.FileUIInfo> searchedList = new List<Common.FileUIInfo>();

            if (OrderCriteria == (int)OrderingOption.Alphabetical)
            {
                searchedList = SearchQueries.FilterDateOrderByNameAscending(privateFileObjects, startDate, endDate);
            }
            else if (OrderCriteria == (int)OrderingOption.ReverseAlphabetical)
            {
                searchedList = SearchQueries.FilterDateOrderByNameDescending(privateFileObjects, startDate, endDate);
            }
            else if (OrderCriteria == (int)OrderingOption.NewestFirst)
            {
                searchedList = SearchQueries.FilterDateOrderByDateDescending(privateFileObjects, startDate, endDate);
            }
            else if (OrderCriteria == (int)OrderingOption.OldestFirst)
            {
                searchedList = SearchQueries.FilterDateOrderByDateAscending(privateFileObjects, startDate, endDate);
            }

            return searchedList;
        }

        private List<Common.FileUIInfo> PublicDateSearch(DateTime startDate, DateTime endDate, int OrderCriteria)
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

        private List<Common.FileUIInfo> GetPrivateFiles()
        {
            int index;
            BlobManager blob = new BlobManager();
            List<Common.FileUIInfo> privateFileList = blob.ListPrivateBlobInfoForUI(User.Identity.Name);
            List<Common.FileUIInfo> privateFileObjects = new List<Common.FileUIInfo>();

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
            return privateFileObjects;
        }

        private List<Common.FileUIInfo> GetPublicFiles()
        {

            int index;
            BlobManager blob = new BlobManager();
            List<Common.FileUIInfo> publicFileList = blob.ListPublicBlobInfoForUI();
            List<Common.FileUIInfo> publicFileObjects = new List<Common.FileUIInfo>();

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
            return publicFileObjects;

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

    public enum OrderingOption
    {
        NewestFirst = 1,
        OldestFirst = 2,
        Alphabetical = 3,
        ReverseAlphabetical = 4

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