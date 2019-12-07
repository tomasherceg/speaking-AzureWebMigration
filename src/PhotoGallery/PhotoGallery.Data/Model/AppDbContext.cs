using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace PhotoGallery.Data.Model
{
    public class AppDbContext : DbContext
    {

        public AppDbContext()
        {
        }

        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        public virtual DbSet<Gallery> Galleries { get; set; }

        public virtual DbSet<Photo> Photos { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // for dev-time migrations only
                optionsBuilder.UseSqlServer("Data Source=.\\SQLEXPRESS; Initial Catalog=AzureMigration; Integrated Security=true");
            }
            base.OnConfiguring(optionsBuilder);
        }

    }
}
