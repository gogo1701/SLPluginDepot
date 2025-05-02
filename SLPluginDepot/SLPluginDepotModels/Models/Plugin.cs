using SLPluginDepotModels.Models;

public class Plugin
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Version { get; set; }

    //public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    //public string FileUrl { get; set; }
    //public string ThumbnailUrl { get; set; }

    
    public string AuthorId { get; set; }
    public ApplicationUser Author { get; set; }

    //public int? OrganizationId { get; set; }
    //public Organization Organization { get; set; }

    //public ICollection<PluginTag> PluginTags { get; set; }
    //public ICollection<PluginRating> Ratings { get; set; }
}
