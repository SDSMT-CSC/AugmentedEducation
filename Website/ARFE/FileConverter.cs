using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace ARFE
{
    public class FileConverter 
    {
        private string InputFolder;
        private string OutputFolder;
        public FileConverter(string inputFolder, string outputFolder)
        {
            InputFolder = inputFolder;
            OutputFolder = outputFolder;
        }

        public bool ConvertToFBX(string filename)
        {
            if (ValidateExtension(filename))
            {
                try
                {
                    int index = filename.LastIndexOf(".");
                    string basefile = filename.Substring(0,index);

                    Process process = new Process();

                    process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    process.StartInfo.FileName = HttpRuntime.AppDomainAppPath + "UploadedFiles\\FileConversion.exe";

                    string arguments = "-i ";
                    arguments += HttpRuntime.AppDomainAppPath + InputFolder + "\\" + filename;
                    arguments += " -t .fbx -o " + HttpRuntime.AppDomainAppPath + OutputFolder + "\\"+ basefile+".fbx";


                    process.StartInfo.Arguments = arguments;
                    
                    process.Start();

                    while(process.HasExited == false)
                    {
                        Thread.Sleep(1000);
                    }

                    process.Close();
                    return true;
                }
                catch 
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public bool ConvertToDAE(string filename)
        {
            if (ValidateExtension(filename))
            {
                try
                {
                    int index = filename.LastIndexOf(".");
                    string basefile = filename.Substring(0, index);

                    Process process = new Process();

                    process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    process.StartInfo.FileName = HttpRuntime.AppDomainAppPath + "UploadedFiles\\FileConversion.exe";

                    string arguments = "-i ";
                    arguments += HttpRuntime.AppDomainAppPath + InputFolder + "\\" + filename;
                    arguments += " -t .dae -o " + HttpRuntime.AppDomainAppPath + OutputFolder + "\\" + basefile + ".dae";


                    process.StartInfo.Arguments = arguments;
                    process.Start();

                    while (process.HasExited == false)
                    {
                        Thread.Sleep(1000);
                    }

                    process.Close();
                    return true;
                }
                catch 
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public bool ConvertToOBJ(string filename)
        {
            if (ValidateExtension(filename))
            {
                try
                {
                    int index = filename.LastIndexOf(".");
                    string basefile = filename.Substring(0, index);

                    Process process = new Process();

                    process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    process.StartInfo.FileName = HttpRuntime.AppDomainAppPath + "UploadedFiles\\FileConversion.exe";

                    string arguments = "-i ";
                    arguments += HttpRuntime.AppDomainAppPath + InputFolder + "\\" + filename;
                    arguments += " -t .dae -o " + HttpRuntime.AppDomainAppPath + OutputFolder + "\\" + basefile + ".obj";


                    process.StartInfo.Arguments = arguments;
                    process.Start();

                    while (process.HasExited == false)
                    {
                        Thread.Sleep(1000);
                    }

                    process.Close();
                    return true;
                }
                catch 
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public bool ConvertToSTL(string filename)
        {
            if (ValidateExtension(filename))
            {
                try
                {
                    int index = filename.LastIndexOf(".");
                    string basefile = filename.Substring(0, index);

                    Process process = new Process();

                    process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    process.StartInfo.FileName = HttpRuntime.AppDomainAppPath + "UploadedFiles\\FileConversion.exe";

                    string arguments = "-i ";
                    arguments += HttpRuntime.AppDomainAppPath + InputFolder + "\\" + filename;
                    arguments += " -t .dae -o " + HttpRuntime.AppDomainAppPath + OutputFolder + "\\" + basefile + ".stl";


                    process.StartInfo.Arguments = arguments;
                    process.Start();

                    while (process.HasExited == false)
                    {
                        Thread.Sleep(1000);
                    }

                    process.Close();
                    return true;
                }
                catch 
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public bool ConvertToPLY(string filename)
        {
            if (ValidateExtension(filename))
            {
                try
                {
                    int index = filename.LastIndexOf(".");
                    string basefile = filename.Substring(0, index);

                    Process process = new Process();

                    process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    process.StartInfo.FileName = HttpRuntime.AppDomainAppPath + "UploadedFiles\\FileConversion.exe";

                    string arguments = "-i ";
                    arguments += HttpRuntime.AppDomainAppPath + InputFolder + "\\" + filename;
                    arguments += " -t .dae -o " + HttpRuntime.AppDomainAppPath + OutputFolder + "\\" + basefile + ".ply";


                    process.StartInfo.Arguments = arguments;
                    process.Start();

                    while (process.HasExited == false)
                    {
                        Thread.Sleep(1000);
                    }

                    process.Close();
                    return true;
                }
                catch 
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public bool ValidateExtension(string filename)
        {
            int index = filename.LastIndexOf(".");
            string extension = filename.Substring(index);

            if(extension == ".fbx" || extension == ".dae" || extension == ".obj" || extension == ".stl" || extension == ".ply")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}