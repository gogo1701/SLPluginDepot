using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SLPluginDepotDB;
using SLPluginDepotModels.Models;
using SLPluginDepotWeb.Controllers;
using System.Linq;
using System.Threading.Tasks;

namespace SLPluginDepotWeb.Controllers
{
    public class PluginBrowserController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PluginBrowserController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string query)
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

            var viewModel = new PluginBrowser
            {
                Plugins = await pluginsQuery.ToListAsync()
            };

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult AddNewPlugin()
        {
            TempData["Message"] = "Add plugin functionality not implemented.";
            return RedirectToAction("Index");
        }
    }
}
