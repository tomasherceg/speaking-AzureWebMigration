using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
            var builder = new HostBuilder();
            builder.ConfigureWebJobs(b =>
                {
                    b.AddAzureStorageCoreServices();
                    b.AddTimers();
                })
                .ConfigureAppConfiguration(config =>
                {
                    config.AddJsonFile("appsettings.json");
                    config.AddEnvironmentVariables();
                })
                .ConfigureServices((context, services) =>
                {
                    var config = context.Configuration;

                    services.AddSingleton(config);
                    services.AddMemoryCache();

                    var photosDirectory = config.GetSection("AppSettings").GetValue<string>("PhotosDirectory");
                    
                    services.AddScoped<ImageProcessingService>();
                    services.AddSingleton<IPhotoStorageService>(
                        provider => new PhotoStorageService(photosDirectory));

                    services.AddEntityFrameworkSqlServer()
                        .AddDbContext<AppDbContext>(options =>
                        {
                            options.UseSqlServer(config.GetConnectionString("Sql"));
                        });
                })
                .UseConsoleLifetime();

            var host = builder.Build();
            using (host)
            {
                await host.RunAsync();
            }
        }

    }
}
