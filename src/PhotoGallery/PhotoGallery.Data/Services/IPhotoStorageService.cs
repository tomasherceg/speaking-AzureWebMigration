using System;
using System.IO;
using System.Threading.Tasks;

namespace PhotoGallery.Data.Services
{
    public interface IPhotoStorageService
    {

        Task StorePhoto(Guid id, Stream stream);

        Task<Stream> GetPhoto(Guid id);

    }
}