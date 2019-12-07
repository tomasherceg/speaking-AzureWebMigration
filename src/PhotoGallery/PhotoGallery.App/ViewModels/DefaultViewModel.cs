using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.Hosting;
using PhotoGallery.Data.Services;
using PhotoGallery.Data.DTO;

namespace PhotoGallery.App.ViewModels
{
    public class DefaultViewModel : MasterPageViewModel
    {
        private readonly GalleryService galleryService;

        public List<GalleryListDTO> Galleries { get; set; }

        public DefaultViewModel(GalleryService galleryService)
        {
            this.galleryService = galleryService;
        }

        public override async Task PreRender()
        {
            Galleries = await galleryService.GetGalleries();
            await base.PreRender();
        }

    }
}
