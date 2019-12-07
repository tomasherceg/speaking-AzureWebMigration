using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.Controls;
using System.ComponentModel.DataAnnotations;
using PhotoGallery.Data.Services;
using PhotoGallery.Data.DTO;
using DotVVM.Framework.Storage;

namespace PhotoGallery.App.ViewModels
{
    public class CreateGalleryViewModel : MasterPageViewModel
    {
        private readonly GalleryService galleryService;
        private readonly IUploadedFileStorage uploadedFileStorage;

        [Required]
        public string Title { get; set; }

        public UploadedFilesCollection Files { get; set; } = new UploadedFilesCollection();

        public CreateGalleryViewModel(GalleryService galleryService, IUploadedFileStorage uploadedFileStorage)
        {
            this.galleryService = galleryService;
            this.uploadedFileStorage = uploadedFileStorage;
        }

        public async Task CreateGallery()
        {
            var gallery = new GalleryNewDTO()
            {
                Title = Title,
                Photos = Files.Files
                    .Select(f => new PhotoNewDTO()
                    {
                        FileName = f.FileName,
                        Stream = uploadedFileStorage.GetFile(f.FileId)
                    })
                    .ToList()
            };

            try
            {
                await galleryService.CreateGallery(gallery);
                Context.RedirectToRoute("Default");
            }
            finally
            {
                foreach (var photo in gallery.Photos)
                {
                    photo.Stream?.Dispose();
                }
            }
        }
    }
}

