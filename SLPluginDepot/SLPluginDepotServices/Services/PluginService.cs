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
                pluginsQuery = pluginsQuery.Where(p =>
                    p.Name.Contains(query) || p.Description.Contains(query));
            }

            return pluginsQuery;
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
