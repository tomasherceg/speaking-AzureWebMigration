using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGallery.Data.Services
{
    public class PhotoStorageService : IPhotoStorageService
    {
        private readonly string directory;

        public PhotoStorageService(string directory)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            this.directory = directory;
        }

        public Task<Stream> GetPhoto(Guid id)
        {
            var path = Path.Combine(directory, id + ".bin");
            var stream = (Stream)File.OpenRead(path);
            return Task.FromResult(stream);
        }

        public async Task StorePhoto(Guid id, Stream stream)
        {
            var path = Path.Combine(directory, id + ".bin");
            using (var fs = File.OpenWrite(path))
            {
                await stream.CopyToAsync(fs);
            }
        }

    }
}
