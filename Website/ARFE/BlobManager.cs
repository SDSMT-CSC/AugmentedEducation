//System .dlls
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;

//NuGet packages
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

//Project references
using Common;

namespace ARFE
{
    /// <summary>
    /// This class is for managing all transactions between the AE website and 
    /// Azure blob storage.  All user containers, user blob creation/deletion, 
    /// blob metadata, blob permissions, and blob listings are handled here.
    /// </summary>
    public class BlobManager
    {
        #region Constructor

        /// <summary>
        /// Default constructor. No special parameters or attributes
        /// are needed to use this class.
        /// </summary>
        public BlobManager() { }

        #endregion


        #region Public Methods

        /// <summary>
        /// Given the user name, get a reference to the user's
        /// associated blob container, or create a container if none exists.
        /// </summary>
        /// <param name="userName">The name of the user and associated blob container.</param>
        /// <returns>
        ///     <ul>
        ///         <li>The blob container if able to reference or create it.</li>
        ///         <li>Null if unable to reference or create the container.</li>
        ///     </ul>
        /// </returns>
        public CloudBlobContainer GetOrCreateBlobContainer(string userName)
        {
            CloudBlobContainer container = GetCloudBlobContainer(userName);

            RemoveExpiredTemporaryBlobs();
            container.CreateIfNotExists();

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
        /// <returns>
        ///     <ul>
        ///         <li>string.Empty - The provided name was unsalvageable.</li>
        ///         <li>The appropriately formatted container name.</li>
        ///     </ul>
        /// </returns>
        public string FormatBlobContainerName(string userName)
        {
            if (userName.Length < 3) return string.Empty;
            if (userName.Length > 62) userName = userName.Substring(0, 62);

            StringBuilder formattedName = new StringBuilder();

            foreach (char c in userName.ToLower())
                formattedName.Append((char.IsLetterOrDigit(c)) ? c : '-');

            //remove all cases of --
            while (formattedName.ToString().Contains("--"))
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


        /// <summary>
        /// Lazily delete blobs that are stored as the result of conversion requests.
        /// Maintain the small .fbx files but delete the extras as they become old.
        /// </summary>
        public static void RemoveExpiredTemporaryBlobs()
        {
            //Found in Web.config
            string blobConnectionString = CloudConfigurationManager.GetSetting("augmentededucationblob_AzureStorageConnectionString");
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(blobConnectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            foreach (CloudBlobContainer container in blobClient.ListContainers())
            {
                List<IListBlobItem> blobList = container.ListBlobs(null, true, BlobListingDetails.Metadata).ToList();

                foreach (IListBlobItem blobItem in blobList)
                {
                    CloudBlockBlob blob = (CloudBlockBlob)blobItem;
                    if (blob.Metadata.Keys.Contains("LastAccessed"))
                    {
                        //compare .UTC now to .UTC time stored for blob
                        DateTime now = DateTime.UtcNow;
                        DateTime lastAccessed = DateTime.SpecifyKind(DateTime.Parse(blob.Metadata["LastAccessed"]), DateTimeKind.Utc);

                        //last accessed over 30 minutes ago
                        if (lastAccessed.AddMinutes(30) < now)
                            blob.Delete(DeleteSnapshotsOption.IncludeSnapshots);
                    }
                }
            }

        }


        #region privately owned containers

        /// <summary>
        /// Upload a file as a blob to cloud storage in the provided user's blob container.
        /// </summary>
        /// <param name="userName">The name of the user and associated blob container.</param>
        /// <param name="fileName">The name of the file to upload to blob storage.</param>
        /// <param name="filePath">The path to the file to upload to blob storage.</param>
        /// <param name="description">The user's description of the file.</param>
        /// <param name="overwrite">Whether or not to overwrite any previously existing blobs with the same name.</param>
        /// <returns></returns>
        public bool UploadBlobToUserContainer(string userName, string fileName, string filePath, string description = "", bool overwrite = false)
        {
            CloudBlobContainer container = GetOrCreateBlobContainer(userName);
            CloudBlockBlob blob = container.GetBlockBlobReference(fileName);
            string extension = fileName.Substring(fileName.LastIndexOf('.'));

            //if doesn't exist or user said to overwrite
            if (overwrite || !blob.Exists())
            {
                blob.UploadFromFile(Path.Combine(filePath, fileName));

                //primarily want to store .fbx due to small size
                //access time used to know when to auto-remove other files
                if (extension != ".fbx")
                    UpdateLastAccessedMetaData(blob);
                //set owner and description properties.
                UpdateOwnerNameMetaData(blob, userName);
                UpdateDescriptionMetaData(blob, description);

                return true;
            }

            return false;
        }


        /// <summary>
        /// Delete a blob by name stored in the user's container.  Blob's stored
        /// in the public container can only be deleted by the owner (handled by
        /// the UI).
        /// </summary>
        /// <param name="userName">The name of the user/container to delete the blob from.</param>
        /// <param name="blobName">The name of the blob to delete.</param>
        /// <returns></returns>
        public bool DeleteBlobByNameInUserContainer(string userName, string blobName)
        {
            CloudBlobContainer container = GetOrCreateBlobContainer(userName);
            CloudBlockBlob blob = container.GetBlockBlobReference(blobName);

            //Blobs must be deleted by including snapshots. 
            //Otherwise, they won't actually delete.
            return blob.DeleteIfExists(DeleteSnapshotsOption.IncludeSnapshots);
        }


        /// <summary>
        /// Attempt to download the requested file from blob storage.  If the file does not exist,
        /// perform the appropriate file conversion and give a downloadable link to the converted
        /// file that matches the user's request.
        /// </summary>
        /// <param name="userName">The name of the user/blob container for the download request.</param>
        /// <param name="fileName">The name of the blob for the download request.</param>
        /// <param name="requestExtension">The requested file type.</param>
        /// <param name="path">The file path for download/conversion if necessary.</param>
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
        /// Get a list of blob Uris within the blob container for a given user name.
        /// </summary>
        /// <param name="userName">The user name to look up associated blob Uris.</param>
        /// <returns>
        /// The list of blob Uris for the give user.
        /// </returns>
        public List<Uri> ListBlobUrisInUserContainer(string userName)
        {
            CloudBlobContainer container = GetOrCreateBlobContainer(userName);

            return container.ListBlobs().Select(blob => blob.Uri).ToList();
        }


        /// <summary>
        /// Get a list of associations of blob names to Uris for a given user name/container.
        /// </summary>
        /// <param name="userName">The user/container name.</param>
        /// <returns>
        /// A list of associations of blob names to Uris.
        /// </returns>
        public List<Tuple<string, Uri>> ListBlobNamesToUrisInUserContainer(string userName)
        {
            List<Tuple<string, Uri>> names_to_uris = new List<Tuple<string, Uri>>();
            List<Uri> blob_uris = ListBlobUrisInUserContainer(userName);

            foreach (Uri u in blob_uris)
            {
                names_to_uris.Add(new Tuple<string, Uri>(GetBlobNameFromUri(u), u));
            }

            return names_to_uris;
        }

        #endregion


        #region public container

        /// <summary>
        /// Given the current logged in user name and the name of the file to upload,
        /// and the local path to the file, upload the file to the public blob container as a 
        /// new blob owned by the user.
        /// </summary>
        /// <param name="userName">Name of the owner</param>
        /// <param name="fileName">Name of the file</param>
        /// <param name="filePath">Path to the file</param>
        /// <returns>
        ///     <ul>
        ///         <li>True: The file was successfully uploaded.</li>
        ///         <li>False: The file failed to upload.</li>
        ///     </ul>
        /// </returns>
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
                    UpdateLastAccessedMetaData(blob);
            }
            return true;
        }


        /// <summary>
        /// Iterate through all blobs in a given user's container. For each blob
        /// get the meta information (blob Name, author name, description, last modify time)
        /// about that blob as a POCO (Plain Old C# Object) and create a list of all of 
        /// the blob's information.
        /// </summary>
        /// <param name="userName">The name of the user/blob container.</param>
        /// <returns>
        /// A list of POCOs containing blob metaData information. 
        /// </returns>
        public List<FileUIInfo> ListPrivateBlobInfoForUI(string userName)
        {
            List<FileUIInfo> list = new List<FileUIInfo>();
            CloudBlobContainer container = GetOrCreateBlobContainer(userName);

            foreach (IListBlobItem blobItem in container.ListBlobs(null, true))
            {
                string author = "Not recorded";
                string description = "No description";
                CloudBlockBlob blob = (CloudBlockBlob)blobItem;

                blob.FetchAttributes();

                if (blob.Metadata.ContainsKey("OwnerName"))
                    author = blob.Metadata["OwnerName"];

                if (blob.Metadata.ContainsKey("Description"))
                    description = blob.Metadata["Description"];

                list.Add(new FileUIInfo(blob.Name, author, description, blob.Properties.LastModified.Value.DateTime));
            }

            return list;
        }


        /// <summary>
        /// Make a call to <see cref="ListPrivateBlobInfoForUI(string)"/> with the userName
        /// "public" to query the public container.
        /// </summary>
        /// <returns>
        /// A list of POCOs containing blob metaData information. 
        /// </returns>
        public List<FileUIInfo> ListPublicBlobInfoForUI()
        {
            return ListPrivateBlobInfoForUI("public");
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


        /// <summary>
        /// Gets a list of all blob Uris in the public container and filters by the ones
        /// that are owned by the provided user name.
        /// </summary>
        /// <param name="userName">The name of the owner to filter the list by.</param>
        /// <returns>
        /// A list of all Uris to blobs that are owned by the user with the provided user name.
        /// </returns>
        public List<Uri> ListBlobUrisInPublicContainerOwnedBy(string userName)
        {
            List<Uri> blobUriList = new List<Uri>();
            CloudBlobContainer container = GetOrCreateBlobContainer("public");
            List<IListBlobItem> blobList = container.ListBlobs(null, true, BlobListingDetails.Metadata).ToList();

            foreach (IListBlobItem blobItem in blobList)
            {
                CloudBlockBlob blob = (CloudBlockBlob)blobItem;
                //ignore blobs that don't have OwnerName metaData set - no way of knowing
                if (blob.Metadata.ContainsKey("OwnerName") && blob.Metadata["OwnerName"] == userName)
                    blobUriList.Add(blob.Uri);
            }

            return blobUriList;
        }


        /// <summary>
        /// Get all Uris for blobs owned by the given user name by calling 
        /// <see cref="ListBlobUrisInPublicContainerOwnedBy(string)"/>. For each Uri.  The blob
        /// name is the last portion of the string following the last '/' character.
        /// </summary>
        /// <param name="userName">The name of the user to filter by.</param>
        /// <returns>
        /// A list of object pairs of (blob Name, blob Uri) for each blob in the public container 
        /// owned by the user.
        /// </returns>
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


        /// <summary>
        /// Given the reference to the requested blob.  Create a sharing token
        /// that allows the blob to be downloaded.  Return the downloadable link
        /// to the requestor.
        /// </summary>
        /// <param name="blob">The binary object stored on Azure.</param>
        /// <returns>The URL that gives download access to the stored blob file.</returns>
        private string GetBlobDownloadLink(CloudBlockBlob blob, DateTime? utcExpirationTime = null)
        {
            //caller can specify expiration time or default to 1 hour
            DateTime expires = utcExpirationTime.HasValue ? utcExpirationTime.Value : DateTime.UtcNow.AddHours(1);

            SharedAccessBlobPolicy sharingPolicy = new SharedAccessBlobPolicy()
            {   //can read, allow up to 1 hour to download
                Permissions = SharedAccessBlobPermissions.Read,
                SharedAccessExpiryTime = expires
            };
            SharedAccessBlobHeaders headers = new SharedAccessBlobHeaders()
            { ContentDisposition = $"attachment;filename={blob.Name}" };

            //Not default file - track access time for auto deletion
            if (!blob.Name.EndsWith(".fbx")
                && blob.Metadata.ContainsKey("LastAccessed"))
            {
                blob.Metadata["LastAccessed"] = DateTime.UtcNow.ToString();
                blob.SetMetadata();
            }

            //download link
            return $"{blob.Uri}{blob.GetSharedAccessSignature(sharingPolicy, headers)}";
        }


        /// <summary>
        /// The user has requested a blob to be downloaded as a file format that doesn't currently
        /// exist in cloud storage.  Download the existing format, run conversion software, and 
        /// re-upload the zipped contents of the conversion.
        /// </summary>
        /// <param name="fromBlob"></param>
        /// <param name="userName"></param>
        /// <param name="fileName"></param>
        /// <param name="path"></param>
        /// <param name="requestExtension"></param>
        /// <returns>
        ///     <ul>
        ///         <li>The error detail about the failed conversion</li>
        ///         <li>The name of the zip file blob containing conversion results.</li>
        ///     </ul>
        /// </returns>
        private string ConvertBlobToBlob(CloudBlockBlob fromBlob, string userName, string fileName, string path, string requestExtension)
        {
            Guid subDir;
            CloudBlockBlob toBlob = null;
            CloudBlobContainer container = fromBlob.Parent.Container;
            string getFileName = $"{fileName.Remove(fileName.LastIndexOf('.'))}";
            string zipFolderName = $"{getFileName}-{requestExtension.Substring(1)}.zip";

            UploadedFileCache uploadedFiles = UploadedFileCache.GetInstance();
            if (!uploadedFiles.DeleteAndRemoveFile(userName, fileName))
            {   //couldn't delete old tracked instance
                return "Error: Unable to download .fbx blob for conversion.";
            }
            else if ((subDir = uploadedFiles.SaveFile(fromBlob, userName, fileName)) != Guid.Empty)
            {   //successfully downloaded original - run conversion and zip results
                if (ConvertAndZip(path, subDir.ToString(), requestExtension, fileName, zipFolderName))
                {
                    //upload converted zipped file to blob storage
                    if (UploadBlobToUserContainer(userName, zipFolderName, path))
                    {   //get reference to blob and get download link
                        toBlob = container.GetBlockBlobReference(zipFolderName);

                        return toBlob.Name;
                    }
                    else { return $"Error: unable to process converted file: {getFileName}."; }
                }
                else { return $"Error: unable to convert {fileName} to type {requestExtension}."; }
            }
            else { return "Error: Unable to download .fbx blob for conversion."; }
        }


        /// <summary>
        /// Set the blob's OwnerName metaData property to the userName provided.
        /// If the userName provided is empty, nothing happens: Blob metaData values
        /// cannot be empty.
        /// </summary>
        /// <param name="blob">The blob whose metaData will be altered.</param>
        /// <param name="userName">The userName to be recorded as the blob owner.</param>
        private void UpdateOwnerNameMetaData(CloudBlockBlob blob, string userName)
        {
            //Metadata fields can't be empty
            if (!string.IsNullOrEmpty(userName))
            {
                //overwrite same blob will keep key
                if (!blob.Metadata.ContainsKey("OwnerName"))
                    blob.Metadata.Add(new KeyValuePair<string, string>("OwnerName", userName));
                else
                    blob.Metadata["OwnerName"] = userName;

                blob.SetMetadata();
            }
        }


        /// <summary>
        /// Set the blob's LastAccessed metaData property to DateTime.UtcNow.
        /// This is used to track blobs whose storage should not be persisted.
        /// </summary>
        /// <param name="blob">The blob whose metaData will be altered.</param>
        private void UpdateLastAccessedMetaData(CloudBlockBlob blob)
        {
            if (!blob.Metadata.ContainsKey("LastAccessed"))
                blob.Metadata.Add(new KeyValuePair<string, string>("LastAccessed", DateTime.UtcNow.ToString()));
            else
                blob.Metadata["LastAccessed"] = DateTime.UtcNow.ToString();

            blob.SetMetadata();
        }


        /// <summary>
        /// Set the blob's Description metaData property to the provided description.
        /// </summary>
        /// <param name="blob">The blob whose metaData will be altered.</param>
        /// <param name="description">The description to be recorded for the blob.</param>
        private void UpdateDescriptionMetaData(CloudBlockBlob blob, string description)
        {
            description = (string.IsNullOrEmpty(description) ? "No description" : description);

            if (!blob.Metadata.ContainsKey("Description"))
                blob.Metadata.Add(new KeyValuePair<string, string>("Description", description));
            else
                blob.Metadata["Description"] = description;

            blob.SetMetadata();
        }


        /// <summary>
        /// Perform the appropriate file conversion by calling to the FileConversion.exe
        /// Zip all contents to be efficiently stored in cloud storage.
        /// </summary>
        /// <param name="path">The base file path to the file that is to be converted.</param>
        /// <param name="subDir">The subdirectory that the file should be stored in.</param>
        /// <param name="requestExtension">The file type to convert to.</param>
        /// <param name="fileName">The name of the file to convert.</param>
        /// <param name="zipFolderName">The name of the .zip folder to produce.</param>
        /// <returns>
        ///     <ul>
        ///         <li>True: The file was successfully converted and is ready for the .zip to be uploaded</li>
        ///         <li>False: The file failed to convert and cannot be uploaded.</li>
        ///     </ul>
        /// </returns>
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

        #endregion
    }
}