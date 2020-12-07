using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;
using PhotoGallery.Data.DTO;
using PhotoGallery.Data.Services;

namespace PhotoGallery.App.ViewModels
{
    public class GalleryViewModel : MasterPageViewModel
    {
        private readonly GalleryService galleryService;

        public List<PhotoListDTO> Photos { get; set; }

        [FromRoute("id")]
        public string GalleryId { get; set; }


        public GalleryViewModel(GalleryService galleryService)
        {
            this.galleryService = galleryService;
        }

        public override async Task PreRender()
        {
            Photos = await galleryService.GetPhotos(GalleryId);
            await base.PreRender();
        }

    }
}

