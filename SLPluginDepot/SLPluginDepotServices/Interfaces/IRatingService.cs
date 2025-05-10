using SLPluginDepotModels.Models;
using System.Threading.Tasks;

namespace SLPluginDepotServices.Interfaces
{
    public interface IRatingService
    {
        Task AddRatingAsync(Plugin plugin, string userId, int rating);
        Task<IEnumerable<PluginRating>> GetRatingsForPluginAsync(int pluginId);
    }
}
