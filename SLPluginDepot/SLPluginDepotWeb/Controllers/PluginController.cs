using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using SLPluginDepotServices.Interfaces;
using SLPluginDepotModels.Models;

namespace SLPluginDepotWeb.Controllers
{
    public class PluginController : Controller
    {
        private readonly IPluginService _pluginService;
        private readonly IPluginUploadService _pluginUploadService;

        public PluginController(IPluginService pluginService, IPluginUploadService pluginUploadService)
        {
            _pluginService = pluginService;
            _pluginUploadService = pluginUploadService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var plugins = _pluginService.GetPlugins();
            return View(plugins);
        }

        [HttpGet]
        public IActionResult AddPlugin()
        {
            TempData.Clear();
            ViewBag.AvailableTags = _pluginService.GetAllTags();
            return View();
        }

        [HttpPost]
        
        public async Task<IActionResult> AddPlugin(IFormFile pluginFile, string pluginName, string pluginDescription, string githubUrl, List<int> selectedTags, IFormFile backgroundImage)
        {
            var userId = User.Identity?.Name ?? "demo-user"; // Replace with actual user ID retrieval if needed

            var result = await _pluginUploadService.UploadPluginWithTagsAsync(
                pluginFile,
                pluginName,
                pluginDescription,
                githubUrl,
                selectedTags,
                userId,
                backgroundImage // Pass the background image here
            );

            if (result != null)
            {
                TempData["Message"] = "Plugin uploaded successfully!";
                return RedirectToAction("Index");
            }

            TempData["Error"] = "Upload failed. Please check your inputs.";
            ViewBag.AvailableTags = _pluginService.GetAllTags();
            return View();
        }




        [HttpGet]
        public IActionResult Search(string query)
        {
            var plugins = _pluginService.GetPluginsFromQuery(query);
            return View("Index", plugins);
        }
    }
}
