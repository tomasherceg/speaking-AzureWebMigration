using DotVVM.Framework.Hosting;
using PhotoGallery.Data.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhotoGallery.App.Presenters
{
    public class PhotoPresenter : IDotvvmPresenter
    {
        private readonly IPhotoStorageService photoStorageService;

        public PhotoPresenter(IPhotoStorageService photoStorageService)
        {
            this.photoStorageService = photoStorageService;
        }

        public async Task ProcessRequest(IDotvvmRequestContext context)
        {
            var id = (string)context.Parameters["id"];
            using (var stream = await photoStorageService.GetPhoto(id))
            {
                context.HttpContext.Response.ContentType = "image/jpeg";
                await stream.CopyToAsync(context.HttpContext.Response.Body);
            }
        }
    }
}
