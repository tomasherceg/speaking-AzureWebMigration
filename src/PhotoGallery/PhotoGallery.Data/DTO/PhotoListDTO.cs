using System;
using System.Collections.Generic;
using System.Text;

namespace PhotoGallery.Data.DTO
{
    public class PhotoListDTO
    {

        public Guid Id { get; set; }

        public string FileName { get; set; }

        public int? Width { get; set; }

        public int? Height { get; set; }

        public bool IsProcessed { get; set; }
    }
}
