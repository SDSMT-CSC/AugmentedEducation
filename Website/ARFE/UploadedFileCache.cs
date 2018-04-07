using System;
using System.IO;
using System.Web;
using System.Collections.Generic;

namespace ARFE
{
    public class UploadedFileCache
    {
        #region Members

        private static List<Guid> s_UsedGuids;
        private static int s_PreserveMinutes = 15;
        private static string s_BasePath = string.Empty;
        private static UploadedFileCache s_Instance = null;
        private static Dictionary<Guid, FileData> s_FileDataByGuid;

        #endregion


        #region Constructor

        //singleton
        public UploadedFileCache Init(string basePath)
        {
            if (s_Instance == null)
            {
                s_Instance = new UploadedFileCache(basePath);
            }
            return s_Instance;
        }

        private UploadedFileCache(string basePath)
        {
            s_BasePath = basePath;
            s_UsedGuids = new List<Guid>();
            s_FileDataByGuid = new Dictionary<Guid, FileData>();
        }

        #endregion


        #region Public Methods

        public string SaveFile(HttpPostedFileBase file, string userName, string fileName, string description = "", bool isPublic = false)
        {
            string errorMsg = string.Empty;
            Guid fileGuid = GetGuidForSavingFile();
            FileData fileData = new FileData(fileGuid, userName, fileName, description, isPublic);

            s_FileDataByGuid.Add(fileGuid, fileData);
            try
            {
                file.SaveAs(Path.Combine(s_BasePath, fileData.SavedFileName));
            }
            catch(Exception ex) { errorMsg = $"Exception: \r\n{ex.ToString()}"; }

            return errorMsg;
        }


        public void RemoveFile(Guid fileGuid)
        {
            if (s_UsedGuids.Contains(fileGuid))
            {
                s_UsedGuids.Remove(fileGuid);
                s_FileDataByGuid.Remove(fileGuid);
            }
        }


        public Guid FindSavedFile(string userName, string origFileName)
        {
            Guid fileGuid = s_UsedGuids[0];
            DateTime mostRecent = DateTime.Now;
            List<FileData> fileDataGuids = new List<FileData>();

            foreach (FileData data in s_FileDataByGuid.Values)
            {
                //belongs to user, has matching name, is most recent
                if (data.OwnerName == userName
                && data.OrigFileName == origFileName
                && data.FileExpirationTime < mostRecent)
                {
                    fileGuid = data.FileGuid;
                    mostRecent = data.FileExpirationTime;
                }
            }

            return fileGuid;
        }


        public string DeleteFile(string userName, string origFileName)
        {
            string errorMsg = string.Empty;
            string savedFileName = string.Empty;
            Guid fileGuid = FindSavedFile(userName, origFileName);

            savedFileName = s_FileDataByGuid[fileGuid].SavedFileName;

            try
            {
                File.Delete(Path.Combine(s_BasePath, savedFileName));
                RemoveFile(fileGuid);
            }
            catch (Exception ex) { errorMsg = $"Exception: \r\n{ex.ToString()}"; }

            return errorMsg;
        }


        public static string DeleteOldFiles()
        {
            DateTime now = DateTime.Now;
            string errorMsg = string.Empty;
            List<Guid> removed = new List<Guid>();

            foreach (Guid g in s_FileDataByGuid.Keys)
            {
                FileData data = s_FileDataByGuid[g];
                if (now > data.FileExpirationTime)
                {
                    removed.Add(g);

                    try
                    {
                        File.Delete(Path.Combine(s_BasePath, data.SavedFileName));
                    }
                    catch (Exception ex) { errorMsg = $"Exception: \r\n{ex.ToString()}"; }

                }
            }

            foreach(Guid g in removed)
            {
                s_Instance.RemoveFile(g);
            }

            return errorMsg;
        }

        #endregion


        #region Private Methods

        private Guid GetGuidForSavingFile()
        {
            Guid g;

            do
            {
                g = new Guid();
            } while (s_UsedGuids.Contains(g));

            s_UsedGuids.Add(g);

            return g;
        }

        #endregion


        #region Internal Classes

        private class FileData
        {
            #region Members

            private bool _IsPublic;
            private Guid _FileGuid;
            private string _OwnerName;
            private string _Description;
            private string _OrigFileName;
            private DateTime _FileExpirationTime;

            #endregion


            #region Constructors 

            public FileData(Guid fileGuid, string ownerName, string fileName, string description = "", bool isPublic = false)
            {
                _IsPublic = isPublic;
                _FileGuid = fileGuid;
                _OwnerName = ownerName;
                _OrigFileName = fileName;
                _Description = description;
                _FileExpirationTime = DateTime.Now.AddMinutes(s_PreserveMinutes);
            }

            #endregion


            #region Properties

            public bool IsPublic => _IsPublic;
            public Guid FileGuid => _FileGuid;
            public string OwnerName => _OwnerName;
            public string Description => _Description;
            public string OrigFileName => _OrigFileName;
            public DateTime FileExpirationTime => _FileExpirationTime;
            public string SavedFileName => $"{FileGuid}__{OrigFileName}";

            #endregion
        }


        #endregion
    }
}