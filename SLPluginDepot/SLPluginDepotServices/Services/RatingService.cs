using SLPluginDepotModels.Models;
using SLPluginDepotServices.Interfaces;
using Microsoft.EntityFrameworkCore;
using SLPluginDepotDB;

namespace SLPluginDepotServices.Services
{
    public class RatingService : IRatingService
    {
        private readonly ApplicationDbContext _context;

        public RatingService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PluginRating>> GetRatingsForPluginAsync(int pluginId)
        {
            return await _context.PluginRatings
                .Include(r => r.User) 
                .Where(r => r.PluginId == pluginId)
                .ToListAsync();
        }
        public async Task AddRatingAsync(Plugin plugin, string userId, double rating, string review)
        {
            if (rating < 1 || rating > 5)
                throw new ArgumentException("Rating must be between 1 and 5.");

            var existingRating = await _context.PluginRatings
                .FirstOrDefaultAsync(r => r.PluginId == plugin.Id && r.UserId == userId);

            if (existingRating != null)
            {
                existingRating.Stars = rating;
                existingRating.Review = review;
                existingRating.RatedAt = DateTime.Now;
                _context.PluginRatings.Update(existingRating);
            }
            else
            {
                var newRating = new PluginRating
                {
                    PluginId = plugin.Id,
                    UserId = userId,
                    Stars = rating,
                    Review = review,
                    RatedAt = DateTime.Now
                };
                _context.PluginRatings.Add(newRating);
            }

            await _context.SaveChangesAsync();
        }


    }
}
