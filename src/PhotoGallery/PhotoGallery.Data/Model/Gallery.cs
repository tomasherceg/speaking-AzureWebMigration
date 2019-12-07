using System;
using System.Collections.Generic;
using System.Text;

namespace PhotoGallery.Data.Model
{
    public class Gallery
    {

        public int Id { get; set; }

        public string Title { get; set; }

        public DateTime CreatedDate { get; set; }

        public virtual ICollection<Photo> Photos { get; } = new List<Photo>();

    }
}
