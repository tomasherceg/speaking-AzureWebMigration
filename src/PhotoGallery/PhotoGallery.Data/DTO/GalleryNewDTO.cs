using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text;

namespace PhotoGallery.Data.DTO
{
    public class GalleryNewDTO
    {

        [Required]
        public string Title { get; set; }

        public List<PhotoNewDTO> Photos { get; set; }

    }
}
