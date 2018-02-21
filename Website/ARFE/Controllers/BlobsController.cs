using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Web.Mvc;

using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace ARFE.Controllers
{
    public class BlobsController : Controller
    {
        #region Public

        // GET: Blobs
        public ActionResult Index()
        {
            return View();
        }

        public CloudBlobContainer GetOrCreateBlobContainer(string userName)
        {
            CloudBlobContainer container = GetCloudBlobContainer(userName);
            ViewBag.Success = container.CreateIfNotExists();
            ViewBag.BlobContainerName = container.Name;

            return container;
        }

        public ActionResult UploadBlobToContainer(string userName, string fileName, string filePath)
        {
            CloudBlobContainer container = GetOrCreateBlobContainer(userName);
            CloudBlockBlob blob = container.GetBlockBlobReference(fileName);

#warning warn user about overwrite file if already exists
            blob.UploadFromFile(Path.Combine(filePath, fileName));

            return View();
        }

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

        #endregion


        #region Private


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

        #endregion
    }
}