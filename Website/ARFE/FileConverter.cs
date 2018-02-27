using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ARFE.Controllers
{
    public class FileConverter 
    {
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
                    arguments += HttpRuntime.AppDomainAppPath + "UploadedFiles\\" + filename;
                    arguments += " -t .fbx -o " + HttpRuntime.AppDomainAppPath + "UploadedFiles\\"+ basefile+".fbx";


                    process.StartInfo.Arguments = arguments;
                    process.Start();
                    process.Close();
                    return true;
                }
                catch (Exception ex)
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
                    arguments += HttpRuntime.AppDomainAppPath + "UploadedFiles\\" + filename;
                    arguments += " -t .dae -o " + HttpRuntime.AppDomainAppPath + "UploadedFiles\\" + basefile + ".dae";


                    process.StartInfo.Arguments = arguments;
                    process.Start();
                    process.Close();
                    return true;
                }
                catch (Exception ex)
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
                    arguments += HttpRuntime.AppDomainAppPath + "UploadedFiles\\" + filename;
                    arguments += " -t .obj -o " + HttpRuntime.AppDomainAppPath + "UploadedFiles\\" + basefile + ".obj";


                    process.StartInfo.Arguments = arguments;
                    process.Start();
                    process.Close();
                    return true;
                }
                catch (Exception ex)
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
                    arguments += HttpRuntime.AppDomainAppPath + "UploadedFiles\\" + filename;
                    arguments += " -t .stl -o " + HttpRuntime.AppDomainAppPath + "UploadedFiles\\" + basefile + ".stl";


                    process.StartInfo.Arguments = arguments;
                    process.Start();
                    process.Close();
                    return true;
                }
                catch (Exception ex)
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
                    arguments += HttpRuntime.AppDomainAppPath + "UploadedFiles\\" + filename;
                    arguments += " -t .ply -o " + HttpRuntime.AppDomainAppPath + "UploadedFiles\\" + basefile + ".ply";


                    process.StartInfo.Arguments = arguments;
                    process.Start();
                    process.Close();
                    return true;
                }
                catch (Exception ex)
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