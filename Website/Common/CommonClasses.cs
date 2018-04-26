using System;
using System.Collections.Generic;

/// <summary>
/// This namespace is for containing all custom objects and methods that should be made publicly available
/// and easily accessible for all other projects within the appliaction.
/// </summary>
namespace Common
{
    /// <summary>
    /// A class that contains file representation information
    /// for displaying information about a user's files to the 
    /// web interface.
    /// </summary>
    public class FileUIInfo
    {
        #region Constructor

        /// <summary>
        /// A constructor that takes any or all parameters to fill the defining object properties.
        /// </summary>
        /// <param name="fileName"> The name of the file. The default value is empty.</param>
        /// <param name="author"> The name of the owner of the file.  the default value is empty.</param>
        /// <param name="description"> The user-provided description of the file.  The default value is empty. </param>
        /// <param name="uploaded"> The UTC time when the file was uploaded to Azure Blob storage. </param>
        public FileUIInfo(string fileName = "", string author = "", string description = "", DateTime uploaded = new DateTime())
        {
            Author = author;
            FileName = fileName;
            Description = description;
            UploadDate = uploaded;
        }

        #endregion

        #region Properties

        /// <summary> The name of the file. </summary>
        public string FileName { get; set; }
        /// <summary> The name of the owner of the file. </summary>
        public string Author { get; set; }
        /// <summary> The user-provided description of the file. </summary>
        public string Description { get; set; }
        /// <summary> The UTC time when the file was uploaded to Azure Blob storage. </summary>
        public DateTime UploadDate { get; set; }

        #endregion
    }

    /// <summary>
    /// The list of file type extensions that are supported by the file conversion software 
    /// that this application uses.
    /// </summary>
    public class SupportedFileTypes
    {
        /// <summary> The list of supported file types. </summary>
        public static List<string> FileList => new List<string> { ".dae", ".fbx", ".obj", ".ply", ".stl" };

    }
}
