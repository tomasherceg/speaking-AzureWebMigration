using System;

namespace PhotoGallery.Data.Model
{
    public class Photo
    {

        public string Id { get; set; }

        public string GalleryId { get; set; }

        public string FileName { get; set; }

        public int? Width { get; set; }

        public int? Height { get; set; }

        public DateTime? ProcessedDate { get; set; }

    }
}