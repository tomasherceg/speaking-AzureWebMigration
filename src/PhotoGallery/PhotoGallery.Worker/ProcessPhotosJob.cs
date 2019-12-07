using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using PhotoGallery.Data.Services;
using System;
using System.Threading.Tasks;

namespace PhotoGallery.Worker
{
    public class ProcessPhotosJob
    {
        private readonly ImageProcessingService imageProcessingService;

        public ProcessPhotosJob(ImageProcessingService imageProcessingService)
        {
            this.imageProcessingService = imageProcessingService;
        }

        public async Task ProcessPhoto([QueueTrigger("newphotos", Connection = "BlobStorage")] string message, ILogger log)
        {
            var photoId = new Guid(message);
            await imageProcessingService.ProcessImage(photoId);
        }
    }
}
