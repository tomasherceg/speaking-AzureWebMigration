using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PhotoGallery.Data.Services
{
    public class FileSystemPhotoStorageService : IPhotoStorageService
    {
        private readonly string directory;

        public FileSystemPhotoStorageService(string directory)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            this.directory = directory;
        }

        public Task<Stream> GetPhoto(string id)
        {
            VerifyPhotoId(id);
            var path = Path.Combine(directory, id + ".bin");
            var stream = (Stream)File.OpenRead(path);
            return Task.FromResult(stream);
        }

        public async Task StorePhoto(string id, Stream stream)
        {
            VerifyPhotoId(id);
            var path = Path.Combine(directory, id + ".bin");
            using (var fs = File.OpenWrite(path))
            {
                await stream.CopyToAsync(fs);
            }
        }

        private void VerifyPhotoId(string id)
        {
            if (!Regex.IsMatch(id, "^[a-fA-F0-9-]+$"))
            {
                throw new SecurityException();
            }
        }

    }
}
