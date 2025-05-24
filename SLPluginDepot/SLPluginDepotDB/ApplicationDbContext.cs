using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SLPluginDepotModels.Models;

namespace SLPluginDepotDB
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Plugin> Plugins { get; set; }
        public DbSet<PluginTag> PluginTags { get; set; }
        public DbSet<PluginRating> PluginRatings { get; set; }
        public DbSet<PluginDownload> PluginDownloads { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Plugin>()
                .HasMany(p => p.PluginTags);

            modelBuilder.Entity<PluginRating>()
                .HasOne(pr => pr.Plugin)
                .WithMany(p => p.Ratings)
                .HasForeignKey(pr => pr.PluginId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PluginTag>().HasData(
                new PluginTag { Id = 1, Name = "Exiled" },
                new PluginTag { Id = 2, Name = "LabAPI" },
                new PluginTag { Id = 3, Name = "Miscellaneous" },
                new PluginTag { Id = 4, Name = "Items" },
                new PluginTag { Id = 5, Name = "Roles" },
                new PluginTag { Id = 6, Name = "Moderation" },
                new PluginTag { Id = 7, Name = "Map" }
            );
        }
    }
}
