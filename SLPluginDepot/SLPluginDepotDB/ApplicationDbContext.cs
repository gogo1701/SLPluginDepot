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
                .HasMany(p => p.PluginTags);
                

            modelBuilder.Entity<PluginRating>()
    .HasOne(pr => pr.Plugin)
    .WithMany(p => p.Ratings)
    .HasForeignKey(pr => pr.PluginId)
    .OnDelete(DeleteBehavior.Restrict); 


        }



        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}