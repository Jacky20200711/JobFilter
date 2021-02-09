using JobFilter.Data;
using JobFilter.Models.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;

namespace JobFilter.Controllers
{
    [Authorize(Roles = "Admin")]
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
                // 調用 CSVService 提供的函數來匯出設定檔
                CSVService.ExportFilterSetting(_context);
                return RedirectToRoute(new { controller = "Setting", action = "Index" });
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.ToString());
                ViewBag.Error = "匯出設定時發生錯誤，請查看LOG!";
                return View("~/Views/Shared/ErrorPage.cshtml");
            }
        }

        public IActionResult ImportFilterSetting()
        {
            try
            {
                // 調用 CSVService 提供的函數來匯入設定檔
                CSVService.ImportFilterSetting(_context);
                return RedirectToRoute(new { controller = "Setting", action = "Index" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                ViewBag.Error = "匯入設定時發生錯誤，請查看LOG!";
                return View("~/Views/Shared/ErrorPage.cshtml");
            }
        }

        public IActionResult ExportUser()
        {
            try
            {
                // 調用 CSVService 提供的函數來匯出用戶資料
                CSVService.ExportUser(_context);
                return RedirectToRoute(new { controller = "User", action = "Index" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                ViewBag.Error = "匯出用戶時發生錯誤，請查看LOG!";
                return View("~/Views/Shared/ErrorPage.cshtml");
            }
        }

        public IActionResult ImportUser()
        {
            try
            {
                // 調用 CSVService 提供的函數來匯入用戶資料
                CSVService.ImportUser(_context);
                return RedirectToRoute(new { controller = "User", action = "Index" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                ViewBag.Error = "匯入用戶時發生錯誤，請查看LOG!";
                return View("~/Views/Shared/ErrorPage.cshtml");
            }
        }
    }
}
