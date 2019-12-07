using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGallery.Data.Services
{
    public class AzureBlobPhotoStorageService : IPhotoStorageService
    {
        private CloudBlobContainer container;

        public AzureBlobPhotoStorageService(string connectionString, string containerName)
        {
            var account = CloudStorageAccount.Parse(connectionString);
            var blobClient = account.CreateCloudBlobClient();
            
            container = blobClient.GetContainerReference(containerName);
            container.CreateIfNotExists(BlobContainerPublicAccessType.Off);
        }

        public Task<Stream> GetPhoto(Guid id)
        {
            var blob = container.GetBlockBlobReference(id + ".bin");
            return blob.OpenReadAsync();
        }

        public Task StorePhoto(Guid id, Stream stream)
        {
            var blob = container.GetBlockBlobReference(id + ".bin");
            return blob.UploadFromStreamAsync(stream);
        }
    }
}
