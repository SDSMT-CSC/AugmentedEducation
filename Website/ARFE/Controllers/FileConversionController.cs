using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ARFE.Controllers
{
    public class FileConversionController : Controller
    {
        public string ConvertToFBX(string filename)
        {
            try
            {
                Process process = new Process();

                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.StartInfo.FileName = HttpRuntime.AppDomainAppPath + "Content\\FileConversion.exe";

                /*string arguments = "-i ";
                arguments += HttpRuntime.AppDomainAppPath + "UploadedFiles\\" + filename;
                arguments += " -t .fbx";

                process.StartInfo.Arguments = arguments;*/
                process.Start();
                process.Close();
                return "Pass";
            }
            catch (Exception ex)
            {
                return "Fail";
            }
        }
    }
}