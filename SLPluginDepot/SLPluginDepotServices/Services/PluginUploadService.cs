using SLPluginDepotDB;
using SLPluginDepotServices.Interfaces;
using Microsoft.AspNetCore.Http; // <-- Add this for IFormFile
using Microsoft.EntityFrameworkCore;


namespace SLPluginDepotServices.Services
{
    public class PluginUploadService : IPluginUploadService
    {
        private readonly ApplicationDbContext _context;
        private readonly string _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

        public PluginUploadService(ApplicationDbContext dbContext)
        {
            _context = dbContext;

            // Ensure the upload folder exists
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
                // Generate a unique file name to avoid conflicts
                var fileName = Path.GetFileName(pluginFile.FileName);
                var filePath = Path.Combine(_uploadPath, fileName);

                // Ensure the file does not already exist
                if (File.Exists(filePath))
                {
                    return false; // A file with the same name already exists.
                }

                // Save the file to the file system
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await pluginFile.CopyToAsync(fileStream);
                }

                // Create a new Plugin object and save the file path to the database
                var plugin = new Plugin
                {
                    Name = pluginName,
                    Description = pluginDescription,
                    FilePath = filePath,  // Store the file path in the database
                    FileName = pluginFile.FileName,  // Store the original file name
                    UploadedAt = DateTime.Now,
                    Version = "1.0", // You can change this to handle versioning logic
                    AuthorId = userId  // Assuming userId is passed from the controller (i.e., the logged-in user)
                };

                _context.Plugins.Add(plugin);
                await _context.SaveChangesAsync();

                return true;
            }

            return false; // Invalid file or file type.
        }
    }
}
