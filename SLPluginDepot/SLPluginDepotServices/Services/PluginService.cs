using SLPluginDepotServices.Interfaces;
using SLPluginDepotModels.Models;
using SLPluginDepotDB;
using Microsoft.EntityFrameworkCore;


namespace SLPluginDepotServices.Services
{
    public class PluginService : IPluginService
    {
        private ApplicationDbContext _context;

        public PluginService(ApplicationDbContext dbContext)
        {
            _context = dbContext;
        }
        public IEnumerable<Plugin> GetPluginsFromQuery(string query)
        {
            var pluginsQuery = _context.Plugins
               .Include(p => p.Author)
               .Include(p => p.PluginTags)
               .Include(p => p.Ratings)
               .AsQueryable();

            if (!string.IsNullOrWhiteSpace(query))
            {
                var loweredQuery = query.ToLower();

                pluginsQuery = pluginsQuery.Where(p =>
                    EF.Functions.Like(p.Name.ToLower(), $"%{loweredQuery}%") ||
                    EF.Functions.Like(p.Description.ToLower(), $"%{loweredQuery}%"));
            }

            return pluginsQuery.ToList(); 
            
        }


        public async Task<Plugin> GetPluginByIdAsync(int id)
        {
            return await _context.Plugins
                .Include(p => p.Author)
                .Include(p => p.PluginTags)
                .Include(p => p.Ratings)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public IEnumerable<Plugin> GetPlugins()
        {
            var pluginsQuery = _context.Plugins
               .Include(p => p.Author)
               .Include(p => p.PluginTags)
               .Include(p => p.Ratings)
               .AsQueryable();

            return pluginsQuery;
        }
        public IEnumerable<PluginTag> GetAllTags()
        {
            return _context.PluginTags.ToList();
        }
    }
}
