﻿using Microsoft.EntityFrameworkCore;
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
                var fileName = Path.GetFileName(pluginFile.FileName);
                var filePath = Path.Combine(_uploadPath, fileName);

                if (File.Exists(filePath))
                {
                    return false;
                }

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await pluginFile.CopyToAsync(fileStream);
                }

                var plugin = new Plugin
                {
                    Name = pluginName,
                    Description = pluginDescription,
                    FilePath = filePath,
                    FileName = pluginFile.FileName,
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

        /*
        public async Task<Plugin> UploadPluginWithTagsAsync(
            IFormFile pluginFile,
            string pluginName,
            string pluginDescription,
            string githubUrl,
            List<int> tagIds,
            string userId)
        {
            if (pluginFile != null && pluginFile.Length > 0 && Path.GetExtension(pluginFile.FileName).ToLower() == ".dll")
            {
                var fileName = Path.GetFileName(pluginFile.FileName);
                var filePath = Path.Combine(_uploadPath, fileName);

                if (File.Exists(filePath))
                {
                    return null;
                }

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await pluginFile.CopyToAsync(fileStream);
                }

                var plugin = new Plugin
                {
                    Name = pluginName,
                    Description = pluginDescription,
                    FilePath = filePath,
                    FileName = pluginFile.FileName,
                    UploadedAt = DateTime.Now,
                    Version = "1.0",
                    AuthorId = userId,
                    PluginTags = new List<PluginTag>()
                };

               

                if (tagIds != null && tagIds.Count > 0)
                {
                    var tags = _context.PluginTags.Where(t => tagIds.Contains(t.Id)).ToList();
                    plugin.PluginTags = tags;
                }

                _context.Plugins.Add(plugin);
                await _context.SaveChangesAsync();

                return plugin;
            }

            return null;
        }
        */

        public async Task<Plugin> UploadPluginWithTagsAsync(
     IFormFile pluginFile,
     string pluginName,
     string pluginDescription,
     string githubUrl,
     List<int> tagIds,
     string userId,
     IFormFile backgroundImage)
        {
            if (pluginFile == null || pluginFile.Length == 0 || Path.GetExtension(pluginFile.FileName).ToLower() != ".dll")
                return null;

            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(pluginFile.FileName)}";
            var filePath = Path.Combine(_uploadPath, fileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await pluginFile.CopyToAsync(fileStream);
            }

            var plugin = new Plugin
            {
                Name = pluginName,
                Description = pluginDescription,
                FilePath = filePath,
                FileName = fileName,
                UploadedAt = DateTime.Now,
                Version = "1.0",
                AuthorId = userId,
                PluginTags = new List<PluginTag>(),
                GitHubUrl = githubUrl
            };

            if (backgroundImage != null && backgroundImage.Length > 0)
            {
                var bgFileName = $"{Guid.NewGuid()}_{Path.GetFileName(backgroundImage.FileName)}";
                var bgFilePath = Path.Combine(_uploadPath, bgFileName);

                using (var imageStream = new FileStream(bgFilePath, FileMode.Create))
                {
                    await backgroundImage.CopyToAsync(imageStream);
                }

                plugin.BackgroundImageUrl = bgFilePath;
            }

            if (tagIds != null && tagIds.Count > 0)
            {
                var tags = _context.PluginTags.Where(t => tagIds.Contains(t.Id)).ToList();
                plugin.PluginTags = tags;
            }

            _context.Plugins.Add(plugin);
            await _context.SaveChangesAsync();

            return plugin;
        }

    }
}
