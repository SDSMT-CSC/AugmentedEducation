using System;
using System.IO;
using System.Web;
using System.Linq;
using System.Web.Mvc;
using System.Net.Http;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;

using AuthenticationTokenCache;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

using Newtonsoft.Json.Linq;

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

        /* Waiting until SSL can be ensured
        public string CreateAccount(string userName, string password, string verifyPassword)
        {
            Models.LoginViewModel loginVM;
            string responseString = string.Empty;
            AccountController accountController = new AccountController();

            if (password != verifyPassword)
            {
                responseString = "Passwords do not match";
            }
            else
            {
                loginVM = new Models.LoginViewModel()
                {
                    Email = userName,
                    Password = password,
                };

                accountController.Login(loginVM, "").Wait();

                if (accountController.User.Identity.IsAuthenticated)
                {
                    responseString = RequestAuthToken(userName, password);
                }
            }


            return responseString;
        }
        */


        [HttpPost]
        public async Task<string> RequestAuthToken()
        {
            string token = string.Empty;
            string reason = string.Empty;
            string success = string.Empty;
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

            //bool.TrueString or bool.FalseString
            success = (token != string.Empty).ToString();
            if (success == bool.TrueString) { reason = "SUCCESS"; }

            //string -> JObject -> string provides formatted Json
            //Quotes needed around everything that isn't a numeric value
            return (JObject.Parse($"{{ \"success\" : \"{success}\", \"reason\" : \"{reason}\", \"token\" : \"{token}\" }}")).ToString();
        }

        [HttpGet]
        public async Task<string> ListFiles(int descriptor, int? pageNumber)
        {
            string token = string.Empty;
            string reason = string.Empty;
            string userName = string.Empty;
            Tuple<string, string> userInfo;
            string[] httpHeaders = Request.Headers.AllKeys;

            if (httpHeaders.Contains("token")) { token = Request.Headers["token"].ToString(); }

            if (token != string.Empty)
            {
                userInfo = _TokenCache.ValidateToken(token);
                if (userInfo != null)
                {
                    reason = await UserSignIn(userInfo.Item1, userInfo.Item2);
                    if (string.IsNullOrEmpty(reason))
                    {
                        BlobManager blobManager = new BlobManager();
                        List<Tuple<string, Uri>> fileList = new List<Tuple<string, Uri>>();

                        switch (descriptor)
                        {
                            case ((int)FileDescriptor.ALL):
                                fileList = blobManager.ListBlobNamesToUrisInUserContainer(userName);
                                fileList.AddRange(blobManager.ListBlobNamesToUrisInPublicContainer());
                                break;
                            case ((int)FileDescriptor.OWNED_ALL):
                                fileList = blobManager.ListBlobNamesToUrisInUserContainer(userName);
                                fileList = blobManager.ListBlobNamesToUrisInPublicContainerOwnedBy(userName);
                                break;
                            case ((int)FileDescriptor.OWNED_PRIVATE):
                                fileList = blobManager.ListBlobNamesToUrisInUserContainer(User.Identity.Name);
                                break;
                            case ((int)FileDescriptor.OWNED_PUBLIC):
                                fileList = blobManager.ListBlobNamesToUrisInPublicContainerOwnedBy(userName);
                                break;
                            case ((int)FileDescriptor.NOT_OWNED_PUBLIC):
                                //List of all public files
                                fileList = blobManager.ListBlobNamesToUrisInPublicContainer();
                                //remove the ones owned by user
                                foreach(Tuple<string, Uri> file in blobManager.ListBlobNamesToUrisInPublicContainerOwnedBy(userName))
                                {
                                    fileList.Remove(file);
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }
                else { reason = "Token not found."; }
            }
            else { reason = "User access token required."; }

            return "";
        }

        //Definitely not void - waiting on requirements
        public void DownloadFile(string authToken, string fileName)//possibly Uri
        { }

        //Definitely not void - waiting on requirements
        public void DownloadFileQR(string authToken, string fileName) //possibly Uri
        { }

        #endregion


        #region Private Methods

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
                if (verified == PasswordVerificationResult.Success)
                {
                    //Try to sign that user in
                    try { SignInManager.SignIn(appUser, false, false); }
                    catch { /* Do nothing - let it fail and return empty token */ }

                    //if login successful - credentials were valid
                    if (!User.Identity.IsAuthenticated || User.Identity.Name != userName)
                    {
                        reason = "User authentication failed.";
                    }
                }
                else { reason = "Password authentication failed."; }
            }
            else { reason = "User information not found."; }

            return reason;
        }

        #endregion
    }
}