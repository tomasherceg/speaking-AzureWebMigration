using Microsoft.EntityFrameworkCore;
using PhotoGallery.Data.DTO;
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
        private readonly AppDbContext dbContext;
        private readonly IPhotoStorageService photoStorageService;

        public GalleryService(AppDbContext dbContext, IPhotoStorageService photoStorageService)
        {
            this.dbContext = dbContext;
            this.photoStorageService = photoStorageService;
        }

        public Task<List<GalleryListDTO>> GetGalleries()
        {
            return dbContext.Galleries
                .Select(g => new GalleryListDTO()
                {
                    Id = g.Id,
                    Title = g.Title,
                    CreatedDate = g.CreatedDate,
                    PhotosCount = g.Photos.Count,
                    IsProcessed = g.Photos.All(p => p.ProcessedDate != null),
                    ThumbnailPhotoId = g.Photos.FirstOrDefault(p => p.ProcessedDate != null).Id
                })
                .OrderByDescending(g => g.CreatedDate)
                .ToListAsync();
        }

        public Task<List<PhotoListDTO>> GetPhotos(int galleryId)
        {
            return dbContext.Photos
                .Where(p => p.GalleryId == galleryId)
                .Select(g => new PhotoListDTO()
                {
                    Id = g.Id,
                    FileName = g.FileName,
                    Width = g.Width,
                    Height = g.Height,
                    IsProcessed = g.ProcessedDate != null
                })
                .ToListAsync();
        }

        public async Task CreateGallery(GalleryNewDTO galleryData)
        {
            var gallery = new Gallery()
            {
                Title = galleryData.Title,
                CreatedDate = DateTime.UtcNow
            };

            foreach (var photoData in galleryData.Photos)
            {
                var photo = new Photo()
                {
                    Id = Guid.NewGuid(),
                    FileName = photoData.FileName
                };
                gallery.Photos.Add(photo);

                await photoStorageService.StorePhoto(photo.Id, photoData.Stream);
            }

            dbContext.Galleries.Add(gallery);
            await dbContext.SaveChangesAsync();
        }

    }
}
