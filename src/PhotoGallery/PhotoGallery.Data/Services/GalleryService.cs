using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Options;
using PhotoGallery.Data.Configuration;
using PhotoGallery.Data.DTO;
using PhotoGallery.Data.Messages;
using PhotoGallery.Data.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGallery.Data.Services
{
    public class GalleryService
    {
        private readonly Container galleriesContainer;
        private readonly Container photosContainer;

        private readonly IPhotoStorageService photoStorageService;
        private readonly NewPhotoNotificiationService newPhotoNotificiationService;

        public GalleryService(CosmosClient client, IOptions<CosmosOptions> options, IPhotoStorageService photoStorageService, NewPhotoNotificiationService newPhotoNotificiationService)
        {
            this.galleriesContainer = client.GetContainer(options.Value.DatabaseId, "galleries");
            this.photosContainer = client.GetContainer(options.Value.DatabaseId, "photos");

            this.photoStorageService = photoStorageService;
            this.newPhotoNotificiationService = newPhotoNotificiationService;
        }

        public Task<List<GalleryListDTO>> GetGalleries()
        {
            return galleriesContainer.GetItemLinqQueryable<Gallery>()
                .Select(g => new GalleryListDTO()
                {
                    Id = g.Id,
                    Title = g.Title,
                    CreatedDate = g.CreatedDate,
                    PhotosCount = g.PhotosCount,
                    ProcessedPhotosCount = g.ProcessedPhotosCount,
                    ThumbnailPhotoId = g.ThumbnailPhotoId
                })
                .OrderByDescending(g => g.CreatedDate)
                .ToFeedIterator()
                .ToListAsync();
        }

        public Task<List<PhotoListDTO>> GetPhotos(string galleryId)
        {
            return photosContainer.GetItemLinqQueryable<Photo>()
                .Where(p => p.GalleryId == galleryId)
                .Select(g => new PhotoListDTO()
                {
                    Id = g.Id,
                    FileName = g.FileName,
                    Width = g.Width,
                    Height = g.Height,
                    IsProcessed = g.ProcessedDate != null
                })
                .ToFeedIterator()
                .ToListAsync();
        }

        public async Task CreateGallery(GalleryNewDTO galleryData)
        {
            var gallery = new Gallery()
            {
                Id = Guid.NewGuid().ToString(),
                Title = galleryData.Title,
                CreatedDate = DateTime.UtcNow,
                PhotosCount = galleryData.Photos.Count                
            };

            foreach (var photoData in galleryData.Photos)
            {
                var photo = new Photo()
                {
                    Id = Guid.NewGuid().ToString(),
                    GalleryId = gallery.Id,
                    FileName = photoData.FileName
                };
                await photosContainer.CreateItemAsync(photo);
                await photoStorageService.StorePhoto(photo.Id, photoData.Stream);
                await newPhotoNotificiationService.NotifyNewPhotoUploaded(new ProcessPhotoMessage() { Id = photo.Id, GalleryId = gallery.Id });
            }

            await galleriesContainer.CreateItemAsync(gallery);
        }

    }
}
