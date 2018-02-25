using AuthenticationTokenCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;


namespace ARFE.Controllers
{
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


        #region Public Methods

        /*
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
        [Route("/MobileAuth/GetToken/")]
        public string RequestAuthToken()
        {
            string token = string.Empty;
            Models.LoginViewModel loginVM;
            string password = string.Empty;
            string userName = string.Empty;
            AccountController accountController;

            HttpRequestMessage httpRequest = new HttpRequestMessage();
            var headers = httpRequest.Headers;

            if (headers.Contains("UserName")) { userName = headers.GetValues("UserName").First(); }
            if (headers.Contains("Password")) { password = headers.GetValues("Password").First(); }

            if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(password))
            {
                accountController = new AccountController();
                loginVM = new Models.LoginViewModel()
                {
                    Password = password,
                    Email = userName,
                };

                accountController.Login(loginVM, "").Wait();

                if (accountController.User.Identity.IsAuthenticated)
                {
                    token = _TokenCache.GenerateToken(userName, password);
                }
            }

            return token;
        }

        [HttpGet]
        public string ViewFiles(string authToken, int descriptor)
        {
            //waiting on requirements

            string responseString = string.Empty;

            //User = AuthTokenLookup(authToken)
            if (!User.Identity.IsAuthenticated)
            { responseString = "bad token"; }
            else
            {
                BlobManager blobManager = new BlobManager();
                List<Tuple<string, Uri>> fileList = new List<Tuple<string, Uri>>();

                switch (descriptor)
                {
                    case ((int)FileDescriptor.ALL):
                        fileList = blobManager.ListBlobNamesToUrisInUserContainer(User.Identity.Name);
                        fileList.AddRange(blobManager.ListBlobNamesToUrisInPublicContainer());
                        break;
                    case ((int)FileDescriptor.OWNED_ALL):
                        //fileList = blobsController.ListBlobNamesToUris(User.Identity.Name);
                        break;
                    case ((int)FileDescriptor.OWNED_PRIVATE):
                        fileList = blobManager.ListBlobNamesToUrisInUserContainer(User.Identity.Name);
                        break;
                    case ((int)FileDescriptor.OWNED_PUBLIC):
                        //fileList = blobsController.ListBlobNamesToUris(User.Identity.Name);
                        break;
                    case ((int)FileDescriptor.NOT_OWNED_PUBLIC):
                        //fileList = blobsController.ListBlobNamesToUris(User.Identity.Name);
                        break;
                    default:
                        break;
                }

                //Convert to JSON in responseString??
            }

            return responseString;
        }

        //Definitely not void - waiting on requirements
        public void DownloadFile(string authToken, string fileName)//possibly Uri
        { }

        //Definitely not void - waiting on requirements
        public void DownloadFileQR(string authToken, string fileName) //possibly Uri
        { }

        #endregion
    }
}