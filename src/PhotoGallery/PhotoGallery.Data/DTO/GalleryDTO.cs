using System;
using System.Collections.Generic;
using System.Text;

namespace PhotoGallery.Data.DTO
{
    public class GalleryListDTO
    {

        public int Id { get; set; }

        public string Title { get; set; }

        public DateTime CreatedDate { get; set; }

        public int PhotosCount { get; set; }

        public bool IsProcessed { get; set; }

        public Guid? ThumbnailPhotoId { get; set; }

    }
}
