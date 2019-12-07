using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PhotoGallery.Data.Model;
using PhotoGallery.Data.Services;
using System;
using System.IO;
using System.Threading.Tasks;

namespace PhotoGallery.Worker
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            var serviceProvider = services.BuildServiceProvider();

            await RunWorker(serviceProvider.GetService<IServiceScopeFactory>());
        }

        private static void ConfigureServices(ServiceCollection services)
        {
            var config = new ConfigurationBuilder()
                            .AddJsonFile("appsettings.json")
                            .Build();

            var photosDirectory = config.GetSection("AppSettings").GetValue<string>("PhotosDirectory");

            services.AddScoped<ImageProcessingService>();
            services.AddSingleton<IPhotoStorageService>(
                provider => new PhotoStorageService(photosDirectory));

            services.AddEntityFrameworkSqlServer()
                .AddDbContext<AppDbContext>(options =>
                {
                    options.UseSqlServer(config.GetConnectionString("Sql"));
                });
        }

        private static async Task RunWorker(IServiceScopeFactory serviceScopeFactory)
        {
            while (true)
            {
                using (var scope = serviceScopeFactory.CreateScope())
                {
                    var processingService = scope.ServiceProvider.GetService<ImageProcessingService>();
                    await processingService.ProcessImages();
                };

                await Task.Delay(30000);
            }
        }

    }
}
