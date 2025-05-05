using Microsoft.AspNetCore.Http;
using SLPluginDepotModels.Models;

namespace SLPluginDepotServices.Interfaces
{
    public interface IPluginUploadService
    {
        Task<IEnumerable<Plugin>> GetPluginsFromQueryAsync(string query);
        Task<IEnumerable<Plugin>> GetPluginsAsync();
        Task<bool> UploadPluginAsync(IFormFile pluginFile, string pluginName, string pluginDescription, string userId);

        
        Task<Plugin> UploadPluginWithTagsAsync(
            IFormFile pluginFile,
            string pluginName,
            string pluginDescription,
            string githubUrl,
            List<int> tagIds,
            string userId,
            IFormFile backgroundImage);
    }
}
