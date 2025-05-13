using SLPluginDepotModels.Models;
using System.Threading.Tasks;

namespace SLPluginDepotServices.Interfaces
{
    public interface IRatingService
    {
        Task AddRatingAsync(Plugin plugin, string userId, double rating, string review);
        Task<IEnumerable<PluginRating>> GetRatingsForPluginAsync(int pluginId);

        Task EditRatingAsync(int ratingId, double stars, string review);

        Task DeleteRatingAsync(int ratingId);
    }
}
