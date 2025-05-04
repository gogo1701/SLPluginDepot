using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using SLPluginDepotServices.Interfaces;
using SLPluginDepotServices.Services;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace SLPluginDepotWeb.Controllers
{
    public class PluginController : Controller
    {
        private readonly IPluginService _pluginService;

        public PluginController(IPluginService pluginService)
        {
            _pluginService = pluginService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(_pluginService.GetPlugins());
        }

        [HttpGet]
        public async Task<IActionResult> Search(string query)
        {
            return View("Index", _pluginService.GetPluginsFromQuery(query));
        }

        [HttpPost]
        public IActionResult AddNewPlugin()
        {
            TempData["Message"] = "Add plugin functionality not implemented.";
            return RedirectToAction("Index");
        }
    }
}
