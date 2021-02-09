using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using JobFilter.Data;
using JobFilter.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using X.PagedList;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using JobFilter.Models.DataStructure;
using JobFilter.Models.Services;
using System;

namespace JobFilter.Controllers
{
    [Authorize]
    public class SettingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger _logger;

        public SettingController(ApplicationDbContext context, UserManager<IdentityUser> userManager, ILogger<UserController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int? page = 1)
        {
            try
            {
                page = page == null ? 1 : page;
                string UserEmail = User.Identity.Name;

                // 若用戶是管理員且有設置查看全部設定檔的Session，則列出所有的設定檔(由新到舊)
                if (HttpContext.Session.GetString("CheckAllSettings") != null && UserService.InAdminGroup(UserEmail))
                {
                    return View(await _context.FilterSetting.OrderByDescending(m => m.Id).ToPagedListAsync(page, 10));
                }
                // 否則只列出該用戶的設定檔(由新到舊)
                else
                {
                    return View(await _context.FilterSetting.Where(m => m.UserEmail == UserEmail).OrderByDescending(m => m.Id).ToPagedListAsync(page, 5));
                }
            }
            catch (Exception)
            {
                ViewBag.Error = "系統忙碌中，請稍後再試 >___<";
                return View("~/Views/Shared/ErrorPage.cshtml");
            }
        }

        [Authorize(Roles = "Admin")]
        public IActionResult SetSessionForCheckAllSettings()
        {
            // 設置查看全部設定檔的Session
            HttpContext.Session.SetString("CheckAllSettings", "1");
            return RedirectToAction("Index", new { page = 1 });
        }

        [Authorize(Roles = "Admin")]
        public IActionResult RemoveSessionOfCheckAllSettings()
        {
            // 移除查看全部設定檔的Session
            HttpContext.Session.Remove("CheckAllSettings");
            return RedirectToAction("Index", new { page = 1 });
        }

        public IActionResult Create(int? returnPage = 0)
        {
            // 紀錄之前所在的分頁號碼
            returnPage = returnPage == null ? 0 : returnPage;
            if (returnPage != 0)
            {
                HttpContext.Session.SetInt32("returnPage", (int)returnPage);
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CrawlUrl,ExcludeWord,IgnoreCompany,MinimumWage,MaximumWage,Remarks")] FilterSetting filterSetting)
        {
            try
            {
                // 調用 SettingService 提供的函數來建立設定檔，若中途出錯則跳轉到指定的錯誤頁面
                string ErrorMessage = SettingService.CreateSetting(_context, filterSetting, User.Identity.Name);
                if (ErrorMessage != null)
                {
                    ViewBag.Error = ErrorMessage;
                    return View("~/Views/Shared/ErrorPage.cshtml");
                }

                // 若中途沒有出錯則寫入變更到DB，並跳轉回設定檔列表
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ViewBag.Error = "系統忙碌中，請稍後再試 >___<";
                return View("~/Views/Shared/ErrorPage.cshtml");
            }
        }

        public async Task<IActionResult> Edit(int? id, int? returnPage = 0)
        {
            if (id == null)
            {
                return NotFound();
            }

            var filterSetting = await _context.FilterSetting.FindAsync(id);
            if (filterSetting == null)
            {
                return NotFound();
            }

            // 令管理員以外的用戶只能編輯自己的設定
            string UserEmail = User.Identity.Name;
            if (!UserService.InAdminGroup(User.Identity.Name) && filterSetting.UserEmail != UserEmail) return NotFound();

            // 紀錄之前所在的分頁號碼
            returnPage = returnPage == null ? 0 : returnPage;
            if (returnPage != 0)
            {
                HttpContext.Session.SetInt32("returnPage", (int)returnPage);
            }

            return View(filterSetting);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CrawlUrl,ExcludeWord,IgnoreCompany,MinimumWage,MaximumWage,Remarks")] FilterSetting filterSetting)
        {
            try
            {
                // 調用 SettingService 提供的函數來編輯設定檔，若中途出錯則跳轉到指定的錯誤頁面
                string ErrorMessage = SettingService.EditSetting(_context, filterSetting, User.Identity.Name, id);
                if (ErrorMessage != null)
                {
                    ViewBag.Error = ErrorMessage;
                    return View("~/Views/Shared/ErrorPage.cshtml");
                }

                // 若中途沒有出錯則寫入變更到DB，並跳轉回之前所在的設定檔列表分頁
                await _context.SaveChangesAsync();
                int? TryGetPage = HttpContext.Session.GetInt32("returnPage");
                int page = TryGetPage != null ? (int)TryGetPage : 1;
                return RedirectToAction("Index", new { page });
            }
            catch (Exception)
            {
                ViewBag.Error = "系統忙碌中，請稍後再試 >___<";
                return View("~/Views/Shared/ErrorPage.cshtml");
            }
        }

        public async Task<IActionResult> Delete(int? id, int? returnPage = 1)
        {
            try
            {
                // 調用 SettingService 提供的函數來刪除設定檔，若中途出錯則跳轉到指定的錯誤頁面
                string ErrorMessage = SettingService.DeleteSetting(_context, User.Identity.Name, id);
                if (ErrorMessage != null)
                {
                    ViewBag.Error = ErrorMessage;
                    return View("~/Views/Shared/ErrorPage.cshtml");
                }

                // 若中途沒有出錯則寫入變更到DB，並跳轉回之前所在的設定檔列表分頁
                await _context.SaveChangesAsync();
                int page = returnPage != null ? (int)returnPage : 1;
                return RedirectToAction("Index", new { page });
            }
            catch (Exception)
            {
                ViewBag.Error = "系統忙碌中，請稍後再試 >___<";
                return View("~/Views/Shared/ErrorPage.cshtml");
            }
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAll()
        {
            // 刪除所有的設定檔
            _context.RemoveRange(_context.FilterSetting);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> AddBlockCompany(string CompanyName, int? returnPage = 0)
        {
            try
            {
                // 檢查 Session 儲存的工作列表是否為空
                string JobListStr = HttpContext.Session.GetString("jobList");
                if (JobListStr == null)
                {
                    ViewBag.Error = "系統忙碌中，請稍後再試 >___<";
                    return View("~/Views/Shared/ErrorPage.cshtml");
                }

                // 紀錄之前所在的工作列表分頁
                returnPage = returnPage == null ? 0 : returnPage;
                if (returnPage != 0)
                {
                    HttpContext.Session.SetInt32("returnPage", (int)returnPage);
                }

                // 調用 SettingService 提供的函數將該公司添加到該用戶的所有設定檔，若中途出錯則跳轉到指定的錯誤頁面
                string ErrorMessage = SettingService.AddBlockCompany(_context, User.Identity.Name, CompanyName);
                if (ErrorMessage != null)
                {
                    ViewBag.Error = ErrorMessage;
                    return View("~/Views/Shared/ErrorPage.cshtml");
                }

                // 若中途沒有出錯則寫入變更到DB
                await _context.SaveChangesAsync();

                // 檢查 Session 儲存的工作列表，去除該公司所提供的工作
                JobList jobs = JsonConvert.DeserializeObject<JobList>(JobListStr);
                JobList validJobs = JobService.GetValidJobs(jobs, CompanyName);
                HttpContext.Session.SetString("jobList", JsonConvert.SerializeObject(validJobs));

                // 跳轉回之前所在的工作列表分頁(分頁判斷的邏輯在 Job/Index )
                return RedirectToRoute(new { controller = "Job", action = "Index" });
            }
            catch (Exception)
            {
                ViewBag.Error = "系統忙碌中，請稍後再試 >___<";
                return View("~/Views/Shared/ErrorPage.cshtml");
            }
        }
    }
}
