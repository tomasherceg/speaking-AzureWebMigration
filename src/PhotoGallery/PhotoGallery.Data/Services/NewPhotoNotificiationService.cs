using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Queue;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGallery.Data.Services
{
    public class NewPhotoNotificiationService
    {
        private CloudQueue queue;

        public NewPhotoNotificiationService(string connectionString, string queueName)
        {
            var account = CloudStorageAccount.Parse(connectionString);
            var queueClient = account.CreateCloudQueueClient();

            queue = queueClient.GetQueueReference(queueName);
            queue.CreateIfNotExists();
        }

        public async Task NotifyNewPhotoUploaded(Guid id)
        {
            await queue.AddMessageAsync(new CloudQueueMessage(id.ToString()));
        }

    }
}
