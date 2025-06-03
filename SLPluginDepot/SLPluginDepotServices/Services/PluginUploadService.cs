using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SLPluginDepotDB;
using SLPluginDepotModels.Models;
using SLPluginDepotServices.Interfaces;

namespace SLPluginDepotServices.Services
{
    public class PluginUploadService : IPluginUploadService
    {
        private readonly ApplicationDbContext _context;
        private readonly string _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

        public PluginUploadService(ApplicationDbContext dbContext)
        {
            _context = dbContext;

            if (!Directory.Exists(_uploadPath))
            {
                Directory.CreateDirectory(_uploadPath);
            }
        }

        public async Task<IEnumerable<Plugin>> GetPluginsFromQueryAsync(string query)
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

            return await pluginsQuery.ToListAsync();
        }

        public async Task<IEnumerable<Plugin>> GetPluginsAsync()
        {
            var pluginsQuery = _context.Plugins
                .Include(p => p.Author)
                .Include(p => p.PluginTags)
                .Include(p => p.Ratings)
                .AsQueryable();

            return await pluginsQuery.ToListAsync();
        }

        public async Task<bool> UploadPluginAsync(IFormFile pluginFile, string pluginName, string pluginDescription, string userId)
        {
            if (pluginFile != null && pluginFile.Length > 0 && Path.GetExtension(pluginFile.FileName).ToLower() == ".dll")
            {
                var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(pluginFile.FileName)}";
                var filePath = Path.Combine(_uploadPath, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await pluginFile.CopyToAsync(fileStream);
                }

                var plugin = new Plugin
                {
                    Name = pluginName,
                    Description = pluginDescription,
                    FilePath = "/uploads/" + uniqueFileName, 
                    FileName = uniqueFileName,
                    UploadedAt = DateTime.Now,
                    Version = "1.0",
                    AuthorId = userId
                };

                _context.Plugins.Add(plugin);
                await _context.SaveChangesAsync();

                return true;
            }

            return false;
        }

        public async Task<Plugin> UploadPluginWithTagsAsync(
    IFormFile pluginFile,
    string pluginName,
    string pluginDescription,
    string githubUrl,
    List<int> tagIds,
    string userId,
    IFormFile backgroundImage)
        {
            var plugin = new Plugin
            {
                Name = pluginName,
                Description = pluginDescription,
                UploadedAt = DateTime.Now,
                Version = "1.0",
                AuthorId = userId,
                GitHubUrl = githubUrl,
                PluginTags = new List<PluginTag>()
            };

            // Create a temporary plugin ID for folder naming (or generate a GUID)
            var tempId = Guid.NewGuid().ToString();

            var pluginDirectory = Path.Combine(_uploadPath, tempId);
            if (!Directory.Exists(pluginDirectory))
                Directory.CreateDirectory(pluginDirectory);

            if (pluginFile != null && pluginFile.Length > 0 && Path.GetExtension(pluginFile.FileName).ToLower() == ".dll")
            {
                var uniquePluginFileName = $"{Guid.NewGuid()}_{Path.GetFileName(pluginFile.FileName)}";
                var pluginFilePath = Path.Combine(pluginDirectory, uniquePluginFileName);

                using (var fileStream = new FileStream(pluginFilePath, FileMode.Create))
                {
                    await pluginFile.CopyToAsync(fileStream);
                }

                plugin.FilePath = $"/uploads/{tempId}/{uniquePluginFileName}";
                plugin.FileName = uniquePluginFileName;
            }
            else
            {
                // Return null or throw error: plugin file is required
                throw new ArgumentException("Plugin file is required and must be a .dll");
            }

            if (backgroundImage != null && backgroundImage.Length > 0)
            {
                var uniqueImageFileName = $"{Guid.NewGuid()}_{Path.GetFileName(backgroundImage.FileName)}";
                var imagePath = Path.Combine(pluginDirectory, uniqueImageFileName);

                using (var imageStream = new FileStream(imagePath, FileMode.Create))
                {
                    await backgroundImage.CopyToAsync(imageStream);
                }

                plugin.BackgroundImageUrl = $"/uploads/{tempId}/{uniqueImageFileName}";
            }
            else
            {
                plugin.BackgroundImageUrl = null;
            }

            if (tagIds != null && tagIds.Count > 0)
            {
                var tags = _context.PluginTags.Where(t => tagIds.Contains(t.Id)).ToList();
                plugin.PluginTags = tags;
            }

            // Now add plugin with all properties set
            _context.Plugins.Add(plugin);
            await _context.SaveChangesAsync();

            // Optionally, rename the directory from tempId to plugin.Id (from DB)
            var newPluginDirectory = Path.Combine(_uploadPath, plugin.Id.ToString());
            if (Directory.Exists(pluginDirectory))
            {
                Directory.Move(pluginDirectory, newPluginDirectory);
            }

            return plugin;
        }


    }
}
