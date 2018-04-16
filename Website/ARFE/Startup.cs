using System;
using System.IO;
using System.Timers;

using Owin;
using Microsoft.Owin;

[assembly: OwinStartupAttribute(typeof(ARFE.Startup))]
namespace ARFE
{
    public partial class Startup
    {
        #region Members

        /// <summary> A flag to signal when to purge old blobs when purging old temporary files. </summary>
        private static bool s_ShouldPurgeBlobs = true;

        #endregion


        #region Public Methods

        /// <summary>
        /// Method called automatically upon startup.  Initial app setup logic and configuration 
        /// goes here.
        /// </summary>
        /// <param name="app">The AE web app</param>
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            //define intermediate upload directory as ~/UploadedFiles/
            string uploadDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UploadedFiles");
            //news up a static instance of FileCache
            UploadedFileCache uploadedFiles = UploadedFileCache.GetInstance();
            uploadedFiles.BasePath = uploadDir;

            //make sure public blob container exists
            BlobManager blobManager = new BlobManager();
            blobManager.GetOrCreateBlobContainer("public");

            //start app self-cleaning timer
            TimerInit();
        }


        /// <summary>
        /// Create a timer that will fire a clean-up event every 15 minutes.
        /// </summary>
        public void TimerInit()
        {
            Timer timer = new Timer();

            //loop the timer
            timer.AutoReset = true;
            //call Timer_Elapsed() when time's up
            timer.Elapsed += Timer_Elapsed;
            //15 min in milliseconds
            timer.Interval = (2 * 60 * 1000); 

            //start
            timer.Enabled = true;
        }

        #endregion


        #region Private Methods

        /// <summary>
        /// A clean-up method.  Call the <see cref="UploadedFileCache.DeleteOldFiles"/> method 
        /// to remove all old files from intermediate file storage every time the event is fired.  
        /// Call the <see cref="BlobManager.RemoveExpiredTemporaryBlobs"/> method to remove
        /// all old temporary blobs from cloud storage every other time the event is fired.
        /// </summary>
        /// <param name="sender">The timer object that called this event.</param>
        /// <param name="e">Any "Timer's up" arguments. Not used.</param>
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            UploadedFileCache.DeleteOldFiles();

            //Only call when true, alternate each time
            //effectively a 30 min timer for purging blobs
            if (s_ShouldPurgeBlobs)
                BlobManager.RemoveExpiredTemporaryBlobs();

            s_ShouldPurgeBlobs = !s_ShouldPurgeBlobs;
        }

        #endregion
    }
}
