using Microsoft.AspNetCore.Mvc;
using SLPluginDepotModels.Models;
using SLPluginDepotServices.Interfaces;
using Microsoft.EntityFrameworkCore;
using SLPluginDepotDB;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace SLPluginDepotWeb.Controllers
{
    public class PluginController : Controller
    {
        private readonly IPluginService _pluginService;
        private readonly IPluginUploadService _pluginUploadService;
        private readonly IRatingService _ratingService;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly string _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

        public PluginController(IPluginService pluginService, IPluginUploadService pluginUploadService, IRatingService ratingService, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _pluginService = pluginService;
            _pluginUploadService = pluginUploadService;
            _ratingService = ratingService;
            _context = context;
            _userManager = userManager;
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
        [Authorize]
        public IActionResult AddPlugin()
        {
            TempData.Clear();
            ViewBag.AvailableTags = _pluginService.GetAllTags();
            return View();
        }


        public IActionResult Details(int id)
        {
            var plugin = _context.Plugins
                .Include(p => p.Author)
                .Include(p => p.PluginTags)
                .Include(p => p.Ratings)
                    .ThenInclude(r => r.User)
                .FirstOrDefault(p => p.Id == id);

            if (plugin == null)
            {
                return NotFound();
            }

            var viewModel = new PluginDetailsView
            {
                Plugin = plugin,
                Ratings = plugin.Ratings.ToList(),
                AverageRating = plugin.Ratings.Any() ? plugin.Ratings.Average(r => r.Stars) : 0
            };

            return View(viewModel); // ✅ Correct model type
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
                return Unauthorized("User does not exist in the system.");
            }

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

        [HttpPost]
        public async Task<IActionResult> EditRating(int ratingId, double stars, string review)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var rating = await _context.PluginRatings.FirstOrDefaultAsync(r => r.Id == ratingId && r.UserId == userId);
            if (rating == null) return Unauthorized();

            await _ratingService.EditRatingAsync(ratingId, stars, review);
            return RedirectToAction("Details", new { id = rating.PluginId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteRating(int ratingId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var rating = await _context.PluginRatings.FirstOrDefaultAsync(r => r.Id == ratingId && r.UserId == userId);
            if (rating == null) return Unauthorized();

            var pluginId = rating.PluginId;
            await _ratingService.DeleteRatingAsync(ratingId);
            return RedirectToAction("Details", new { id = pluginId });
        }

        [HttpPost]
        public async Task<IActionResult> AddComment(int pluginId, float stars, string? review)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var existing = await _context.PluginRatings
                .FirstOrDefaultAsync(r => r.PluginId == pluginId && r.UserId == user.Id);

            if (existing == null)
            {
                existing = new PluginRating
                {
                    PluginId = pluginId,
                    UserId = user.Id,
                    Stars = stars,
                    Review = review
                };
                _context.PluginRatings.Add(existing);
            }
            else
            {
                existing.Stars = stars;
                existing.Review = review;
                existing.RatedAt = DateTime.UtcNow;
                _context.PluginRatings.Update(existing);
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> EditComment(int reviewId, int stars, string review)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var rating = await _context.PluginRatings.FirstOrDefaultAsync(r => r.Id == reviewId && r.UserId == userId);
            if (rating == null)
                return Unauthorized();

            rating.Stars = stars;
            rating.Review = review;
            rating.RatedAt = DateTime.Now; 

            await _context.SaveChangesAsync();

            return Ok();
        }



    }
}
