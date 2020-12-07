using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PhotoGallery.Data.Configuration;
using PhotoGallery.Data.Messages;
using PhotoGallery.Data.Model;
using PhotoGallery.Data.Services;
using System;
using System.IO;
using System.Threading.Tasks;

namespace PhotoGallery.Worker
{
    class Program
    {
        private static IConfigurationRoot config;
        private static IServiceProvider serviceProvider;

        static async Task Main(string[] args)
        {
            config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();

            var services = new ServiceCollection();
            services.AddSingleton(config);

            services.AddScoped<ImageProcessingService>();
            services.AddSingleton<IPhotoStorageService>(
                //provider => new FileSystemPhotoStorageService(config.GetSection("AppSettings").GetValue<string>("PhotosDirectory"))
                provider => new AzureBlobPhotoStorageService(config.GetConnectionString("BlobStorage"), "photos")
            );

            services.Configure<CosmosOptions>(config.GetSection("Cosmos"));
            services.AddSingleton<CosmosClient>(new CosmosClientBuilder(
                    config.GetConnectionString("Cosmos"))
                    .WithSerializerOptions(new CosmosSerializationOptions
                    {
                        PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                    }).Build());

            serviceProvider = services.BuildServiceProvider();

            await RunJob();
        }

        private static async Task RunJob()
        {
            var account = CloudStorageAccount.Parse(config.GetConnectionString("BlobStorage"));
            var queueClient = account.CreateCloudQueueClient();

            var queue = queueClient.GetQueueReference("newphotos");
            queue.CreateIfNotExists();

            while (true)
            {
                var message = await queue.GetMessageAsync();
                if (message == null)
                {
                    Console.WriteLine("Queue is empty...");
                    await Task.Delay(15000);
                    continue;
                }
                try
                {
                    Console.WriteLine("Processing message...");
                    await ProcessMessage(message);
                    await queue.DeleteMessageAsync(message);
                    Console.WriteLine("Message processed successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error processing message! " + ex);
                }
            };
        }

        private static async Task ProcessMessage(CloudQueueMessage message)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var imageProcessingService = scope.ServiceProvider.GetRequiredService<ImageProcessingService>();
                var msg = JsonConvert.DeserializeObject<ProcessPhotoMessage>(message.AsString);
                await imageProcessingService.ProcessImage(msg.Id, msg.GalleryId);
            }
        }
    }
}
