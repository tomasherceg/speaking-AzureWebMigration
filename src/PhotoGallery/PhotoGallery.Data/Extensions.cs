using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGallery.Data
{
    public static class Extensions
    {

        public static async Task<List<T>> ToListAsync<T>(this FeedIterator<T> iterator)
        {
            var results = new List<T>();
            do
            {
                var items = await iterator.ReadNextAsync();
                results.AddRange(items);
            }
            while (iterator.HasMoreResults);
            return results;
        }

        public static async Task<T> FirstOrDefaultAsync<T>(this FeedIterator<T> iterator)
        {
            var items = await iterator.ReadNextAsync();
            return items.FirstOrDefault();
        }

    }
}
