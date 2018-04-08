using System.Timers;

using Owin;
using Microsoft.Owin;

[assembly: OwinStartupAttribute(typeof(ARFE.Startup))]
namespace ARFE
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            string uploadDir = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "UploadedFiles");
            //news up a static instance
            UploadedFileCache uploadedFiles = UploadedFileCache.GetInstance();
            uploadedFiles.BasePath = uploadDir;

            BlobManager blobManager = new BlobManager();
            blobManager.GetOrCreateBlobContainer("public");

            TimerInit();
        }

        public void TimerInit()
        {
            Timer timer = new Timer();

            timer.AutoReset = true;
            timer.Elapsed += Timer_Elapsed;
            timer.Interval = (5 * 60 * 1000); //5 min in milliseconds

            timer.Enabled = true;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            UploadedFileCache.DeleteOldFiles();
        }
    }
}
