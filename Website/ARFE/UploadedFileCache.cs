using System;
using System.IO;
using System.Web;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Collections.Concurrent;

namespace ARFE
{
    public class UploadedFileCache
    {
        #region Members

        private static ConcurrentBag<Guid> s_UsedGuids;
        private static int s_PreserveMinutes = 15;
        private static string s_BasePath = string.Empty;
        private static UploadedFileCache s_Instance = null;
        private static ConcurrentDictionary<Guid, FileData> s_FileDataByGuid;

        #endregion


        #region Constructor

        //singleton
        public static UploadedFileCache GetInstance()
        {
            if (s_Instance == null)
            {
                s_Instance = new UploadedFileCache();
            }
            return s_Instance;
        }

        private UploadedFileCache()
        {
            s_FileDataByGuid = new ConcurrentDictionary<Guid, FileData>();
            s_UsedGuids = new ConcurrentBag<Guid>();
        }

        #endregion


        #region Properties

        public string BasePath
        {
            get { return s_BasePath; }
            set
            {
                if (string.IsNullOrEmpty(s_BasePath))
                    s_BasePath = value;
                CleanupUploadDirectory();
            }
        }

        #endregion


        #region Public Methods


        public static void DeleteOldFiles()
        {
            DateTime now = DateTime.Now;
            List<Guid> removed = new List<Guid>();

            if (s_FileDataByGuid != null)
            {
                foreach (Guid g in s_FileDataByGuid.Keys)
                {
                    FileData data = s_FileDataByGuid[g];
                    if (now > data.FileExpirationTime)
                    {
                        if (s_Instance.DeleteFile(data.OwnerName, data.FileName))
                            removed.Add(g);
                        else
                            s_Instance.MarkForDelete(data.OwnerName, data.FileName);
                    }
                }
            }

            s_Instance.CleanExtraFilesFromUploadDirectory();

            //get all UploadedFiles/<GUID> directory names
            List<string> subDirs = Directory.GetDirectories(s_BasePath).ToList();
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
                s_Instance.RemoveFile(g);
            }
        }


        public bool SaveFile(HttpPostedFileBase file, string userName, string fileName, string altFileName = "", string description = "", bool isPublic = false)
        {
            Guid fileGuid = GetGuidForSavingFile();
            string path = Path.Combine(s_BasePath, fileGuid.ToString());
            FileData fileData = new FileData(fileGuid, userName, fileName, altFileName, description, isPublic);

            Directory.CreateDirectory(path);
            file.SaveAs(Path.Combine(path, fileData.FileName));

            if (File.Exists(Path.Combine(path, fileData.FileName)))
            {
                s_FileDataByGuid.TryAdd(fileGuid, fileData);
                return true;
            }
            else
            {
                //file didn't save, don't keep subDir
                DeleteDirectory(path);
            }

            return false;
        }

        public Guid SaveFile(CloudBlockBlob file, string userName, string fileName)
        {
            Guid fileGuid = GetGuidForSavingFile();
            string path = Path.Combine(s_BasePath, fileGuid.ToString());

            Directory.CreateDirectory(path);

            //path parameter is absolute
            using (Stream fileStream = File.OpenWrite(Path.Combine(path, fileName)))
            {   //Have to use DownloadToStream - DownloadToFile results in access denied error
                file.DownloadToStream(fileStream);
            }

            if (!File.Exists(Path.Combine(path, fileName)))
            {   //file didn't save, don't keep subDir
                DeleteDirectory(path);
                return Guid.Empty;
            }

            return fileGuid;
        }


        public void RemoveFile(Guid fileGuid)
        {
                while(s_UsedGuids.Contains(fileGuid)
                      && !s_UsedGuids.TryTake(out Guid guid));

                while(s_FileDataByGuid.ContainsKey(fileGuid)
                      && s_FileDataByGuid.TryRemove(fileGuid, out FileData fd));
        }


        public Guid FindContainingFolderGUIDForFile(string userName, string uploadFileName)
        {
            List<FileData> fileDataGuids = new List<FileData>();
            DateTime mostRecent = DateTime.MinValue;

            //default to unusable guid
            Guid fileGuid = Guid.Empty;

            if (s_FileDataByGuid != null)
            {
                foreach (FileData data in s_FileDataByGuid.Values)
                {
                    //belongs to user, has matching name, is most recent
                    if (data.OwnerName == userName
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


        public string GetFileDescription(string userName, string uploadFileName)
        {
            Guid fileGuid = FindContainingFolderGUIDForFile(userName, uploadFileName);

            if (s_FileDataByGuid.ContainsKey(fileGuid))
            {
                return s_FileDataByGuid[fileGuid].Description;
            }

            return string.Empty;
        }


        public bool IsSavedFilePublic(string userName, string uploadFileName)
        {
            Guid fileGuid = FindContainingFolderGUIDForFile(userName, uploadFileName);

            if (s_FileDataByGuid.ContainsKey(fileGuid))
            {
                return s_FileDataByGuid[fileGuid].IsPublic;
            }

            return false;
        }


        public bool DeleteFile(string userName, string uploadFileName)
        {
            string savedFileName = string.Empty;
            Guid fileGuid = FindContainingFolderGUIDForFile(userName, uploadFileName);

            if (fileGuid != Guid.Empty)
            {
                if (DeleteDirectory(Path.Combine(s_BasePath, fileGuid.ToString())))
                {
                    return true;
                }

                MarkForDelete(userName, uploadFileName);
            }
            //doesn't exist - like deleting
            else return true;

            return false;
        }


        public bool DeleteAndRemoveFile(string userName, string uploadFileName)
        {
            string savedFileName = string.Empty;
            Guid fileGuid = FindContainingFolderGUIDForFile(userName, uploadFileName);

            if (DeleteDirectory(Path.Combine(s_BasePath, fileGuid.ToString())))
            {
                RemoveFile(fileGuid);
                return true;
            }

            MarkForDelete(userName, uploadFileName);
            return false;
        }


        public void MarkForDelete(string userName, string uploadFileName)
        {
            Guid fileGuid = FindContainingFolderGUIDForFile(userName, uploadFileName);

            if (s_FileDataByGuid.ContainsKey(fileGuid))
            {
                s_FileDataByGuid[fileGuid].MarkForDelete();
            }
        }

        #endregion


        #region Private Methods

        private Guid GetGuidForSavingFile()
        {
            Guid g;

            do
            {
                g = Guid.NewGuid();
            } while (s_UsedGuids.Contains(g));

            s_UsedGuids.Add(g);

            return g;
        }


        private bool DeleteDirectory(string path)
        {
            int retryCount = 15;

            while (retryCount-- > 0 && Directory.Exists(path))
            {
                try { Directory.Delete(path, true); }
                catch { Thread.Sleep(500); }
            }

            return !(Directory.Exists(path));
        }


        private void CleanupUploadDirectory()
        {
            List<string> subDirs = new List<string>();

            CleanExtraFilesFromUploadDirectory();                
            subDirs = Directory.GetDirectories(s_BasePath).ToList();

            foreach (string dir in subDirs)
            {
                DeleteDirectory(Path.Combine(s_BasePath, dir));
            }

        }


        private void CleanExtraFilesFromUploadDirectory()
        {
            string extrasPath = Path.Combine(s_BasePath, "Extras");
            List<string> extraFiles = Directory.GetFiles(s_BasePath).ToList();

            if(!Directory.Exists(extrasPath))
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

            DeleteDirectory(extrasPath);
        }

        #endregion


        #region Internal Classes

        private class FileData
        {
            #region Members

            private bool _IsPublic;
            private Guid _FileGuid;
            private string _FileName;
            private string _OwnerName;
            private string _Description;
            private string _AltFileName;
            private DateTime _FileExpirationTime;

            #endregion


            #region Constructors

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

            public bool IsPublic => _IsPublic;
            public Guid FileGuid => _FileGuid;
            public string FileName => _FileName;
            public string OwnerName => _OwnerName;
            public string Description => _Description;
            public string AltFileName
            {   //no altName -> return fileName
                get { return string.IsNullOrEmpty(_AltFileName) ? _FileName : _AltFileName; }
            }

            public DateTime FileExpirationTime => _FileExpirationTime;

            #endregion


            #region Public Methods

            public void MarkForDelete()
            {
                _FileExpirationTime = DateTime.Now.AddHours(-1);
            }

            #endregion
        }
        #endregion
    }
}