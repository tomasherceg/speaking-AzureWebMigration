using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Routing;
using PhotoGallery.Data.Services;
using PhotoGallery.Data.Model;
using System.IO;
using PhotoGallery.App.Presenters;
using Microsoft.Azure.Cosmos;
using PhotoGallery.Data.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Azure.Cosmos.Fluent;

namespace PhotoGallery.App
{
    public class Startup
    {
        private readonly IWebHostEnvironment env;

        public IConfigurationRoot Configuration { get; }

        public Startup(IWebHostEnvironment env)
        {
            Configuration = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(env.ContentRootPath, "appsettings.json"))
                .AddEnvironmentVariables()
                .Build();
            this.env = env;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDataProtection();
            services.AddAuthorization();
            services.AddWebEncoders();

            services.AddDotVVM<DotvvmStartup>();

            services.AddScoped<PhotoPresenter>();

            services.AddScoped<GalleryService>();
            services.AddSingleton<IPhotoStorageService>(
                //provider => new FileSystemPhotoStorageService(Path.Combine(env.ContentRootPath, "wwwroot/Photos"))
                provider => new AzureBlobPhotoStorageService(Configuration.GetConnectionString("BlobStorage"), "photos")
            );
            services.AddSingleton<NewPhotoNotificiationService>(
                provider => new NewPhotoNotificiationService(Configuration.GetConnectionString("BlobStorage"), "newphotos")
            );

            services.Configure<CosmosOptions>(Configuration.GetSection("Cosmos"));
            services.AddSingleton<CosmosClient>(new CosmosClientBuilder(
                    Configuration.GetConnectionString("Cosmos"))
                    .WithSerializerOptions(new CosmosSerializationOptions
                    {
                        PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                    }).Build());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            // use DotVVM
            var dotvvmConfiguration = app.UseDotVVM<DotvvmStartup>(env.ContentRootPath);
            dotvvmConfiguration.AssertConfigurationIsValid();
            
            // use static files
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(env.WebRootPath)
            });
        }
    }
}
