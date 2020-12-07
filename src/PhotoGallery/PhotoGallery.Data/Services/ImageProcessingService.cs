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
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using PhotoGallery.Data.Configuration;
using System.Linq;
using Microsoft.Azure.Cosmos.Linq;

namespace PhotoGallery.Data.Services
{
    public class ImageProcessingService
    {
        private readonly IPhotoStorageService photoStorageService;

        private readonly Container galleriesContainer;
        private readonly Container photosContainer;

        public ImageProcessingService(CosmosClient client, IOptions<CosmosOptions> options, IPhotoStorageService photoStorageService)
        {
            this.galleriesContainer = client.GetContainer(options.Value.DatabaseId, "galleries");
            this.photosContainer = client.GetContainer(options.Value.DatabaseId, "photos");

            this.photoStorageService = photoStorageService;
        }

        public async Task ProcessImage(string id, string galleryId)
        {
            var unprocessedPhoto = (await photosContainer.ReadItemAsync<Photo>(id, new PartitionKey(galleryId))).Resource;
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

                    await photosContainer.UpsertItemAsync<Photo>(unprocessedPhoto);
                    
                    var gallery = (await galleriesContainer.ReadItemAsync<Gallery>(galleryId, new PartitionKey(galleryId))).Resource;
                    gallery.ProcessedPhotosCount++;
                    if (gallery.ThumbnailPhotoId == null)
                    {
                        gallery.ThumbnailPhotoId = unprocessedPhoto.Id;
                    }
                    await galleriesContainer.UpsertItemAsync<Gallery>(gallery);

                    Console.WriteLine($"Photo processed successfully.");
                }
            }
            catch (Exception ex)
            {
                // invalid photo - remove it
                await photosContainer.DeleteItemAsync<Photo>(id, new PartitionKey(galleryId));

                Console.WriteLine($"Error processing photo. {ex}");
            }
        }

        private async Task<Image<Rgba32>> LoadImageAsync(string id, IPhotoStorageService photoStorageService)
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
