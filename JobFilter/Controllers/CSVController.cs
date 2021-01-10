using JobFilter.Data;
using JobFilter.Models;
using JobFilter.Models.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JobFilter.Controllers
{
    [Authorize]
    public class CSVController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        public CSVController(ApplicationDbContext context, ILogger<UserController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult ExportFilterSetting()
        {
            if (!AuthorizeManager.InAdminGroup(User.Identity.Name)) return NotFound();

            CSVManager.ExportFilterSetting(_context);

            return RedirectToRoute(new { controller = "FilterSettings", action = "Index" });
        }

        public IActionResult ImportFilterSetting()
        {
            if (!AuthorizeManager.InAdminGroup(User.Identity.Name)) return NotFound();

            CSVManager.ImportFilterSetting(_context);

            return RedirectToRoute(new { controller = "FilterSettings", action = "Index" });
        }
    }
}
