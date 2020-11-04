using JobFilter.Data;
using JobFilter.Models;
using JobFilter.Models.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobFilter.Controllers
{
    [Authorize]
    public class CSVController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CSVController(ApplicationDbContext context)
        {
            _context = context;
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
