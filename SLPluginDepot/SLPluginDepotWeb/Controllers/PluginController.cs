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
        private readonly IRatingService _ratingService;

        public PluginController(IPluginService pluginService, IPluginUploadService pluginUploadService, IRatingService ratingService)
        {
            _pluginService = pluginService;
            _pluginUploadService = pluginUploadService;
            _ratingService = ratingService;
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

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var plugin = await _pluginService.GetPluginByIdAsync(id);

            if (plugin == null)
            {
                return NotFound();
            }

            var ratings = await _ratingService.GetRatingsForPluginAsync(id);

            var averageRating = ratings.Any() ? ratings.Average(r => r.Stars) : 0;

            var viewModel = new PluginDetailsView
            {
                Plugin = plugin,
                Ratings = ratings.ToList(),
                AverageRating = averageRating
            };

            return View(viewModel);
        }





        [HttpPost]
        public async Task<IActionResult> RatePlugin(int id, int rating)
        {
            var userId = User.Identity?.Name ?? "demo-user";
            var plugin = await _pluginService.GetPluginByIdAsync(id);

            if (plugin == null)
            {
                return NotFound();
            }

            var pluginRating = new PluginRating
            {
                PluginId = plugin.Id,
                UserId = userId,
                Stars = rating,
                Review = null,  
                RatedAt = DateTime.UtcNow
            };

            await _ratingService.AddRatingAsync(plugin, userId, rating);

            return RedirectToAction("Details", new { id = id });
        }






        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPlugin(
    IFormFile pluginFile,
    string pluginName,
    string pluginDescription,
    string githubUrl,
    List<int> selectedTags,
    IFormFile backgroundImage)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;


            var result = await _pluginUploadService.UploadPluginWithTagsAsync(
                pluginFile,
                pluginName,
                pluginDescription,
                githubUrl,
                selectedTags,
                userId,
                backgroundImage
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
