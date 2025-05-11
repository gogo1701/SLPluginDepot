
namespace SLPluginDepotModels.Models
{
    public class PluginTag
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public ICollection<Plugin> Plugins { get; set; }
    }
}