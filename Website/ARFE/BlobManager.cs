using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.IO.Compression;
using System.Collections.Generic;

using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace ARFE
{
    public class BlobManager
    {
        #region Public

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
        /// Only lowercase letters, numbers, and dashes are allowed.
        /// Only one consecutive dash allowed.
        /// Dash must be surrounded by numers or letters.
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public string FormatBlobContainerName(string userName)
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
        public bool UploadBlobToUserContainer(string userName, string fileName, string filePath, bool overwrite = false)
        {
            CloudBlobContainer container = GetOrCreateBlobContainer(userName);
            CloudBlockBlob blob = container.GetBlockBlobReference(fileName);
            string extension = fileName.Substring(fileName.LastIndexOf('.'));

            if (!blob.Exists())
            {
                blob.UploadFromFile(Path.Combine(filePath, fileName));
               
                //overwrite same blob will keep key
                if (!blob.Metadata.ContainsKey("OwnerName"))
                {
                    blob.Metadata.Add(new KeyValuePair<string, string>("OwnerName", userName));
                    blob.SetMetadata();
                }
                //primarily want to store .fbx due to small size
                if (extension != ".fbx")
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

                    //conversion Result may be converted blob name or error
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
        public bool UploadBlobToPublicContainer(string userName, string fileName, string filePath, bool overwrite = false)
        {
            CloudBlobContainer container = GetOrCreateBlobContainer("public");
            CloudBlockBlob blob = container.GetBlockBlobReference($"{FormatBlobContainerName(userName)}-{fileName}");

            if (!blob.Exists())
            {
                blob.UploadFromFile(Path.Combine(filePath, fileName));
                blob.Metadata.Add(new KeyValuePair<string, string>("OwnerName", userName));
                blob.SetMetadata();
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
            CloudBlockBlob toBlob = null;
            string returnMessage = string.Empty;
            CloudBlobContainer container = fromBlob.Parent.Container;
            string getFileName = $"{fileName.Remove(fileName.LastIndexOf('.'))}";
            string folderName = $"{fileName.Remove(fileName.LastIndexOf('.'))}-{requestExtension.Substring(1)}";
            string zipFolderName = $"{folderName}.zip";

            while (File.Exists(Path.Combine(path, fileName)))
            {   //remove other file if not in use
                try
                {
                    File.Delete(Path.Combine(path, fileName));
                }
                //sleep 50  miliseconds - don't waste resources just looping
                catch { Thread.Sleep(50); }
            }

            //path parameter is absolute
            using (Stream fileStream = File.OpenWrite(Path.Combine(path, fileName)))
            {   //Have to use DownloadToStream - DownloadToFile results in access denied error
                fromBlob.DownloadToStream(fileStream);
            }

            if (ConvertAndZip(path, requestExtension, fileName, folderName, getFileName, zipFolderName))
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

            CleanupUploadedFiles(path, fileName, folderName, zipFolderName);

            return returnMessage;
        }


        private bool ConvertAndZip(string path, string requestExtension, string fileName, string folderName, string getFileName, string zipFolderName)
        {
            bool converted = false;
            List<string> producedExtensions = new List<string>();
            FileConverter converter = new FileConverter("UploadedFiles", "UploadedFiles");

            switch (requestExtension) //extensible for more filetype conversion download options
            {
                case ".obj": // convert to .obj
                    producedExtensions.Add(".obj");
                    producedExtensions.Add(".mtl");
                    converted = converter.ConvertToOBJ(fileName);
                    break;
                case ".dae": // convert to .dae
                    producedExtensions.Add(".dae");
                    converted = converter.ConvertToDAE(fileName);
                    break;
                case ".fbx": // convert to .fbx
                    producedExtensions.Add(".fbx");
                    converted = converter.ConvertToFBX(fileName);
                    break;
                case ".stl": // convert to .stl
                    producedExtensions.Add(".stl");
                    converted = converter.ConvertToSTL(fileName);
                    break;
                case ".ply": // convert to .ply
                    producedExtensions.Add(".ply");
                    converted = converter.ConvertToPLY(fileName);
                    break;
                default: break;
            }

            if (converted)
            {
                string[] allFiles = Directory.GetFiles(path);
                DirectoryInfo newDir = Directory.CreateDirectory(Path.Combine(path, folderName));

                foreach (string file in allFiles)
                {
                    string nameWithoutPath = file.Substring(file.LastIndexOf(@"\") + 1);
                    foreach (string ext in producedExtensions)
                    {
                        if (nameWithoutPath.StartsWith(getFileName) && nameWithoutPath.EndsWith(ext))
                        {
                            File.Move(file, $@"{newDir.FullName}\{nameWithoutPath}");
                            break;
                        }
                    }
                }

                ZipFile.CreateFromDirectory(newDir.FullName, $@"{newDir.Parent.FullName}\{zipFolderName}");
            }

            return converted;
        }

        private void CleanupUploadedFiles(string path, string fileName, string folderName, string zipDirectory)
        {
            string[] allFiles = Directory.GetFiles(path);
            string noextension = fileName.Substring(0, fileName.IndexOf('.'));

            //Clear up ~/UploadedFiles folder
            foreach (string file in allFiles)
            {
                if (!file.EndsWith(".dll") && !file.EndsWith(".exe"))
                {
                    string nameWithoutPath = file.Replace($@"{path}\", "");

                    if (nameWithoutPath.StartsWith(noextension) && File.Exists(file))
                    {
                        File.Delete(file);
                    }
                }
            }

            //delete all contents of folder that made .zip and folder
            if (Directory.Exists(Path.Combine(path, folderName)))
            {
                foreach (string file in Directory.GetFiles(Path.Combine(path, folderName)))
                {
                    File.Delete(file);
                }
                Directory.Delete(Path.Combine(path, folderName));
            }
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