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
    /// A class derived from the <see cref="Controller"/> class that has all
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
            // Create a list of SelectListItems so these can be rendered on the page
            var model = new FileTypeModel();
            model.FileTypes = GetSelectListItems();

            ViewBag.publicFiles = PublicDateSearch(DateTime.MinValue, DateTime.MaxValue, (int)OrderingOption.NewestFirst);
            ViewBag.privateFiles = PrivateDateSearch(DateTime.MinValue, DateTime.MaxValue, (int)OrderingOption.NewestFirst);

            return View("Index", model);
        }


        /// <summary>
        /// The controller action responsible for the sub-menu button clicks on the menu
        /// that corresponds to the user's privately owned content. 
        /// The [HttpPost] assembly tag is used to register this method as elligible for POST
        /// requests only.
        /// The [Authorize] assembly tag is used in collaboration with ASP.NET Identity.
        /// If the action is attempted to be browsed to without the browser being correctly signed in
        /// with Identity, the request is denied.
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
        /// The [HttpPost] assembly tag is used to register this method as elligible for POST
        /// requests only.
        /// The [Authorize] assembly tag is used in collaboration with ASP.NET Identity.
        /// If the action is attempted to be browsed to without the browser being correctly signed in
        /// with Identity, the request is denied.
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
        /// The Index controller action is called when the user in doing a unique search of the private files
        /// The [Authorize] assembly tag is used in collaboration with ASP.NET Identity.
        /// If the action is attempted to be browsed to without the browser being correctly signed in
        /// with Identity, the request is denied.
        /// </summary>
        /// <param name="prOrderType">
        /// An integer that designates the ordering style. Value corresponds to Ordering Option Enum
        /// </param>
        /// <param name="prSearchType">
        /// A string containing the type of search the user is trying to do. Value is either "name" or "date"
        /// </param>
        /// <param name="prTextCriteria">
        /// The user inputed string to filter files that only hold that name
        /// </param>
        /// <param name="prStartDateCrit">
        /// String containing the lower bound date for the date search. Format: "YYYY/MM/DD"
        /// </param>
        /// <param name="prEndDateCrit">
        /// String containing the upper bound date for the date search. Format: "YYYY/MM/DD"
        /// </param>
        /// <returns>
        ///     A view to the "Index.cshtml" page in the Views/UserContent/ folder.
        /// </returns>
        [Authorize]
        [HttpPost]
        public ActionResult PrivateSearch(int prOrderType, string prSearchType, string prTextCriteria, string prStartDateCrit, string prEndDateCrit)
        {
            var model = new FileTypeModel();
            model.FileTypes = GetSelectListItems();

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


        /// <summary>
        /// The Index controller action is called when the user in doing a unique search of the public files
        /// The [Authorize] assembly tag is used in collaboration with ASP.NET Identity.
        /// If the action is attempted to be browsed to without the browser being correctly signed in
        /// with Identity, the request is denied.
        /// </summary>
        /// <param name="puOrderType">
        /// An integer that designates the ordering style. Value corresponds to Ordering Option Enum
        /// </param>
        /// <param name="puSearchType">
        /// A string containing the type of search the user is trying to do. Value is either "name" or "date"
        /// </param>
        /// <param name="puTextCriteria">
        /// The user inputed string to filter files that only hold that name
        /// </param>
        /// <param name="puStartDateCrit">
        /// String containing the lower bound date for the date search. Format: "YYYY/MM/DD"
        /// </param>
        /// <param name="puEndDateCrit">
        /// String containing the upper bound date for the date search. Format: "YYYY/MM/DD"
        /// </param>
        /// <returns>
        ///     A view to the "Index.cshtml" page in the Views/UserContent/ folder.
        /// </returns>
        [Authorize]
        [HttpPost]
        public ActionResult PublicSearch(int puOrderType, string puSearchType, string puTextCriteria, string puStartDateCrit, string puEndDateCrit)
        {
            var model = new FileTypeModel();
            model.FileTypes = GetSelectListItems();

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

        /// <summary>
        /// This function is responsible for filtering the private file list based off a string. It also
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

        /// <summary>
        /// This function is responsible for filtering the public file list based off a string. It also
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

        /// <summary>
        /// This function responsible foe filtering the private file list date and ordering
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

        /// <summary>
        /// This function responsible foe filtering the public file list date and ordering
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

        /// <summary>
        /// This function gets all of the private files and uses it to populate
        /// a list of FileUIInfo objects. It then returns that list to the caller.
        /// requests only.
        /// <returns>
        /// A List of FileUIInfo objects corresponding to the users private
        /// </returns>
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

        /// <summary>
        /// This function gets all of the users public files and uses it to populate
        /// a list of FileUIInfo objects. It then returns that list to the caller.
        /// requests only.
        /// <returns>
        /// A List of FileUIInfo objects corresponding to the users public files
        /// </returns>
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

    public enum OrderingOption
    {
        NewestFirst = 1,
        OldestFirst = 2,
        Alphabetical = 3,
        ReverseAlphabetical = 4

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