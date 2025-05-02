using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SLPluginDepotModels.Models
{
    public class ApplicationUser : IdentityUser
    {

        public string DiscordUsername { get; set;} = string.Empty;

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        //public ICollection<Plugin> Plugins { get; set; }             
        //public ICollection<PluginRating> Ratings { get; set; }       
        //public ICollection<PluginDownload> Downloads { get; set; }   
        //public ICollection<OrganizationMember> OrganizationMemberships { get; set; }
        //public ICollection<FavoritePlugin> Favorites { get; set; }
    }
}
