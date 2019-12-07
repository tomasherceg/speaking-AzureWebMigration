using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using PhotoGallery.Data.Services;
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

        public async Task Run([TimerTrigger("0 * * * * *")] TimerInfo timerInfo, ILogger log)
        {
            await imageProcessingService.ProcessImages();
        }
    }
}
