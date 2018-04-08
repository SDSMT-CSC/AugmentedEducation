using System;

namespace Common
{
    public class FileUIInfo
    {
        #region Constructor

        public FileUIInfo(string fileName = "", string author = "", string description = "", DateTime uploaded = new DateTime())
        {
            Author = author;
            FileName = fileName;
            Description = description;
            UploadDate = uploaded;
        }

        #endregion


        #region Properties

        public string FileName { get; set; }
        public string Author { get; set; }
        public string Description { get; set; }
        public DateTime UploadDate { get; set; }

        #endregion
    }
}
