using System;
using System.Collections.Generic;

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

    public class SupportedFileTypes
    {
        public static List<string> FileList => new List<string> { ".dae", ".fbx", ".obj", ".ply", ".stl" };

    }
}
