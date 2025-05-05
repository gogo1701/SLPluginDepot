using Microsoft.Identity.Client;
using SLPluginDepotModels.Models;

namespace SLPluginDepotModels.Models
{
    public class Plugin
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }

        public DateTime UploadedAt { get; set; }

        //public string FileUrl { get; set; }
        public string GitHubUrl { get; set; }
        //public string ThumbnailUrl { get; set; }

        public string FilePath { get; set; }

        public string FileName { get; set; }


        public string AuthorId { get; set; }
        public ApplicationUser Author { get; set; }

        public string BackgroundImageUrl { get; set; }
        public int Rating { get; set; }

        //public int? OrganizationId { get; set; }
        //public Organization Organization { get; set; }

        public ICollection<PluginTag> PluginTags { get; set; }
        public ICollection<PluginRating> Ratings { get; set; }
    }
}
