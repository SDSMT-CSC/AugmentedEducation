using System;
using System.IO;
using System.Web;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;
using AuthenticationTokenCache;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace ARFE.Controllers
{
    [Route("[controller]/[action]")]
    public class MobileAuthController : Controller
    {
        #region Members

        private TokenCache _TokenCache;
        private enum FileDescriptor
        {
            ALL = 0,
            OWNED_ALL,
            OWNED_PRIVATE,
            OWNED_PUBLIC,
            NOT_OWNED_PUBLIC,
        };

        #endregion


        #region Constructor

        public MobileAuthController()
        {
            _TokenCache = TokenCache.Init();
        }

        #endregion


        #region Properties

        public ApplicationUserManager UserManager => HttpContext.GetOwinContext().Get<ApplicationUserManager>();
        public ApplicationSignInManager SignInManager => HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
        #endregion


        #region Public Methods

        /// <summary>
        /// Read the userName and password values from the JSON and create a unique 
        /// Authorization Token string associated to the user credentials and stored
        /// in a cache.
        /// </summary>
        /// <returns>JSON response of the Authentication Token</returns>
        [HttpPost]
        public async Task<string> RequestAuthToken()
        {
            try
            {
                string token = string.Empty;
                string reason = string.Empty;
                string userName = string.Empty;
                string password = string.Empty;
                Stream bodyStream = Request.InputStream;
                JObject requestJson = JObject.Parse(new StreamReader(bodyStream).ReadToEnd());

                foreach (JToken t in requestJson.Children())
                {
                    //Get Json values out from keys
                    if (t.Path == "userName") { userName = requestJson["userName"].ToString(); }
                    else if (t.Path == "password") { password = requestJson["password"].ToString(); }
                }

                if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(password))
                {
                    userName = userName.ToLower();
                    reason = await UserSignIn(userName, password);

                    if (string.IsNullOrEmpty(reason))
                    {
                        //return token generated for user
                        token = _TokenCache.GenerateToken(userName, password);
                    }
                }
                else { reason = "'userName' and 'password' fields are required."; }

                //string -> JObject -> string provides formatted Json
                return (JObject.Parse(FormatRequestAuthTokenResult(reason, token))).ToString();
            }
            catch (Exception ex)
            {
                return ($"{{ \"Exception\": \"{ex.ToString()}\" }}");
            }
        }


        /// <summary>
        /// Return a list of files that a mobile user has access to denoted by an ownership
        /// descriptor value.  The discriptor values are 
        /// 0: ALL (All public and private files that the mobile user has access to.),
        /// 1: OWNED_ALL (All public and private files that the mobile user owns), 
        /// 2: OWNED_PRIVATE (All of the private files owned by the mobile user), 
        /// 3: OWNED_PUBLIC (All of the public files owned by the mobile user), 
        /// 4: NOT_OWNED_PUBLIC (All of the public files not owned by the mobile user).
        /// 
        /// If a <paramref name="pageNumber"/> value is supplied, the response list will be paginated
        /// with at most 10 results per page, and the correct page of values will be returned.
        /// </summary>
        /// <param name="descriptor">The file descriptor value</param>
        /// <param name="pageNumber">The page index to return.  If null, return all pages.</param>
        /// <returns>The JSON response of the requested file names to associated non-downloadable URIs.</returns>
        [HttpGet]
        public async Task<string> ListFiles(int descriptor, int? pageNumber)
        {
            try
            {
                int totalPages = 0;
                int currentPage = 0;
                string reason = string.Empty;
                string userName = string.Empty;
                Tuple<string, string> validateUserInfo;
                List<Tuple<string, Uri>> fileList = new List<Tuple<string, Uri>>();
                List<List<Tuple<string, Uri>>> pageFiles = new List<List<Tuple<string, Uri>>>();

                validateUserInfo = await ValidateUser();

                if (string.IsNullOrEmpty(validateUserInfo.Item2))
                {
                    userName = validateUserInfo.Item1;
                    BlobManager blobManager = new BlobManager();

                    switch (descriptor)
                    {
                        case ((int)FileDescriptor.ALL): //0
                            fileList = blobManager.ListBlobNamesToUrisInUserContainer(userName);
                            fileList.AddRange(blobManager.ListBlobNamesToUrisInPublicContainer());
                            break;
                        case ((int)FileDescriptor.OWNED_ALL): //1
                            fileList = blobManager.ListBlobNamesToUrisInUserContainer(userName);
                            fileList = blobManager.ListBlobNamesToUrisInPublicContainerOwnedBy(userName);
                            break;
                        case ((int)FileDescriptor.OWNED_PRIVATE): //2
                            fileList = blobManager.ListBlobNamesToUrisInUserContainer(userName);
                            break;
                        case ((int)FileDescriptor.OWNED_PUBLIC): //3
                            fileList = blobManager.ListBlobNamesToUrisInPublicContainerOwnedBy(userName);
                            break;
                        case ((int)FileDescriptor.NOT_OWNED_PUBLIC): //4
                                                                     //List of all public files
                            fileList = blobManager.ListBlobNamesToUrisInPublicContainer();
                            //remove the ones owned by user
                            foreach (Tuple<string, Uri> file in blobManager.ListBlobNamesToUrisInPublicContainerOwnedBy(userName))
                            {
                                fileList.Remove(file);
                            }
                            break;
                        default:
                            break;
                    }

                    //if requesting page and there is content
                    if (pageNumber.HasValue && fileList.Count > 0)
                    {
                        //if requesting valid page
                        if (pageNumber.Value > 0)
                        {
                            pageFiles = PaginateFiles(fileList);
                            //if requested page is in range of real pages
                            if (pageNumber.Value <= pageFiles.Count)
                            {
                                totalPages = pageFiles.Count;
                                currentPage = pageNumber.Value;
                                fileList = pageFiles[pageNumber.Value - 1];
                            }
                            else { reason = "Page number out of bounds."; }
                        }
                        else { reason = "Page number must be greater than 0."; }
                    }
                }

                //string -> JObject -> string provides formatted Json
                //Quotes needed around everything that isn't a numeric value
                return (JObject.Parse(FormatListFilesResult(fileList, reason, currentPage, totalPages))).ToString();

            }
            catch (Exception ex)
            {
                return ($"{{ \"Exception\": \"{ex.ToString()}\" }}");
            }
        }


        /// <summary>
        /// Using the Authentication Token and the FileUri, both provided in the HTTP header as 
        /// fileUri: [fileUri], token: [token]
        /// Authenticate the user from the provided token, and request to Azure Blob storage to 
        /// get a downloadable file URI for the file.
        /// </summary>
        /// <returns>The JSON response containing the downloadable file URI.</returns>
        [HttpGet]
        public async Task<string> DownloadFile()
        {
            try
            {
                string reason = string.Empty;
                string fileUri = string.Empty;
                string downloadUrl = string.Empty;
                Tuple<string, string> validateUserInfo = null;

                //Get username and validate password from auth token
                validateUserInfo = await ValidateUser();

                if (string.IsNullOrEmpty(validateUserInfo.Item2))
                {
                    //get file URI from HTTP headers
                    if (Request.Headers.AllKeys.Contains("fileUri")) { fileUri = Request.Headers["fileUri"].ToString(); }

                    //Parse Blob Container name and Blob Name
                    string[] uriParts = fileUri.Split('/');
                    BlobManager blobManager = new BlobManager();
                    string blobName = uriParts[uriParts.Count() - 1];
                    string containerName = uriParts[uriParts.Count() - 2];

                    //make sure user is only trying to get their own or public file
                    if (containerName.Equals(blobManager.FormatBlobContainerName(validateUserInfo.Item1)) || containerName.Equals("public"))
                    {
                        downloadUrl = blobManager.ConvertAndDownloadBlobFromUserContainer(containerName, blobName, ".obj", Server.MapPath("~/UploadedFiles"));
                        if (downloadUrl.Contains("Error: "))
                        {
                            //bad download attempt
                            reason = downloadUrl;
                            downloadUrl = string.Empty;
                        }
                    }
                    else { reason = "Permission denied."; }
                }
                //return JSON
                return (JObject.Parse(FormatDownloadFileResult(reason, downloadUrl))).ToString();
            }
            catch (Exception ex)
            {
                //return exception message
                return ($"{{ \"Exception\": \"{ex.ToString()}\" }}");
            }
        }

        #endregion


        #region Private Methods

        private async Task<Tuple<string, string>> ValidateUser()
        {
            string token = string.Empty;
            string reason = string.Empty;
            Tuple<string, string> userInfo = null;

            //token should be passed in HttpRequest headers as Json:
            //{ "token" : "xxxxxxxxxxxx" }
            if (Request.Headers.AllKeys.Contains("token")) { token = Request.Headers["token"].ToString(); }

            if (token != string.Empty)
            {
                userInfo = _TokenCache.ValidateToken(token);
                if (userInfo != null)
                {
                    reason = await UserSignIn(userInfo.Item1, userInfo.Item2);
                }
                else { reason = "Token not found."; }
            }
            else { reason = "User access token required."; }

            return new Tuple<string, string>(userInfo.Item1, reason);
        }

        private async Task<string> UserSignIn(string userName, string password)
        {
            string reason = string.Empty;

            //lookup user from Owin Context by UserName
            Models.ApplicationUser appUser = await HttpContext.GetOwinContext()
                                                                .Get<ApplicationUserManager>()
                                                                .FindByNameAsync(userName);
            //user found from lookup
            if (appUser != null)
            {
                //Compare password provided from Http Request to stored hash using EF
                PasswordVerificationResult verified = UserManager.PasswordHasher.VerifyHashedPassword(appUser.PasswordHash, password);
                if (verified == PasswordVerificationResult.Failed)
                {
                    reason = "Password authentication failed.";
                    //else : correct UserName, Password ==> good to go
                }
            }
            else { reason = "User information not found."; }

            return reason;
        }

        private string FormatRequestAuthTokenResult(string reason, string token)
        {
            string quote = "\"";
            //bool.TrueString or bool.FalseString
            string success = (string.IsNullOrEmpty(reason)).ToString();
            if (success == bool.TrueString) { reason = "SUCCESS"; }

            StringBuilder jsonString = new StringBuilder("{ ");
            jsonString.Append($"{quote}success{quote} : {quote}{success}{quote}, ");
            jsonString.Append($"{quote}reason{quote} : {quote}{reason}{quote}, ");
            jsonString.Append($"{quote}token{quote} : {quote}{token}{quote}");
            jsonString.Append(" }");

            //Quotes needed around everything that isn't a numeric value
            return jsonString.ToString();
        }

        private string FormatListFilesResult(List<Tuple<string, Uri>> fileList, string reason, int currentPage, int totalPages)
        {
            string quote = "\"";
            //bool.TrueString or bool.FalseString
            string success = (string.IsNullOrEmpty(reason)).ToString();
            if (success == bool.TrueString) { reason = "SUCCESS"; }
            StringBuilder jsonString = new StringBuilder("{ ");

            jsonString.Append($"{quote}success{quote} : {quote}{success}{quote}, ");
            jsonString.Append($"{quote}reason{quote} : {quote}{reason}{quote}, ");
            //Open "result" : { ... }
            // '{{' required when using $"" interpolation
            jsonString.Append($"{quote}result{quote} : {{ ");

            //don't list the files if error response
            if (success == bool.TrueString)
            {
                //Open "files" : [ ... ]
                jsonString.Append($"{quote}files{quote} : [");

                foreach (Tuple<string, Uri> fileItem in fileList)
                {
                    //Open each file result Json Object
                    jsonString.Append("{ ");
                    jsonString.Append($"{quote}name{quote} : {quote}{fileItem.Item1}{quote},");
                    jsonString.Append($"{quote}uri{quote} : {quote}{fileItem.Item2}{quote}");
                    //Close each file result Json Object
                    jsonString.Append(" }");
                    if (fileItem != fileList.Last())
                    {   //Append comma if more to come
                        jsonString.Append(",");
                    }
                }

                //Close "files" : [ ... ]
                jsonString.Append(" ], ");
                //"result" : { } info that isn't file objects
                jsonString.Append($"{quote}totalCount{quote} : {fileList.Count}, ");
                jsonString.Append($"{quote}totalPages{quote} : {totalPages}, ");
                jsonString.Append($"{quote}currentPage{quote} : {currentPage} ");
            }

            jsonString.Append("} ");
            //Close whole json result
            jsonString.Append("}");

            return jsonString.ToString();
        }

        private string FormatDownloadFileResult(string reason, string downloadUrl)
        {
            string quote = "\"";
            string success = (string.IsNullOrEmpty(reason)).ToString();
            if (success == bool.TrueString) { reason = "SUCCESS"; }

            StringBuilder jsonString = new StringBuilder("{ ");
            jsonString.Append($"{quote}success{quote} : {quote}{success}{quote}, ");
            jsonString.Append($"{quote}reason{quote} : {quote}{reason}{quote}, ");
            jsonString.Append($"{quote}result{quote} : {quote}{downloadUrl}{quote}");
            jsonString.Append(" }");

            return jsonString.ToString();
        }

        private List<List<Tuple<string, Uri>>> PaginateFiles(List<Tuple<string, Uri>> fileList)
        {
            int idx = 0;
            List<Tuple<string, Uri>> tempList = null;
            List<List<Tuple<string, Uri>>> pageFiles = new List<List<Tuple<string, Uri>>>();

            //loop over every file read
            while (idx < fileList.Count)
            {
                //if at an even 10 index
                if (idx % 10 == 0)
                {
                    //if not just entering loop - add tempList as page
                    if (tempList != null) { pageFiles.Add(tempList); }
                    //new tempList for next range of values
                    tempList = new List<Tuple<string, Uri>>();
                }
                //add value and incriment position
                tempList.Add(fileList[idx++]);
            }

            //broke out of while mid-page
            if (tempList.Count > 0) { pageFiles.Add(tempList); }

            return pageFiles;
        }

        #endregion
    }
}