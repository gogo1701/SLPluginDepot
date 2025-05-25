using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SLPluginDepotDB;
using SLPluginDepotModels.Models;
using SLPluginDepotServices.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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
            // Step 1: Create a new plugin entity first (without pluginId)
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

            // Step 2: Add the plugin to the database to generate the pluginId
            _context.Plugins.Add(plugin);
            await _context.SaveChangesAsync(); // Now pluginId will be generated

            // Step 3: Get the generated pluginId from the database
            var pluginId = plugin.Id.ToString();  // This is the unique identifier for the plugin

            // Step 4: Create a folder for the plugin using its pluginId
            var pluginDirectory = Path.Combine(_uploadPath, pluginId);
            if (!Directory.Exists(pluginDirectory))
            {
                Directory.CreateDirectory(pluginDirectory);
            }

            // Step 5: Handle the plugin file upload
            if (pluginFile != null && pluginFile.Length > 0 && Path.GetExtension(pluginFile.FileName).ToLower() == ".dll")
            {
                var uniquePluginFileName = $"{Guid.NewGuid()}_{Path.GetFileName(pluginFile.FileName)}";
                var pluginFilePath = Path.Combine(pluginDirectory, uniquePluginFileName);

                // Save plugin file
                using (var fileStream = new FileStream(pluginFilePath, FileMode.Create))
                {
                    await pluginFile.CopyToAsync(fileStream);
                }

                plugin.FilePath = $"/uploads/{pluginId}/{uniquePluginFileName}";
                plugin.FileName = uniquePluginFileName;
            }

            // Step 6: Handle  background image upload
            if (backgroundImage != null && backgroundImage.Length > 0)
            {
                var uniqueImageFileName = $"{Guid.NewGuid()}_{Path.GetFileName(backgroundImage.FileName)}";
                var imagePath = Path.Combine(pluginDirectory, uniqueImageFileName);

                // Save image file
                using (var imageStream = new FileStream(imagePath, FileMode.Create))
                {
                    await backgroundImage.CopyToAsync(imageStream);
                }

                plugin.BackgroundImageUrl = $"/uploads/{pluginId}/{uniqueImageFileName}";
            }

            if (tagIds != null && tagIds.Count > 0)
            {
                var tags = _context.PluginTags.Where(t => tagIds.Contains(t.Id)).ToList();
                plugin.PluginTags = tags;
            }

            _context.Plugins.Update(plugin);
            await _context.SaveChangesAsync();

            return plugin;
        }

    }
}
