using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ARFE.Controllers
{
    public class MobileAuthController : Controller
    {
        enum FileDescriptor
        {
            ALL = 0,
            OWNED_ALL,
            OWNED_PRIVATE,
            OWNED_PUBLIC,
            NOT_OWNED_PUBLIC,
        };

        //waiting on requirements
        public string CreateAccount(string userName, string password, string verifyPassword)
        {
            string responseString = string.Empty;

            if(password != verifyPassword)
            {
                responseString = "Passwords do not match";
            }

            //if( able to create account)
            //{
            //    responseString = RequestAuthToken(userName, password);
            //}


            return responseString;
        }

        //waiting on requirements
        public string RequestAuthToken(string userName, string password)
        {
            string authToken = string.Empty;


            return authToken;
        }

        //waiting on requirements
        public string ViewFiles(string authToken, int descriptor)
        {
            //waiting on requirements

            string responseString = string.Empty;

            //User = AuthTokenLookup(authToken)
            if(!User.Identity.IsAuthenticated)
            { responseString = "bad token"; }
            else
            {
                BlobManager blobManager = new BlobManager();
                List<Tuple<string, Uri>> fileList = new List<Tuple<string, Uri>>();

                switch(descriptor)
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
    }
}