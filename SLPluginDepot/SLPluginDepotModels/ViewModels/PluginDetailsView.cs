using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLPluginDepotModels.Models
{
    public class PluginDetailsView
    {
        public Plugin Plugin { get; set; }
        public List<PluginRating> Ratings { get; set; }
        public double AverageRating { get; set; }
    }

}
