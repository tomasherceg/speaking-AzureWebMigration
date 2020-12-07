using System;
using System.IO;
using System.Threading.Tasks;

namespace PhotoGallery.Data.Services
{
    public interface IPhotoStorageService
    {

        Task StorePhoto(string id, Stream stream);

        Task<Stream> GetPhoto(string id);

    }
}