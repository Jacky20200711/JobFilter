using JobFilter.Data;
using JobFilter.Models;
using JobFilter.Models.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;

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
            try
            {
                if (!AuthorizeManager.InAdminGroup(User.Identity.Name)) return NotFound();
                CSVManager.ExportFilterSetting(_context);
                return RedirectToRoute(new { controller = "FilterSettings", action = "Index" });
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.ToString());
                ViewBag.Error = "匯出資料時發生錯誤，請查看LOG...";
                return View("~/Views/Shared/ErrorPage.cshtml");
            }
        }

        public IActionResult ImportFilterSetting()
        {
            try
            {
                if (!AuthorizeManager.InAdminGroup(User.Identity.Name)) return NotFound();
                CSVManager.ImportFilterSetting(_context);
                return RedirectToRoute(new { controller = "FilterSettings", action = "Index" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                ViewBag.Error = "匯入資料時發生錯誤，請查看LOG...";
                return View("~/Views/Shared/ErrorPage.cshtml");
            }
        }
    }
}
