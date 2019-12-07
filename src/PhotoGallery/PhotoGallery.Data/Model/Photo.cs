using System;

namespace PhotoGallery.Data.Model
{
    public class Photo
    {

        public Guid Id { get; set; }

        public int GalleryId { get; set; }

        public virtual Gallery Gallery { get; set; }

        public string FileName { get; set; }

        public int? Width { get; set; }

        public int? Height { get; set; }

        public DateTime? ProcessedDate { get; set; }

    }
}