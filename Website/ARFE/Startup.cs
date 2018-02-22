﻿using Microsoft.Owin;
using Owin;


[assembly: OwinStartupAttribute(typeof(ARFE.Startup))]
namespace ARFE
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            Controllers.BlobsController blobsController = new Controllers.BlobsController();
            blobsController.GetOrCreateBlobContainer("public");
        }
    }
}
