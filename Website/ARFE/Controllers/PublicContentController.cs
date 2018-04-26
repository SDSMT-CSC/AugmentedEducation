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
            var model = new FileTypeModel();

            ViewBag.fileObjects = DateSearch(DateTime.MinValue,DateTime.MaxValue, (int)OrderingOption.NewestFirst);
            // Create a list of SelectListItems so these can be rendered on the page
            model.FileTypes = GetSelectListItems();

            return View("Index", model);
        }

        /// <summary>
        /// The controller action responsible for the filtering and ordering the 
        /// files, based on the user input.
        /// </summary>
        /// <param name="OrderType"> 
        /// An integer that references the Ordering Option enum
        /// </param>
        /// <param name="SearchType">
        /// A string containing "name" or "date". The denotes which type of search
        /// is begin submitted.
        /// </param>
        /// </param>
        /// <param name="TextCriteria">
        /// User inputted search string for the linq query
        /// </param>
        /// </param>
        /// <param name="StartDateCrit">
        /// A string containing the lower bound date for the date search
        /// </param>
        /// </param>
        /// <param name="EndDateCrit">
        /// A string containing the upper bound date for the date search
        /// </param>
        /// <returns>
        ///     A view to the "Index.cshtml" page in the Views/PublicContent/ folder.
        /// </returns>
        [Authorize]
        [HttpPost]
        public ActionResult Search(int OrderType, string SearchType, string TextCriteria, string StartDateCrit, string EndDateCrit)
        {
            var model = new FileTypeModel();
            model.FileTypes = GetSelectListItems();

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


        /// <summary>
        /// This function responsible foe filtering the file list date and ordering
        /// it based on the user input.
        /// </summary>
        /// <param name="startDate"> 
        /// A datetime contaning the lowerbound for the date search
        /// </param>
        /// <param name="endDate">
        /// A datetime containing the upper bound for the date search
        /// </param>
        /// <param name="OrderCriteria">
        /// Integer corresponding to the OrderingOption enum
        /// </param>
        /// <returns>
        /// A list for FileUIInfo objects
        /// </returns>
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


        /// <summary>
        /// This function is responsible for filtering the file list based off a string. It also
        /// orders the list based on the option provided by the user
        /// </summary>
        /// <param name="SearchCriteria"> 
        /// A <see cref="FileTypeModel"/> object representing the file selected.
        /// </param>
        /// <param name="OrderCriteria">
        /// Integer that corresponds to the OrderingOptions Enum
        /// </param>
        /// <returns>
        /// A list of FileUIInfo objects
        /// </returns>
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

        /// <summary>
        /// This function gets all of the files labeled public and uses it to populate
        /// a list of FileUIInfo objects. It then returns that list to the caller.
        /// requests only.
        /// <returns>
        /// A List of FileUIInfo objects corresponding to all public files
        /// </returns>
        private List<Common.FileUIInfo> GetPublicFiles()
        {
            int index;
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
            return fileObjects;
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


        /// <summary>
        /// Takes a string in the form of YYYY/MM/DD and converts into a datetime object and returns it.
        /// </summary>
        /// <param name="date">
        /// A string in the form of YYYY/MM/DD 
        /// </param>
        /// <returns>
        /// A datetime object created using the input string.
        /// </returns>
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

