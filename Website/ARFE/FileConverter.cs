using System.Web;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;

/// <summary>
/// This is the over-arching namespace for all website related code.
/// </summary>
namespace ARFE
{
    /// <summary>
    /// The FileConverter class serves as an interface between the ASP.NET controllers and other 
    /// C# classes to FileConversion.exe.  This is used throughout the application for managing
    /// file compatability and efficiency in both file storage and transfer.
    /// </summary>
    public class FileConverter
    {
        #region Members

        /// <summary> Backing variable for the location of the file to convert. </summary>
        private string _InputFolder;
        /// <summary> Backing variable for the destination of the converted file. </summary>
        private string _OutputFolder;
        /// <summary> The AppDomainPath in a single variable since it is used frequently. </summary>
        private string _AppDomainPath = HttpRuntime.AppDomainAppPath;

        #endregion


        #region Constructor

        /// <summary>
        /// Public constructor that requires parameters for both the file path location
        /// of the file to convert and the file path location for the files produced
        /// by the conversion process.
        /// </summary>
        /// <param name="inputFolder">The file path location of the file to convert.</param>
        /// <param name="outputFolder">The file path location for the files produced by the conversion process.</param>
        public FileConverter(string inputFolder, string outputFolder)
        {
            _InputFolder = inputFolder;
            _OutputFolder = outputFolder;
        }

        #endregion


        #region Properties

        /// <summary> 
        /// Public read-only property defined with an expression-bodied member. 
        /// The property is the file path location of the file to convert.
        /// </summary>
        public string InputFolder => _InputFolder;

        /// <summary> 
        /// Public read-only property defined with an expression-bodied member. 
        /// The property is the file path location of the files produced by the conversion process.
        /// </summary>
        public string OutputFolder => _OutputFolder;
        /// <summary> 
        /// Public read-only property defined with an expression-bodied member. 
        /// The property is the base directory of the AppDomainPath.
        /// </summary>
        public string BasePath => _AppDomainPath;

        #endregion


        #region Public Methods

        /// <summary>
        /// First checks if the file is of a supported type, by calling <see cref="ValidateExtension(string)"/>.
        /// If the file type is supported, a call to FileConversion.exe is made to convert the file to 
        /// a .fbx file.
        /// </summary>
        /// <param name="fileName">The name of the file to convert in the <see cref="InputFolder"/> directory.</param>
        /// <returns>
        ///     <ul>
        ///         <li>True: The file was converted successfully.</li>
        ///         <li>False: The file was not converted successfully.</li>
        ///     </ul>
        /// </returns>
        public bool ConvertToFBX(string fileName)
        {
            if (ValidateExtension(fileName))
            {
                int index = fileName.LastIndexOf(".");
                string baseFile = fileName.Substring(0, index);

                return Convert(fileName, baseFile, ".fbx");
            }

            return false;
        }


        /// <summary>
        /// First checks if the file is of a supported type, by calling <see cref="ValidateExtension(string)"/>.
        /// If the file type is supported, a call to FileConversion.exe is made to convert the file to 
        /// a .dae file.
        /// </summary>
        /// <param name="fileName">The name of the file to convert in the <see cref="InputFolder"/> directory.</param>
        /// <returns>
        ///     <ul>
        ///         <li>True: The file was converted successfully.</li>
        ///         <li>False: The file was not converted successfully.</li>
        ///     </ul>
        /// </returns>
        public bool ConvertToDAE(string fileName)
        {
            if (ValidateExtension(fileName))
            {
                int index = fileName.LastIndexOf(".");
                string baseFile = fileName.Substring(0, index);

                return Convert(fileName, baseFile, ".dae");
            }

            return false;
        }


        /// <summary>
        /// First checks if the file is of a supported type, by calling <see cref="ValidateExtension(string)"/>.
        /// If the file type is supported, a call to FileConversion.exe is made to convert the file to 
        /// a .obj file.
        /// </summary>
        /// <param name="fileName">The name of the file to convert in the <see cref="InputFolder"/> directory.</param>
        /// <returns>
        ///     <ul>
        ///         <li>True: The file was converted successfully.</li>
        ///         <li>False: The file was not converted successfully.</li>
        ///     </ul>
        /// </returns>
        public bool ConvertToOBJ(string fileName)
        {
            if (ValidateExtension(fileName))
            {
                int index = fileName.LastIndexOf(".");
                string baseFile = fileName.Substring(0, index);

                Convert(fileName, baseFile, ".obj");
            }

            return false;
        }


        /// <summary>
        /// First checks if the file is of a supported type, by calling <see cref="ValidateExtension(string)"/>.
        /// If the file type is supported, a call to FileConversion.exe is made to convert the file to 
        /// a .stl file.
        /// </summary>
        /// <param name="fileName">The name of the file to convert in the <see cref="InputFolder"/> directory.</param>
        /// <returns>
        ///     <ul>
        ///         <li>True: The file was converted successfully.</li>
        ///         <li>False: The file was not converted successfully.</li>
        ///     </ul>
        /// </returns>
        public bool ConvertToSTL(string fileName)
        {
            if (ValidateExtension(fileName))
            {
                int index = fileName.LastIndexOf(".");
                string baseFile = fileName.Substring(0, index);

                return Convert(fileName, baseFile, ".stl");
            }

            return false;
        }


        /// <summary>
        /// First checks if the file is of a supported type, by calling <see cref="ValidateExtension(string)"/>.
        /// If the file type is supported, a call to FileConversion.exe is made to convert the file to 
        /// a .ply file.
        /// </summary>
        /// <param name="fileName">The name of the file to convert in the <see cref="InputFolder"/> directory.</param>
        /// <returns>
        ///     <ul>
        ///         <li>True: The file was converted successfully.</li>
        ///         <li>False: The file was not converted successfully.</li>
        ///     </ul>
        /// </returns>
        public bool ConvertToPLY(string fileName)
        {
            if (ValidateExtension(fileName))
            {
                int index = fileName.LastIndexOf(".");
                string baseFile = fileName.Substring(0, index);

                return Convert(fileName, baseFile, ".ply");
            }

            return false;
        }

        #endregion


        #region Private Methods

        /// <summary>
        /// Verify that the type of file that has been requested to be converted is 
        /// supported by FileConversion.exe.  The file extesion must match one of the 
        /// following:
        /// <ul>
        ///     <li>.dae</li>
        ///     <li>.fbx</li>
        ///     <li>.obj</li>
        ///     <li>.ply</li>
        ///     <li>.stl</li>
        /// </ul>
        /// </summary>
        /// <param name="filename">
        /// The name of the file that has been requested to be converted to an alternate format.
        /// </param>
        /// <returns>
        /// <ul>
        ///     <li>True: The file format of <paramref name="filename"/> is supported.</li>
        ///     <li>False: The file format of <paramref name="filename"/> is not supported.</li>
        /// </ul>
        /// </returns>
        private bool ValidateExtension(string filename)
        {
            //get just the ".ext" extension
            int index = filename.LastIndexOf(".");
            string extension = filename.Substring(index);
            //list is easily extensible
            List<string> supportedFormats = new List<string>() { ".dae", ".fbx", ".obj", ".ply", ".stl" };

            //for more formats, either add to the declaration or: 
            //supportedForamts.Add(".ext");

            return supportedFormats.Contains(extension);
        }


        /// <summary>
        /// The general conversion method that each of <see cref="ConvertToDAE(string)"/>,
        /// <see cref="ConvertToFBX(string)"/>, <see cref="ConvertToOBJ(string)"/>, 
        /// <see cref="ConvertToPLY(string)"/>, and <see cref="ConvertToSTL(string)"/> call to
        /// in order to perform the appropriate file conversion.
        /// </summary>
        /// <param name="fileName"> The name of the file in <see cref="InputFolder"/> to convert. </param>
        /// <param name="baseFile"> The base name of <paramref name="fileName"/> without the file extension. </param>
        /// <param name="extension"> The extension of the file type that is expected from the conversion. </param>
        /// <returns>
        ///     <ul>
        ///         <li>True: The file was converted successfully.</li>
        ///         <li>False: The file was not converted successfully.</li>
        ///     </ul>
        /// </returns>
        private bool Convert(string fileName, string baseFile, string extension)
        {
            Process process = new Process();
            string typeArguments = $"-t {extension}";
            string inputArguments = $"-i {BasePath}{InputFolder}\\{fileName}";
            string outputArguments = $"-o {BasePath}{OutputFolder}\\{baseFile}{extension}";

            try
            {
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.StartInfo.FileName = $"{BasePath}UploadedFiles\\FileConversion.exe";
                process.StartInfo.Arguments = $"{inputArguments} {typeArguments} {outputArguments}";
                process.Start();

                while (!process.HasExited)
                    Thread.Sleep(1000);
                process.Close();

                return true;
            }
            catch { }

            return false;
        }

        #endregion
    }
}