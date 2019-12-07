using System;
using System.IO;

namespace PhotoGallery.Data.DTO
{
    public class PhotoNewDTO
    {

        public string FileName { get; set; }

        public Stream Stream { get; set; }

    }
}