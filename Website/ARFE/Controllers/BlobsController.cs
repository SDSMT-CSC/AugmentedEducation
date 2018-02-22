using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Collections.Generic;

using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace ARFE.Controllers
{
    public class BlobsController : Controller
    {
        #region Public

        /// <summary>
        /// Given the current Identity logged in user name, get a reference to the users
        /// associated blob container, or create a container if none exists.
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public CloudBlobContainer GetOrCreateBlobContainer(string userName)
        {
            CloudBlobContainer container = GetCloudBlobContainer(userName);
            ViewBag.Success = container.CreateIfNotExists();
            ViewBag.BlobContainerName = container.Name;

            return container;
        }

        /// <summary>
        /// Given the current Identity logged in user name and the name of the file to upload,
        /// and the local path to the file, upload the file to the users blob container as a 
        /// new blob.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="fileName"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public ActionResult UploadBlobToContainer(string userName, string fileName, string filePath)
        {
            CloudBlobContainer container = GetOrCreateBlobContainer(userName);
            CloudBlockBlob blob = container.GetBlockBlobReference(fileName);

#warning warn user about overwrite file if already exists
            blob.UploadFromFile(Path.Combine(filePath, fileName));

            return View();
        }

        /// <summary>
        /// Given the current Identity logged in user name and the name of the file to download,
        /// return a redirect to allow download of the file.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public ActionResult DownloadBlobFromContainer(string userName, string fileName)
        {
            CloudBlobContainer container = GetOrCreateBlobContainer(userName);
            CloudBlockBlob blob = container.GetBlockBlobReference(fileName);
            SharedAccessBlobPolicy sharingPolicy = new SharedAccessBlobPolicy()
            {   //can read, allow up to 1 hour to download
                Permissions = SharedAccessBlobPermissions.Read,
                SharedAccessExpiryTime = DateTime.UtcNow.AddHours(1)
            };
            SharedAccessBlobHeaders headers = new SharedAccessBlobHeaders()
            { ContentDisposition = $"attachment;filename={blob.Name}" };

            return Redirect($"{blob.Uri}{blob.GetSharedAccessSignature(sharingPolicy, headers)}");
        }

        /// <summary>
        /// Get a list of blob names within the blob container for a given 
        /// Identity user name
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public List<string> ListBlobNamesInContainer(string userName)
        {
#warning This is roughly where you would be looking for content
            List<string> blobNames = new List<string>();

            foreach (Uri u in ListBlobUrisInContainer(userName))
            {
                blobNames.Add(GetBlobNameFromUri(u));
            }

            return blobNames;
        }

        /// <summary>
        /// Get a list of blob Uris within the blob container for a given 
        /// Identity user name
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public List<Uri> ListBlobUrisInContainer(string userName)
        {
            CloudBlobContainer container = GetCloudBlobContainer(userName);

            return container.ListBlobs().Select(blob => blob.Uri).ToList();
        }

        /// <summary>
        /// Get a list of associations of blob names to Uris within
        /// a given container.
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public List<Tuple<string, Uri>> ListBlobNamesToUris(string userName)
        {
            List<Tuple<string, Uri>> names_to_uris = new List<Tuple<string, Uri>>();
            List<Uri> blob_uris = ListBlobUrisInContainer(userName);

            foreach(Uri u in blob_uris)
            {
                string name = GetBlobNameFromUri(u);
                names_to_uris.Add(new Tuple<string, Uri>(name, u));
            }

            return names_to_uris;
        }

        #endregion


        #region Private

        /// <summary>
        /// Given the signed in Identity username, get or create the associated Blob Container
        /// for that user.
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        private CloudBlobContainer GetCloudBlobContainer(string userName)
        {
            //Found in Web.config
            string blobConnectionString = CloudConfigurationManager.GetSetting("augmentededucationblob_AzureStorageConnectionString");
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(blobConnectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            return blobClient.GetContainerReference(FormatBlobContainerName(userName));
        }

        /// <summary>
        /// Azure Blob Containers have a rigid rule set on Container name format.
        /// Only lowercase letters, numbers, and dashes are allowed.
        /// Only one consecutive dash allowed.
        /// Dash must be surrounded by numers or letters.
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        private string FormatBlobContainerName(string userName)
        {
            string formattedName = userName.ToLower();

            formattedName = formattedName.Replace('@', '-');
            formattedName = formattedName.Replace('.', '-');
            formattedName = formattedName.Replace("--", "-");
            if (formattedName[0] == '-')
                formattedName.Remove(0, 1);
            if (formattedName[(formattedName.Length - 1)] == '-')
                formattedName.Remove(formattedName.Length - 1);

            return formattedName;
        }

        /// <summary>
        /// Given the Uri for a blob within a container, extract the 
        /// name from after the last '/'
        /// </summary>
        /// <param name="blobUri"></param>
        /// <returns></returns>
        private string GetBlobNameFromUri(Uri blobUri)
        {
            string uri_string = blobUri.ToString();
            string[] sp = uri_string.Split('/');

            return sp[sp.Length - 1];
        }

        #endregion
    }
}