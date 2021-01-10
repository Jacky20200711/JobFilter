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
    public class FilterSettingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger _logger;

        public FilterSettingsController(ApplicationDbContext context, UserManager<IdentityUser> userManager, ILogger<UserController> logger)
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
                if (HttpContext.Session.GetString("CheckAllSettings") != null && AuthorizeManager.InAdminGroup(UserEmail))
                {
                    return View(await _context.FilterSetting.OrderByDescending(m => m.Id).ToPagedListAsync(page, 10));
                }
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

        public IActionResult SetSessionForCheckAllSettings()
        {
            if (!AuthorizeManager.InAdminGroup(User.Identity.Name)) return NotFound();
            HttpContext.Session.SetString("CheckAllSettings", "1");
            return RedirectToAction("Index", new { page = 1 });
        }

        public IActionResult RemoveSessionOfCheckAllSettings()
        {
            if (!AuthorizeManager.InAdminGroup(User.Identity.Name)) return NotFound();
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
        public IActionResult Create([Bind("Id,CrawlUrl,ExcludeWord,IgnoreCompany,MinimumWage,MaximumWage,Remarks")] FilterSetting filterSetting)
        {
            try
            {
                string ErrorMessage = FilterSettingManager.CreateSetting(_context, filterSetting, User.Identity.Name);
                if (ErrorMessage != null)
                {
                    ViewBag.Error = ErrorMessage;
                    return View("~/Views/Shared/ErrorPage.cshtml");
                }

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
            var filterSetting = await _context.FilterSetting.FindAsync(id);
            if (id == null || filterSetting == null)
            {
                return NotFound();
            }

            // 令管理員以外的用戶只能編輯自己的設定
            string UserEmail = User.Identity.Name;
            if (!AuthorizeManager.InAdminGroup(User.Identity.Name) && filterSetting.UserEmail != UserEmail) return NotFound();

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
        public IActionResult Edit(int id, [Bind("Id,CrawlUrl,ExcludeWord,IgnoreCompany,MinimumWage,MaximumWage,Remarks")] FilterSetting filterSetting)
        {
            try
            {
                string ErrorMessage = FilterSettingManager.EditSetting(_context, filterSetting, User.Identity.Name, id);
                if (ErrorMessage != null)
                {
                    ViewBag.Error = ErrorMessage;
                    return View("~/Views/Shared/ErrorPage.cshtml");
                }

                // 返回之前的分頁
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

        public IActionResult Delete(int? id, int? returnPage = 1)
        {
            try
            {
                string ErrorMessage = FilterSettingManager.DeleteSetting(_context, User.Identity.Name, id);
                if (ErrorMessage != null)
                {
                    ViewBag.Error = ErrorMessage;
                    return View("~/Views/Shared/ErrorPage.cshtml");
                }

                // 返回之前的分頁
                int page = returnPage != null ? (int)returnPage : 1;
                return RedirectToAction("Index", new { page });
            }
            catch (Exception)
            {
                ViewBag.Error = "系統忙碌中，請稍後再試 >___<";
                return View("~/Views/Shared/ErrorPage.cshtml");
            }
        }

        public async Task<IActionResult> DeleteAll()
        {
            if (User.Identity.Name != AuthorizeManager.SuperAdmin) return NotFound();
            _context.RemoveRange(_context.FilterSetting);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult AddBlockCompany(string CompanyName)
        {
            try
            {
                // 優先確認 session 儲存的工作列表不為空 
                string JobListStr = HttpContext.Session.GetString("jobList");
                if (JobListStr == null)
                {
                    ViewBag.Error = "系統忙碌中，請稍後再試 >___<";
                    return View("~/Views/Shared/ErrorPage.cshtml");
                }

                // 將封鎖的公司添加到該使用者的所有設定檔
                string ErrorMessage = FilterSettingManager.AddBlockCompany(_context, User.Identity.Name, CompanyName);
                if (ErrorMessage != null)
                {
                    ViewBag.Error = ErrorMessage;
                    return View("~/Views/Shared/ErrorPage.cshtml");
                }

                // 過濾 session 儲存的工作列表
                JobList jobList = JsonConvert.DeserializeObject<JobList>(JobListStr);
                jobList = JobFilterManager.GetValidJobList(jobList, CompanyName);
                HttpContext.Session.SetString("jobList", JsonConvert.SerializeObject(jobList));
                return RedirectToRoute(new { controller = "JobFilter", action = "Index" });
            }
            catch (Exception)
            {
                ViewBag.Error = "系統忙碌中，請稍後再試 >___<";
                return View("~/Views/Shared/ErrorPage.cshtml");
            }
        }
    }
}
