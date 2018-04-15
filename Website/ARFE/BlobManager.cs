using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Text;

namespace ARFE
{
    public class BlobManager
    {
        #region Constructor

        public BlobManager() { }

        #endregion


        #region Public Methods

        /// <summary>
        /// Given the current Identity logged in user name, get a reference to the users
        /// associated blob container, or create a container if none exists.
        /// </summary>
        /// <param name="userName"></param>
        /// <returns>
        /// *The blob container if able to reference or create it.
        /// *Null if unable to reference or create the container.
        /// </returns>
        public CloudBlobContainer GetOrCreateBlobContainer(string userName)
        {
            CloudBlobContainer container = GetCloudBlobContainer(userName);

            if (!container.CreateIfNotExists() && container == null)
            {
                //Some sort of error
            }
            else
            {
                PurgeOldTemporaryBlobs(userName);
                if (userName != "public") { PurgeOldTemporaryBlobs("public"); }
            }

            return container;
        }


        /// <summary>
        /// Azure Blob Containers have a rigid rule set on Container name format.
        /// https://blogs.msdn.microsoft.com/jmstall/2014/06/12/azure-storage-naming-rules/
        /// Only lowercase letters, numbers, and dashes are allowed.
        /// Only one consecutive dash allowed.
        /// Dash must be surrounded by numers or letters.
        /// Name must be between 3 and 63 characters in length
        /// </summary>
        /// <param name="userName">Container names are created from the user's user name.</param>
        /// <returns></returns>
        public string FormatBlobContainerName(string userName)
        {
            if (userName.Length < 3) return string.Empty;
            if (userName.Length > 62) userName = userName.Substring(0, 62);

            StringBuilder formattedName = new StringBuilder(); 

            foreach (char c in userName.ToLower())
            {
                formattedName.Append((char.IsLetterOrDigit(c)) ? c : '-');
            }

            //remove all cases of --
            while(formattedName.ToString().Contains("--"))
                formattedName = formattedName.Replace("--", "-");
            
            //can't start with -
            if (formattedName[0] == '-')
                formattedName.Remove(0, 1);
            //can't end with -
            if (formattedName[(formattedName.Length - 1)] == '-')
                formattedName.Remove(formattedName.Length - 1, 1);

            if (formattedName.Length < 3) return string.Empty;

            return formattedName.ToString();
        }


        #region privately owned containers


        /// <summary>
        /// Given the current Identity logged in user name and the name of the file to upload,
        /// and the local path to the file, upload the file to the users blob container as a 
        /// new blob.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="fileName"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public bool UploadBlobToUserContainer(string userName, string fileName, string filePath, string description = "", bool overwrite = false)
        {
            CloudBlobContainer container = GetOrCreateBlobContainer(userName);
            CloudBlockBlob blob = container.GetBlockBlobReference(fileName);
            string extension = fileName.Substring(fileName.LastIndexOf('.'));

            if (overwrite || !blob.Exists())
            {
                blob.UploadFromFile(Path.Combine(filePath, fileName));

                UpdateOwnerNameMetaData(blob, userName);
                UpdateDescriptionMetaData(blob, description);

                //primarily want to store .fbx due to small size
                if (extension != ".fbx")
                {
                    UpdateLastAccessedMetaData(blob);
                }

                return true;
            }

            return false;
        }


        public bool DeleteBlobByNameInUserContainer(string userName, string blobName)
        {
            CloudBlobContainer container = GetOrCreateBlobContainer(userName);
            CloudBlockBlob blob = container.GetBlockBlobReference(blobName);

            return blob.DeleteIfExists(DeleteSnapshotsOption.IncludeSnapshots);
        }


        /// <summary>
        /// Given the current Identity logged in user name and the name of the file to download,
        /// return a redirect to allow download of the file.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public string DownloadBlobFromUserContainer(string userName, string fileName)
        {
            CloudBlobContainer container = GetOrCreateBlobContainer(userName);
            CloudBlockBlob blob = container.GetBlockBlobReference(fileName);

            return blob.Exists()
                ? GetBlobDownloadLink(blob)
                : $"Error: {fileName} not found.";
        }


        /// <summary>
        /// Get reference to <paramref name="fileName"/> blob in storage, perform
        /// filetype conversion and upload converted if necessary.  Return download link or error.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="fileName"></param>
        /// <param name="requestExtension"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public string ConvertAndDownloadBlobFromUserContainer(string userName, string fileName, string requestExtension, string path)
        {
            CloudBlobContainer container;
            CloudBlockBlob blob, getBlob;
            string getFileName = $"{fileName.Remove(fileName.LastIndexOf('.'))}-{requestExtension.Substring(1)}.zip";

            container = GetOrCreateBlobContainer(userName);
            blob = container.GetBlockBlobReference(fileName);
            getBlob = container.GetBlockBlobReference(getFileName);

            if (blob.Exists())
            {
                //file exists how we want it in storage, download
                if (fileName.EndsWith(requestExtension)) { return GetBlobDownloadLink(blob); }
                else
                {
                    //see if conversion exists in storage and download
                    if (getBlob.Exists()) { return GetBlobDownloadLink(getBlob); }

                    //conversionResult = [converted blob name | error]
                    string conversionResult = ConvertBlobToBlob(blob, userName, fileName, path, requestExtension);
                    getBlob = container.GetBlockBlobReference(conversionResult);

                    //result was blob
                    if (getBlob.Exists()) { return GetBlobDownloadLink(getBlob); }
                    //result was error
                    else { return conversionResult; }
                }
            }
            else { return $"Error: {fileName} not found."; }
        }


        /// <summary>
        /// Get a list of blob names within the blob container for a given 
        /// Identity user name
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public List<string> ListBlobNamesInUserContainer(string userName)
        {
            List<string> blobNames = new List<string>();

            foreach (Uri u in ListBlobUrisInUserContainer(userName))
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
        public List<Uri> ListBlobUrisInUserContainer(string userName)
        {
            CloudBlobContainer container = GetOrCreateBlobContainer(userName);

            return container.ListBlobs().Select(blob => blob.Uri).ToList();
        }


        public List<CloudBlockBlob> ListBlobsInUserContainer(string userName)
        {
            List<CloudBlockBlob> list = new List<CloudBlockBlob>();
            CloudBlobContainer container = GetOrCreateBlobContainer(userName);

            foreach (IListBlobItem blobItem in container.ListBlobs(null, true))
            {
                CloudBlockBlob blob = (CloudBlockBlob)blobItem;
                blob.FetchAttributes();
                list.Add(blob);
            }

            return list;
        }


        /// <summary>
        /// Get a list of associations of blob names to Uris within
        /// a given container.
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public List<Tuple<string, Uri>> ListBlobNamesToUrisInUserContainer(string userName)
        {
            List<Tuple<string, Uri>> names_to_uris = new List<Tuple<string, Uri>>();
            List<Uri> blob_uris = ListBlobUrisInUserContainer(userName);

            foreach (Uri u in blob_uris)
            {
                string name = GetBlobNameFromUri(u);
                names_to_uris.Add(new Tuple<string, Uri>(name, u));
            }

            return names_to_uris;
        }

        #endregion


        #region public container


        /// <summary>
        /// Given the current Identity logged in user name and the name of the file to upload,
        /// and the local path to the file, upload the file to the users blob container as a 
        /// new blob.
        /// </summary>
        /// <param name="userName">Name of the owner</param>
        /// <param name="fileName">Name of the file</param>
        /// <param name="filePath">Path to the file</param>
        /// <returns></returns>
        public bool UploadBlobToPublicContainer(string userName, string fileName, string filePath, string description = "", bool overwrite = false)
        {
            string extension = fileName.Substring(fileName.LastIndexOf('.'));
            CloudBlobContainer container = GetOrCreateBlobContainer("public");
            CloudBlockBlob blob = container.GetBlockBlobReference($"{FormatBlobContainerName(userName)}-{fileName}");

            if (overwrite || !blob.Exists())
            {
                blob.UploadFromFile(Path.Combine(filePath, fileName));

                UpdateOwnerNameMetaData(blob, userName);
                UpdateDescriptionMetaData(blob, description);

                //primarily want to store .fbx due to small size
                if (extension != ".fbx")
                {
                    UpdateLastAccessedMetaData(blob);
                }

                return true;
            }

            return false;
        }


        /// <summary>
        /// Return a redirect to allow download of the file in the public container.
        /// </summary>
        /// <param name="fileName">Name of the file to download from the public container</param>
        /// <returns>URL to download the blob file</returns>
        public string DownloadBlobFromPublicContainer(string fileName)
        {
            CloudBlobContainer container = GetOrCreateBlobContainer("public");
            CloudBlockBlob blob = container.GetBlockBlobReference(fileName);
            SharedAccessBlobPolicy sharingPolicy = new SharedAccessBlobPolicy()
            {   //can read, allow up to 1 hour to download
                Permissions = SharedAccessBlobPermissions.Read,
                SharedAccessExpiryTime = DateTime.UtcNow.AddHours(1)
            };
            SharedAccessBlobHeaders headers = new SharedAccessBlobHeaders()
            { ContentDisposition = $"attachment;filename={blob.Name}" };

            return $"{blob.Uri}{blob.GetSharedAccessSignature(sharingPolicy, headers)}";
        }


        /// <summary>
        /// Get a list of blob names within the public blob container
        /// </summary>
        /// <returns>A list of blob names from the public container</returns>
        public List<string> ListBlobNamesInPublicContainer()
        {
            List<string> blobNames = new List<string>();

            foreach (Uri u in ListBlobUrisInUserContainer("public"))
            {
                blobNames.Add(GetBlobNameFromUri(u));
            }

            return blobNames;
        }


        public List<Common.FileUIInfo> ListPrivateBlobInfoForUI(string userName)
        {
            List<Common.FileUIInfo> list = new List<Common.FileUIInfo>();
            CloudBlobContainer container = GetOrCreateBlobContainer(userName);


            foreach (IListBlobItem blobItem in container.ListBlobs(null, true))
            {
                Common.FileUIInfo info;
                CloudBlockBlob blob = (CloudBlockBlob)blobItem;
                blob.FetchAttributes();
                string author, description = "No description";

                if (blob.Metadata.ContainsKey("OwnerName"))
                    author = blob.Metadata["OwnerName"];
                else
                    author = "Not recorded";

                if (blob.Metadata.ContainsKey("Description"))
                    description = blob.Metadata["Description"];


                info = new Common.FileUIInfo(blob.Name, author, description, blob.Properties.LastModified.Value.DateTime);
                list.Add(info);
            }

            return list;
        }


        public List<Common.FileUIInfo> ListPublicBlobInfoForUI()
        {
            return ListPrivateBlobInfoForUI("public");
        }


        /// <summary>
        /// Get a list of blob Uris within the public blob container
        /// </summary>
        /// <returns>A list of Uris to the blobs in the public container</returns>
        public List<Uri> ListBlobUrisInPublicContainer()
        {
            CloudBlobContainer container = GetOrCreateBlobContainer("public");

            return container.ListBlobs().Select(blob => blob.Uri).ToList();
        }


        /// <summary>
        /// Get a list of associations of blob names to Uris within the public container
        /// </summary>
        /// <returns>
        /// A list of tuples of blob names and their associated Uris from the 
        /// public container
        /// </returns>
        public List<Tuple<string, Uri>> ListBlobNamesToUrisInPublicContainer()
        {
            List<Tuple<string, Uri>> names_to_uris = new List<Tuple<string, Uri>>();
            List<Uri> blob_uris = ListBlobUrisInUserContainer("public");

            foreach (Uri u in blob_uris)
            {
                string name = GetBlobNameFromUri(u);
                names_to_uris.Add(new Tuple<string, Uri>(name, u));
            }

            return names_to_uris;
        }


        public bool DeleteBlobByNameInPublicContainer(string blobName)
        {
            return DeleteBlobByNameInUserContainer("public", blobName);
        }




        public List<string> ListBlobNamesInPublicContainerOwnedBy(string userName)
        {
            List<string> blobList = new List<string>();

            foreach (Uri u in ListBlobUrisInPublicContainerOwnedBy(userName))
            {
                blobList.Add(GetBlobNameFromUri(u));
            }

            return blobList;
        }


        public List<Uri> ListBlobUrisInPublicContainerOwnedBy(string userName)
        {
            CloudBlobContainer container = GetOrCreateBlobContainer("public");
            List<IListBlobItem> blobList = container.ListBlobs(null, true, BlobListingDetails.Metadata).ToList();
            List<Uri> blobUriList = new List<Uri>();

            foreach (IListBlobItem blobItem in blobList)
            {
                CloudBlockBlob blob = (CloudBlockBlob)blobItem;
                if (blob.Metadata["OwnerName"] == userName)
                    blobUriList.Add(blob.Uri);
            }

            return blobUriList;
        }


        public List<Tuple<string, Uri>> ListBlobNamesToUrisInPublicContainerOwnedBy(string userName)
        {
            List<Tuple<string, Uri>> names_to_uris = new List<Tuple<string, Uri>>();
            List<Uri> blob_uris = ListBlobUrisInPublicContainerOwnedBy(userName);

            foreach (Uri u in blob_uris)
            {
                string name = GetBlobNameFromUri(u);
                names_to_uris.Add(new Tuple<string, Uri>(name, u));
            }

            return names_to_uris;
        }

        #endregion


        #endregion


        #region Private Methods

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


        private string GetBlobDownloadLink(CloudBlockBlob blob)
        {
            SharedAccessBlobPolicy sharingPolicy = new SharedAccessBlobPolicy()
            {   //can read, allow up to 1 hour to download
                Permissions = SharedAccessBlobPermissions.Read,
                SharedAccessExpiryTime = DateTime.UtcNow.AddHours(1)
            };
            SharedAccessBlobHeaders headers = new SharedAccessBlobHeaders()
            { ContentDisposition = $"attachment;filename={blob.Name}" };


            if (!blob.Name.EndsWith(".fbx"))
            {
                if (blob.Metadata.ContainsKey("LastAccessed"))
                {
                    blob.Metadata["LastAccessed"] = DateTime.UtcNow.ToString();
                }
                blob.SetMetadata();
            }

            return $"{blob.Uri}{blob.GetSharedAccessSignature(sharingPolicy, headers)}";
        }


        private string ConvertBlobToBlob(CloudBlockBlob fromBlob, string userName, string fileName, string path, string requestExtension)
        {
            Guid subDir;
            CloudBlockBlob toBlob = null;
            string returnMessage = string.Empty;
            CloudBlobContainer container = fromBlob.Parent.Container;
            string getFileName = $"{fileName.Remove(fileName.LastIndexOf('.'))}";
            string zipFolderName = $"{getFileName}-{requestExtension.Substring(1)}.zip";

            UploadedFileCache uploadedFiles = UploadedFileCache.GetInstance();
            if (!uploadedFiles.DeleteAndRemoveFile(userName, fileName))
            {
                returnMessage = "Error: Unable to download .fbx blob for conversion.";
            }
            else if ((subDir = uploadedFiles.SaveFile(fromBlob, userName, fileName)) != Guid.Empty)
            {
                if (ConvertAndZip(path, subDir.ToString(), requestExtension, fileName, zipFolderName))
                {
                    //upload converted file to blob storage
                    if (UploadBlobToUserContainer(userName, zipFolderName, path))
                    {   //get reference to blob and get download link
                        toBlob = container.GetBlockBlobReference(zipFolderName);

                        returnMessage = toBlob.Name;
                    }
                    else { returnMessage = $"Error: unable to process converted file: {getFileName}."; }
                }
                else { returnMessage = $"Error: unable to convert {fileName} to type {requestExtension}."; }
            }
            else { returnMessage = "Error: Unable to download .fbx blob for conversion."; }

            return returnMessage;
        }


        private void UpdateOwnerNameMetaData(CloudBlockBlob blob, string userName)
        {
            //overwrite same blob will keep key
            if (!blob.Metadata.ContainsKey("OwnerName"))
            {
                blob.Metadata.Add(new KeyValuePair<string, string>("OwnerName", userName));
                blob.SetMetadata();
            }
        }

        private void UpdateLastAccessedMetaData(CloudBlockBlob blob)
        {
            if (!blob.Metadata.ContainsKey("LastAccessed"))
            {
                blob.Metadata.Add(new KeyValuePair<string, string>("LastAccessed", DateTime.UtcNow.ToString()));
            }
            else
            {
                blob.Metadata["LastAccessed"] = DateTime.UtcNow.ToString();
            }
            blob.SetMetadata();
        }

        private void UpdateDescriptionMetaData(CloudBlockBlob blob, string description)
        {
            description = (string.IsNullOrEmpty(description) ? "No description" : description);

            if (!blob.Metadata.ContainsKey("Description"))
                blob.Metadata.Add(new KeyValuePair<string, string>("Description", description));
            else
                blob.Metadata["Description"] = description;

            blob.SetMetadata();
        }


        private bool ConvertAndZip(string path, string subDir, string requestExtension, string fileName, string zipFolderName)
        {
            bool converted = false;
            char sep = Path.DirectorySeparatorChar;
            string noExtension = fileName.Remove(fileName.LastIndexOf('.'));
            FileConverter converter = new FileConverter($"UploadedFiles{sep}{subDir}", $"UploadedFiles{sep}{subDir}");

            switch (requestExtension) //extensible for more filetype conversion download options
            {
                case ".obj": // convert to .obj
                    converted = converter.ConvertToOBJ(fileName);
                    break;
                case ".dae": // convert to .dae
                    converted = converter.ConvertToDAE(fileName);
                    break;
                case ".fbx": // convert to .fbx
                    converted = converter.ConvertToFBX(fileName);
                    break;
                case ".stl": // convert to .stl
                    converted = converter.ConvertToSTL(fileName);
                    break;
                case ".ply": // convert to .ply
                    converted = converter.ConvertToPLY(fileName);
                    break;
                default: break;
            }

            if (converted)
            {
                //delete file converted from
                File.Delete(Path.Combine(path, subDir, fileName));

                //delete intermediate .dae file
                if (requestExtension != ".dae"
                    && File.Exists(Path.Combine(path, subDir, $"{noExtension}.dae")))
                {
                    File.Delete(Path.Combine(path, subDir, $"{noExtension}.dae"));
                }

                System.IO.Compression.ZipFile.CreateFromDirectory(Path.Combine(path, subDir), Path.Combine(path, zipFolderName));
            }

            return converted;
        }


        private void PurgeOldTemporaryBlobs(string userName)
        {
            CloudBlobContainer container = GetCloudBlobContainer(userName);
            try
            {
                List<IListBlobItem> blobList = container.ListBlobs(null, true, BlobListingDetails.Metadata).ToList();

                foreach (IListBlobItem blobItem in blobList)
                {
                    CloudBlockBlob blob = (CloudBlockBlob)blobItem;
                    if (blob.Metadata.Keys.Contains("LastAccessed"))
                    {
                        DateTime now = DateTime.UtcNow;
                        DateTime lastAccessed = DateTime.SpecifyKind(DateTime.Parse(blob.Metadata["LastAccessed"]), DateTimeKind.Utc);

                        if (lastAccessed.AddHours(.5) < now) //over 30 minutes old
                        {
                            blob.Delete(DeleteSnapshotsOption.IncludeSnapshots);
                        }
                    }
                }
            }
            catch { /*Ignore - can fail on new users*/ }
        }


        #endregion
    }
}