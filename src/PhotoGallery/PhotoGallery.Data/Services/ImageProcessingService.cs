using Microsoft.EntityFrameworkCore;
using PhotoGallery.Data.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.IO;

namespace PhotoGallery.Data.Services
{
    public class ImageProcessingService
    {
        private readonly AppDbContext dbContext;
        private readonly IPhotoStorageService photoStorageService;

        public ImageProcessingService(AppDbContext dbContext, IPhotoStorageService photoStorageService)
        {
            this.dbContext = dbContext;
            this.photoStorageService = photoStorageService;
        }

        public async Task ProcessImage(Guid id)
        {
            var unprocessedPhoto = await dbContext.Photos.FirstOrDefaultAsync(p => p.Id == id);
            if (unprocessedPhoto.ProcessedDate != null)
            {
                return;
            }

            Console.WriteLine($"Processing photo {unprocessedPhoto.Id}...");
            try
            {
                using (var image = await LoadImageAsync(unprocessedPhoto.Id, photoStorageService))
                {
                    // process the image
                    image.Mutate(x => x
                        .Resize(new ResizeOptions()
                        {
                            Mode = ResizeMode.Max,
                            Size = new SixLabors.Primitives.Size(1600, 1600),
                        }));

                    await SaveImageAsync(photoStorageService, unprocessedPhoto, image);
                    
                    // store image size
                    unprocessedPhoto.Width = image.Width;
                    unprocessedPhoto.Height = image.Height;
                    unprocessedPhoto.ProcessedDate = DateTime.UtcNow;

                    Console.WriteLine($"Photo processed successfully");
                }
            }
            catch (Exception ex)
            {
                // invalid photo - remove it
                dbContext.Photos.Remove(unprocessedPhoto);

                Console.WriteLine($"Error processing photo. {ex}");
            }

            await dbContext.SaveChangesAsync();
        }


        private async Task<Image<Rgba32>> LoadImageAsync(Guid id, IPhotoStorageService photoStorageService)
        {
            using (var inputStream = await photoStorageService.GetPhoto(id))
            {
                return Image.Load<Rgba32>(inputStream);
            }
        }

        private async Task SaveImageAsync(IPhotoStorageService photoStorageService, Photo unprocessedPhoto, Image<Rgba32> image)
        {
            using (var ms = new MemoryStream())
            {
                image.Save(ms, new JpegEncoder() { Quality = 95 });
                ms.Position = 0;
                await photoStorageService.StorePhoto(unprocessedPhoto.Id, ms);
            }
        }
    }
}
