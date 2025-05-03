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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Plugin>()
                .HasMany(p => p.PluginTags)
                .WithMany(t => t.Plugins);
        }



        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}