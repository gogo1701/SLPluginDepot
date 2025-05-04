namespace SLPluginDepotServices.Interfaces
{
    public interface IPluginService
    {
        public IEnumerable<Plugin> GetPluginsFromQuery(string query);
        public IEnumerable<Plugin> GetPlugins();
    }
}
