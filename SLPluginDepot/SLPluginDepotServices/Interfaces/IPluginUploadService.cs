using Microsoft.AspNetCore.Http;

namespace SLPluginDepotServices.Interfaces
{
    public interface IPluginUploadService
    {
        Task<IEnumerable<Plugin>> GetPluginsFromQueryAsync(string query);
        Task<IEnumerable<Plugin>> GetPluginsAsync();
        Task<bool> UploadPluginAsync(IFormFile pluginFile, string pluginName, string pluginDescription, string userId);
    }
}
