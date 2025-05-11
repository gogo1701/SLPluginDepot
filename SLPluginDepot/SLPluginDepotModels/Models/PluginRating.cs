using SLPluginDepotModels.Models;

namespace SLPluginDepotModels.Models
{
    public class PluginRating
    {
        public int Id { get; set; }

        public int PluginId { get; set; }
        public Plugin Plugin { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public double Stars { get; set; }
        public string Review { get; set; }

        public DateTime RatedAt { get; set; }
    }
}