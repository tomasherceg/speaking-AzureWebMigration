using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Queue;
using Newtonsoft.Json;
using PhotoGallery.Data.Messages;
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

        public async Task NotifyNewPhotoUploaded(ProcessPhotoMessage message)
        {
            await queue.AddMessageAsync(new CloudQueueMessage(JsonConvert.SerializeObject(message)));
        }

    }
}
