//System .dll's
using System;
using System.IO;
using System.Web;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using System.Collections.Concurrent;

//NuGet packages
using Microsoft.WindowsAzure.Storage.Blob;

namespace ARFE
{
    /// <summary>
    /// A singleton class, constructed via the <see cref="GetInstance"/> method call.
    /// This class serves as a cache for tracking the location, details about, and 
    /// automatic removal of files that are saved for intermediate storage between 
    /// being uploaded to Azure blob storage, or converted to some other file type 
    /// pending a download.
    /// </summary>
    public class UploadedFileCache
    {
        #region Members

        /// <summary> 
        /// ConcurrentBag similar to a thread-safe (unordered) List. Used for tracking
        /// unique sub-directories that files will be written-to and read-from during the
        /// upload/download/conversion processes.
        /// </summary>
        private static ConcurrentBag<Guid> s_UsedGuids;
        /// <summary> Minimum number of minutes to leave a file before purging. </summary>
        private static int s_PreserveMinutes = 15;
        /// <summary> The root directory for file upload/download intermediate storage. </summary>
        private static string s_BasePath = string.Empty;
        /// <summary> A singleton instance. This class should only exist once, since it's a cache. </summary>
        private static UploadedFileCache s_Instance = null;
        /// <summary>
        /// Thread-safe dictionary, used to associate Guids (as subdirectory names)
        /// to the file that is temporarily being stored in it.
        /// </summary>
        private static ConcurrentDictionary<Guid, FileData> s_FileDataByGuid;

        #endregion


        #region Constructor

        /// <summary>
        /// A publicly available method for creating a single instance of this cache
        /// if one doesn't already exist, or getting a reference to the single instance.
        /// </summary>
        /// <returns>
        /// A reference to the single instance of this cache class.
        /// </returns>
        public static UploadedFileCache GetInstance()
        {
            //create if not exists
            if (s_Instance == null)
                s_Instance = new UploadedFileCache();

            return s_Instance;
        }


        /// <summary>
        /// A private constructor to ensure that the only way to get an object reference
        /// to this cache class is by calling <see cref="GetInstance"/>.
        /// </summary>
        private UploadedFileCache()
        {
            s_UsedGuids = new ConcurrentBag<Guid>();
            s_FileDataByGuid = new ConcurrentDictionary<Guid, FileData>();
        }

        #endregion


        #region Properties

        /// <summary>
        /// The publicly accessible property for the root directory for file upload/download intermediate storage.
        /// This property can only be set once and upon being set, purges the supplied directory path of all 
        /// unneccessary files and folders.
        /// </summary>
        public string BasePath
        {
            get { return s_BasePath; }
            set
            {
                if (string.IsNullOrEmpty(s_BasePath))
                {
                    s_BasePath = value;
                    CleanupUploadDirectory();
                }
            }
        }

        #endregion


        #region Public Methods

        /// <summary>
        /// A publicly available static method for cleaning the root upload directory.
        /// This method is called at timed intervals from <see cref="Startup.Timer_Elapsed(object, System.Timers.ElapsedEventArgs)"/>.
        /// </summary>
        public static void DeleteOldFiles()
        {
            DateTime now = DateTime.Now;
            List<Guid> removed = new List<Guid>();

            //Make sure instances exist - else null referrence exceptions
            if (s_FileDataByGuid != null)
            {   //iterate Guid keys as subdirectory names
                foreach (Guid g in s_FileDataByGuid.Keys)
                {   //get file data for subdirectory
                    FileData data = s_FileDataByGuid[g];
                    if (now > data.FileExpirationTime)
                    {   //attempt to delete if temporary file time is up.
                        //if doesn't delete, mark for deletion by guaranteeing expired
                        //if (s_Instance.DeleteFile(data.OwnerName, data.FileName))
                        if (s_Instance.DeleteAndRemoveFile(data.OwnerName, data.FileName))
                            removed.Add(g);
                        else
                            s_Instance.MarkForDelete(data.OwnerName, data.FileName);
                    }
                }
            }

            //delte any loose file that aren't converter or .dll's
            s_Instance.CleanExtraFilesFromUploadDirectory();

            //get all UploadedFiles/<GUID> directory names
            List<string> subDirPaths = Directory.GetDirectories(s_BasePath).ToList();
            List<string> subDirs = new List<string>();
            foreach(string path in subDirPaths)
            {
                subDirs.Add(path.Substring(path.LastIndexOf(Path.DirectorySeparatorChar) + 1));
            }

            foreach (Guid g in s_UsedGuids)
            {
                //not a tracked subdir - delete it
                if (!s_FileDataByGuid.ContainsKey(g))
                {
                    s_Instance.DeleteDirectory(Path.Combine(s_BasePath, g.ToString()));
                    //add guid to list of tracked items to be removed
                    if (!removed.Contains(g))
                        removed.Add(g);
                }
                //guid exists as UsedGuid but not as subdir name
                else if (!subDirs.Contains(g.ToString()))
                {
                    //add guid to list of tracked items to be removed
                    if (!removed.Contains(g))
                        removed.Add(g);
                }
            }

            //remove old tracked items
            foreach (Guid g in removed)
            {
                s_Instance.RemoveFileTracking(g);
            }
        }


        /// <summary>
        /// Create a unique sub-directory for the file to save the file in.  Information about
        /// the file associated with the unique sub-directory will be added to the 
        /// thread-safe <see cref="s_UsedGuids"/> and <see cref="s_FileDataByGuid"/> members
        /// for tracking lifespan and information regarding the temporary file(s) stored.
        /// </summary>
        /// <param name="file">
        /// The file uploaded to the website via HTTP POST in the 
        /// <see cref="Controllers.UploadController.UploadFile(HttpPostedFileBase, HttpPostedFileBase, bool, string, string)"/>
        /// method or the <see cref="Controllers.UploadController.UploadFile"/> method.
        /// </param>
        /// <param name="userName">The name of the user uploading the file.</param>
        /// <param name="fileName">The name of the file being uploaded.</param>
        /// <param name="altFileName">An alternative file name that the file should be saved as instead, provided by the user.</param>
        /// <param name="description">A description of the file provided by user.</param>
        /// <param name="isPublic">
        /// A flag indicating if the file should be uploaded to the public blob container of the user's private blob container.
        /// </param>
        /// <returns>
        ///     <ul>
        ///         <li>True: The file was uploaded successfully.</li>
        ///         <li>False: The file failed to upload.</li>
        ///     </ul>
        /// </returns>
        public bool SaveFile(HttpPostedFileBase file, string userName, string fileName, string altFileName = "", string description = "", bool isPublic = false)
        {
            int retry = 50;
            Guid fileGuid = GetGuidForSavingFile();
            string path = Path.Combine(s_BasePath, fileGuid.ToString());
            FileData fileData = new FileData(fileGuid, userName, fileName, altFileName, description, isPublic);

            DirectoryInfo dirInfo = Directory.CreateDirectory(path);
            if (dirInfo.Exists)
            {
                file.SaveAs(Path.Combine(path, fileData.FileName));

                if (File.Exists(Path.Combine(path, fileData.FileName)))
                {
                    //loop until the GUID:FileData is added to tracking.
                    //Don't check contains - guaranteed uniqe from GetGuidForSavingFile()
                    while (retry > 0 && !s_FileDataByGuid.TryAdd(fileGuid, fileData)) { retry--; }
                    return true;
                }
                else
                {
                    //file didn't save, don't keep subDir
                    DeleteDirectory(path);
                    //loop until removed from Guid tracking.
                    //check .Contains() each time in case removed externally
                    while (retry > 0
                            && s_UsedGuids.Contains(fileGuid)
                            && !s_UsedGuids.TryTake(out fileGuid)) { retry--; }
                }
            }

            return false;
        }


        /// <summary>
        /// Create a unique sub-directory for the file to save the file in.  Information about
        /// the file associated with the unique sub-directory will be added to the 
        /// thread-safe <see cref="s_UsedGuids"/> and <see cref="s_FileDataByGuid"/> members
        /// for tracking lifespan and information regarding the temporary file(s) stored.        
        /// </summary>
        /// <param name="file">
        /// The file being downloaded from the 
        /// <see cref="BlobManager.ConvertBlobToBlob(CloudBlockBlob, string, string, string, string)"/> method.
        /// </param>
        /// <param name="userName">The name of the user uploading the file.</param>
        /// <param name="fileName">The name of the file being uploaded.</param>
        /// <returns>
        ///     <ul>
        ///         <li>Guid : The Guid that is the name of the sub-directory that the file was saved to.</li>
        ///         <li>Guid.Empty : The file could not be saved.</li>
        ///     </ul>
        /// </returns>
        public Guid SaveFile(CloudBlockBlob file, string userName, string fileName)
        {
            int retry = 50;
            Guid fileGuid = GetGuidForSavingFile();
            string path = Path.Combine(s_BasePath, fileGuid.ToString());

            DirectoryInfo dirInfo = Directory.CreateDirectory(path);
            if (dirInfo.Exists)
            {
                //path parameter is absolute
                using (Stream fileStream = File.OpenWrite(Path.Combine(path, fileName)))
                {   //Have to use DownloadToStream - DownloadToFile results in access denied error
                    file.DownloadToStream(fileStream);
                }

                if (!File.Exists(Path.Combine(path, fileName)))
                {   //file didn't save, don't keep subDir
                    while (!s_UsedGuids.TryTake(out fileGuid) && retry > 0) { retry--; }
                    DeleteDirectory(path);
                    return Guid.Empty;
                }

                //file saved - track Guid:FileData
                FileData data = new FileData(fileGuid, userName, fileName);
                while (!s_FileDataByGuid.TryAdd(fileGuid, data) && retry > 0) { retry--; }
            }
            else
                fileGuid = Guid.Empty;

            return fileGuid;
        }


        /// <summary>
        /// For a provided Guid representing a sub-directory, remove the Guid from <see cref="s_UsedGuids"/> 
        /// and remove the Guid:FileData record from <see cref="s_FileDataByGuid"/>. An immediate removal 
        /// is not guaranteed from either members due to their thread-safe design.  Removal is retried at 
        /// most 50 times each.
        /// </summary>
        /// <param name="fileGuid">The Guid to stop tracking.</param>
        public void RemoveFileTracking(Guid fileGuid)
        {
            int retry = 50;
            //retry at most 50 times
            //check .Contains() each time, may have been removed elsewhere
            while (retry > 0
                    && s_UsedGuids.Contains(fileGuid)
                    && !s_UsedGuids.TryTake(out Guid guid)) { retry--; }

            retry = 50;
            while (retry > 0
                    && s_FileDataByGuid.ContainsKey(fileGuid)
                    && s_FileDataByGuid.TryRemove(fileGuid, out FileData fd)) { retry--; }
        }


        /// <summary>
        /// Check each <see cref="FileData(Guid, string, string, string, string, bool)"/> 
        /// in <see cref="s_FileDataByGuid"/> values.  Attempt to match user name and file name.
        /// If multiple matches are found, which should almost never be the case, the most recent is assumed.
        /// </summary>
        /// <param name="userName">The name of the user who owns the file.</param>
        /// <param name="uploadFileName">The file name (or alternate file name) that the file was saved as.</param>
        /// <returns>
        ///     <ul>
        ///         <li>Guid : The Guid that is the name of the sub-directory that the file is in.</li>
        ///         <li>Guid.Empty : File not found.</li>
        ///     </ul>
        /// </returns>
        public Guid FindContainingFolderGUIDForFile(string userName, string uploadFileName)
        {
            //default to unusable guid
            Guid fileGuid = Guid.Empty;
            DateTime mostRecent = DateTime.MinValue;
            List<FileData> fileDataGuids = new List<FileData>();

            if (s_FileDataByGuid != null)
            {
                foreach (FileData data in s_FileDataByGuid.Values)
                {
                    //belongs to user, has matching name, is most recent
                    if (data.OwnerName == userName
                    //See FileData definition
                    //if AltFileName == "" : AltFileName == FileName
                    && data.AltFileName == uploadFileName
                    && data.FileExpirationTime > mostRecent)
                    {
                        fileGuid = data.FileGuid;
                        mostRecent = data.FileExpirationTime;
                    }
                }
            }

            return fileGuid;
        }


        /// <summary>
        /// Get the Guid that is the sub-directory name of the file from the 
        /// <see cref="FindContainingFolderGUIDForFile(string, string)"/> method.
        /// From the Guid, get the FileData from <see cref="s_FileDataByGuid"/>.
        /// Use the FileData to access the user provided file description.
        /// </summary>
        /// <param name="userName">The name of the user who saved the file.</param>
        /// <param name="uploadFileName">The name of the file that was saved.</param>
        /// <returns>
        ///     <ul>
        ///         <li>Description : The user-provided description of the file.</li>
        ///         <li>String.Empty : File Not Found</li>
        ///     </ul>
        /// </returns>
        public string GetFileDescription(string userName, string uploadFileName)
        {
            Guid fileGuid = FindContainingFolderGUIDForFile(userName, uploadFileName);

            if (s_FileDataByGuid.ContainsKey(fileGuid))
            {
                return s_FileDataByGuid[fileGuid].Description;
            }

            return string.Empty;
        }


        /// <summary>
        /// Get the Guid that is the sub-directory name of the file from the 
        /// <see cref="FindContainingFolderGUIDForFile(string, string)"/> method.
        /// From the Guid, get the FileData from <see cref="s_FileDataByGuid"/>.
        /// Use the FileData to access the user provided public/private flag.
        /// </summary>
        /// <param name="userName">The name of the user who saved the file.</param>
        /// <param name="uploadFileName">The name of the file that was saved.</param>
        /// <returns>
        ///     <ul>
        ///         <li>IsPublic : The user-provided public/private flag value.</li>
        ///         <li>Default : False, meaning private.</li>
        ///     </ul>
        /// </returns>
        public bool IsSavedFilePublic(string userName, string uploadFileName)
        {
            Guid fileGuid = FindContainingFolderGUIDForFile(userName, uploadFileName);

            if (s_FileDataByGuid.ContainsKey(fileGuid))
            {
                return s_FileDataByGuid[fileGuid].IsPublic;
            }

            return false;
        }


        /// <summary>
        /// Get the Guid that is the sub-directory name of the file from the 
        /// <see cref="FindContainingFolderGUIDForFile(string, string)"/> method.
        /// Use the Guid to delete the sub-directory and all of its contents.
        /// Call <see cref="RemoveFileTracking(Guid)"/> to free the Guid from tracking.
        /// </summary>
        /// <param name="userName">The name of the user who saved the file.</param>
        /// <param name="uploadFileName">The name of the file that was saved.</param>
        /// <returns>
        ///     <ul>
        ///         <li>True : The sub-directory that the file is in was deleted.</li>
        ///         <li>False : The sub-directory was not deleted, but was marked for deletion.</li>
        ///     </ul>
        /// </returns>
        public bool DeleteAndRemoveFile(string userName, string uploadFileName)
        {
            string savedFileName = string.Empty;
            Guid fileGuid = FindContainingFolderGUIDForFile(userName, uploadFileName);

            if (DeleteDirectory(Path.Combine(s_BasePath, fileGuid.ToString())))
            {
                RemoveFileTracking(fileGuid);
                return true;
            }

            MarkForDelete(userName, uploadFileName);
            return false;
        }


        /// <summary>
        /// Mark a <see cref="FileData(Guid, string, string, string, string, bool)"/> 
        /// record for deletion by calling <see cref="FileData.MarkForDelete"/> to set 
        /// its expiration time to some time in the past.  
        /// The next time that entries are examined for expiration, it should be deleted.
        /// </summary>
        /// <param name="userName">The name of the user who saved the file.</param>
        /// <param name="uploadFileName">The name of the file that was saved.</param>
        public void MarkForDelete(string userName, string uploadFileName)
        {
            Guid fileGuid = FindContainingFolderGUIDForFile(userName, uploadFileName);

            if (s_FileDataByGuid.ContainsKey(fileGuid))
                s_FileDataByGuid[fileGuid].MarkForDelete();
        }

        #endregion


        #region Private Methods

        /// <summary>
        /// Generate a Guid that doesn't currently exist in <see cref="s_UsedGuids"/>
        /// or <see cref="s_FileDataByGuid"/>.
        /// </summary>
        /// <returns>
        /// A newly registered Guid.
        /// </returns>
        private Guid GetGuidForSavingFile()
        {
            Guid g;

            do
            {
                g = Guid.NewGuid();
            } while (s_UsedGuids.Contains(g) || s_FileDataByGuid.ContainsKey(g));

            s_UsedGuids.Add(g);

            return g;
        }


        /// <summary>
        /// Given the file path to a directory, attempt to recursively remove it.
        /// </summary>
        /// <param name="path">The file path to the directory.</param>
        /// <returns>
        ///     <ul>
        ///         <li>True : The directory and all of its contents were deleted.</li>
        ///         <li>False : Nothing was deleted.</li>
        ///     </ul>
        /// </returns>
        private bool DeleteDirectory(string path)
        {
            int retryCount = 15;

            while (retryCount-- > 0 && Directory.Exists(path))
            {
                //Try catch in case directory is open.
                //shouldn't be the case running in production mode.
                try { Directory.Delete(path, true); }
                catch { Thread.Sleep(500); }
            }

            //true : deleted
            return !(Directory.Exists(path));
        }


        /// <summary>
        /// Remove all unnecessarry files and folders from the intermediate 
        /// upload directory. Called on Startup.
        /// </summary>
        private void CleanupUploadDirectory()
        {
            CleanExtraFilesFromUploadDirectory();

            //shouldn't have any sub-directories.
            List<string> subDirs = Directory.GetDirectories(s_BasePath).ToList();

            foreach (string dir in subDirs)
                DeleteDirectory(Path.Combine(s_BasePath, dir));
        }


        /// <summary>
        /// Remove any files from the intermediate upload directory that are not
        /// required for file conversion.
        /// </summary>
        private void CleanExtraFilesFromUploadDirectory()
        {
            //Move all extra files into a new Directory called "Extras"
            string extrasPath = Path.Combine(s_BasePath, "Extras");
            List<string> extraFiles = Directory.GetFiles(s_BasePath).ToList();

            if (!Directory.Exists(extrasPath))
                Directory.CreateDirectory(extrasPath);

            for (int fileIdx = 0; fileIdx < extraFiles.Count; fileIdx++)
            {
                string file = extraFiles[fileIdx];
                string ext = file.Substring(file.LastIndexOf('.'));
                if (ext != ".exe" && ext != ".dll")
                {
                    File.Move(file, Path.Combine(extrasPath, $"{fileIdx}{ext}"));
                    //0.txt, 1.fbx, 2.dae - name doesn't matter, files are going to be deleted
                }
            }

            //recursively delete the "Extras" directory
            DeleteDirectory(extrasPath);
        }

        #endregion


        #region Internal Classes

        /// <summary>
        /// A private internal class used for keeping track of saved file information.
        /// This class serves as the value type for the <see cref="s_FileDataByGuid"/> member.
        /// </summary>
        private class FileData
        {
            #region Members

            /// <summary> A backing variable for the <see cref="IsPublic"/> property. </summary>
            private bool _IsPublic;
            /// <summary> A backing variable for the <see cref="FileGuid"/> property. </summary>
            private Guid _FileGuid;
            /// <summary> A backing variable for the <see cref="FileName"/> property. </summary>
            private string _FileName;
            /// <summary> A backing variable for the <see cref="OwnerName"/> property. </summary>
            private string _OwnerName;
            /// <summary> A backing variable for the <see cref="Description"/> property. </summary>
            private string _Description;
            /// <summary> A backing variable for the <see cref="AltFileName"/> property. </summary>
            private string _AltFileName;
            /// <summary> A backing variable for the <see cref="FileExpirationTime"/> property. </summary>
            private DateTime _FileExpirationTime;

            #endregion


            #region Constructors

            /// <summary>
            /// The constructor for the class. Requires that a <paramref name="fileGuid"/>, 
            /// <paramref name="ownerName"/>, and a <paramref name="fileName"/> be provided.
            /// </summary>
            /// <param name="fileGuid">The tracking Guid associated with this file.</param>
            /// <param name="ownerName">The name of the user who owns this file.</param>
            /// <param name="fileName">The original name of this file.</param>
            /// <param name="altFileName">
            /// OPTIONAL. The name to save this file as. If none is provided, the file is saved as <paramref name="fileName"/>.
            /// </param>
            /// <param name="description">OPTIONAL. The description of the file.</param>
            /// <param name="isPublic">OPTIONAL. The flag describing if the file should be public or private.</param>
            public FileData(Guid fileGuid, string ownerName, string fileName, string altFileName = "", string description = "", bool isPublic = false)
            {
                _IsPublic = isPublic;
                _FileGuid = fileGuid;
                _FileName = fileName;
                _OwnerName = ownerName;
                _AltFileName = altFileName;
                _Description = description;
                _FileExpirationTime = DateTime.Now.AddMinutes(s_PreserveMinutes);
            }

            #endregion


            #region Properties

            /// <summary> A read-only public property. The flag describing if the file should be public or private.</summary>
            public bool IsPublic => _IsPublic;

            /// <summary> A read-only public property. The tracking Guid associated with this file.</summary>
            public Guid FileGuid => _FileGuid;

            /// <summary> A read-only public property. The original name of this file.</summary>
            public string FileName => _FileName;

            /// <summary> A read-only public property. The name of the user who owns this file.</summary>
            public string OwnerName => _OwnerName;

            /// <summary> A read-only public property. The description of the file.</summary>
            public string Description => _Description;

            /// <summary> A read-only public property. The name to save this file as. If none is provided, the value is <see cref="FileName"/>.</summary>
            public string AltFileName
            {   //no altName -> return fileName
                get { return string.IsNullOrEmpty(_AltFileName) ? _FileName : _AltFileName; }
            }

            /// <summary> A read-only public property. The DateTime for when the file should be automatically removed from intermediate storage.</summary>
            public DateTime FileExpirationTime => _FileExpirationTime;

            #endregion


            #region Public Methods

            /// <summary>
            /// Set the FileData's expiration time to some time in the past, meaning that it should be removed.
            /// </summary>
            public void MarkForDelete()
            {
                _FileExpirationTime = DateTime.Now.AddHours(-1);
            }

            #endregion
        }
        #endregion
    }
}