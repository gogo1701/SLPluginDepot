using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using SLPluginDepotServices.Interfaces;
using SLPluginDepotModels.Models;
using Microsoft.EntityFrameworkCore;
using SLPluginDepotDB;
using System.Security.Claims;

namespace SLPluginDepotWeb.Controllers
{
    public class PluginController : Controller
    {
        private readonly IPluginService _pluginService;
        private readonly IPluginUploadService _pluginUploadService;
        private readonly IRatingService _ratingService;
        private readonly ApplicationDbContext _context;
        private readonly string _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

        public PluginController(IPluginService pluginService, IPluginUploadService pluginUploadService, IRatingService ratingService, ApplicationDbContext context)
        {
            _pluginService = pluginService;
            _pluginUploadService = pluginUploadService;
            _ratingService = ratingService;
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var plugins = _pluginService.GetPlugins();
            return View(plugins);
        }

        [HttpGet]
        public async Task<IActionResult> DownloadPlugin(int id)
        {
            var plugin = await _pluginService.GetPluginByIdAsync(id);

            if (plugin == null)
            {
                return NotFound();
            }

            var filePath = plugin.FilePath;

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            var fileName = Path.GetFileName(filePath);

            return File(fileBytes, "application/octet-stream", fileName);
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
        public async Task<IActionResult> RatePlugin(int id, double rating, string review)
        {
            var userId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier); // Get user ID from claims
            if (string.IsNullOrEmpty(userId))
            {
                // Handle the case where the user is not authenticated or no userId is found
                return Unauthorized("You must be logged in to rate this plugin.");
            }

            var plugin = await _pluginService.GetPluginByIdAsync(id);

            if (plugin == null)
            {
                return NotFound();
            }

            // Check if the user exists in the AspNetUsers table
            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
            {
                // Handle the case where the user does not exist
                return Unauthorized("User does not exist in the system.");
            }

            // Now safely add the rating
            await _ratingService.AddRatingAsync(plugin, userId, rating, review);

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
