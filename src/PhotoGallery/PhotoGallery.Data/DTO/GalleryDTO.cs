using System;
using System.Collections.Generic;
using System.Text;

namespace PhotoGallery.Data.DTO
{
    public class GalleryListDTO
    {

        public string Id { get; set; }

        public string Title { get; set; }

        public DateTime CreatedDate { get; set; }

        public int PhotosCount { get; set; }

        public int ProcessedPhotosCount { get; set; }

        public bool IsProcessed => PhotosCount == ProcessedPhotosCount;

        public string ThumbnailPhotoId { get; set; }

    }
}
