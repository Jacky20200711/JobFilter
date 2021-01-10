using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JobFilter.Data;
using JobFilter.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using X.PagedList;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using JobFilter.Models.DataStructure;
using JobFilter.Models.Services;

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
            page = page == null ? 1 : page;

            string UserEmail = User.Identity.Name;

            if (!AuthorizeManager.InAdminGroup(UserEmail))
            {
                return View(await _context.FilterSetting.Where(m => m.UserEmail == UserEmail).OrderByDescending(m => m.Id).ToPagedListAsync(page, 5));
            }
            else
            {
                if(HttpContext.Session.GetString("CheckAllSettings") != null)
                {
                    // 查看所有的設定
                    return View(await _context.FilterSetting.OrderByDescending(m => m.Id).ToPagedListAsync(page, 10));
                }
                else
                {
                    // 查看自己的設定
                    return View(await _context.FilterSetting.Where(m => m.UserEmail == UserEmail).OrderByDescending(m => m.Id).ToPagedListAsync(page, 5));
                }
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

        public async Task<IActionResult> Details(int? id)
        {
            if (!AuthorizeManager.InAdminGroup(User.Identity.Name)) return NotFound();

            if (id == null)
            {
                return NotFound();
            }

            var filterSetting = await _context.FilterSetting
                .FirstOrDefaultAsync(m => m.Id == id);
            if (filterSetting == null)
            {
                return NotFound();
            }

            return View(filterSetting);
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
            if (ModelState.IsValid)
            {
                string UserEmail = User.Identity.Name;

                // 查看設定檔的數量是否已達上限
                List<FilterSetting> filterSettings = _context.FilterSetting.Where(m => m.UserEmail == UserEmail).ToList();

                if(filterSettings.Count > 2)
                {
                    TempData["CreateSettingError"] = "建立失敗，您的設定數量已達上限!";
                    return RedirectToAction(nameof(Index));
                }

                // 在後端進行表單驗證
                if (!FilterSettingManager.IsValidSetting(filterSetting))
                {
                    return Content("表單資料錯誤，請檢查輸入的內容!");
                }

                // 若通過驗證則儲存表單
                filterSetting.UserEmail = UserEmail;
                filterSetting.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                _context.Add(filterSetting);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(filterSetting);
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
        public async Task<IActionResult> Edit(int id, [Bind("Id,CrawlUrl,ExcludeWord,IgnoreCompany,MinimumWage,MaximumWage,Remarks")] FilterSetting filterSetting)
        {
            if (id != filterSetting.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // 在後端進行表單驗證
                    if (!FilterSettingManager.IsValidSetting(filterSetting))
                    {
                        return Content("表單資料錯誤，請檢查輸入的內容!");
                    }

                    // 令管理員以外的用戶只能編輯自己的設定
                    string UserEmail = User.Identity.Name;
                    FilterSetting Setting = _context.FilterSetting.FirstOrDefault(m => m.Id == id);
                    if (!AuthorizeManager.InAdminGroup(User.Identity.Name) && Setting.UserEmail != UserEmail)
                    {
                        return NotFound();
                    }

                    // 更新設定
                    Setting.CrawlUrl = filterSetting.CrawlUrl;
                    Setting.MinimumWage = filterSetting.MinimumWage;
                    Setting.MaximumWage = filterSetting.MaximumWage;
                    Setting.ExcludeWord = filterSetting.ExcludeWord;
                    Setting.IgnoreCompany = filterSetting.IgnoreCompany;
                    Setting.Remarks = filterSetting.Remarks;
                    await _context.SaveChangesAsync();

                    // 返回之前的分頁
                    int? TryGetPage = HttpContext.Session.GetInt32("returnPage");
                    int page = TryGetPage != null ? (int)TryGetPage : 1;
                    return RedirectToAction("Index", new { page });
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _logger.LogError(ex.ToString());
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(filterSetting);
        }

        public async Task<IActionResult> Delete(int? id, int? returnPage = 0)
        {
            if (id == null)
            {
                return NotFound();
            }

            var filterSetting = await _context.FilterSetting
                .FirstOrDefaultAsync(m => m.Id == id);
            if (filterSetting == null)
            {
                return NotFound();
            }

            // 令管理員以外的用戶只能刪除自己的設定
            string UserEmail = User.Identity.Name;
            if (!AuthorizeManager.InAdminGroup(User.Identity.Name) && filterSetting.UserEmail != UserEmail) return NotFound();

            _context.FilterSetting.Remove(filterSetting);
            await _context.SaveChangesAsync();

            // 紀錄之前所在的分頁號碼
            returnPage = returnPage == null ? 0 : returnPage;
            if (returnPage != 0)
            {
                HttpContext.Session.SetInt32("returnPage", (int)returnPage);
            }

            // 返回之前的分頁
            int? TryGetPage = HttpContext.Session.GetInt32("returnPage");
            int page = TryGetPage != null ? (int)TryGetPage : 1;
            return RedirectToAction("Index", new { page });
        }

        private bool FilterSettingExists(int id)
        {
            return _context.FilterSetting.Any(e => e.Id == id);
        }

        public async Task<IActionResult> DeleteAll()
        {
            if (User.Identity.Name != AuthorizeManager.SuperAdmin) return NotFound();

            _context.RemoveRange(_context.FilterSetting);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> AddBlockCompany(string CompanyName)
        {
            // 檢查該公司的名稱與長度
            if (!FilterSettingManager.IsValidString(CompanyName, 50))
            {
                ViewBag.Error = "封鎖失敗，此公司的名稱含有不支援的字元或是字數超過限制(50字)!";
                return View("~/Views/Shared/ErrorPage.cshtml");
            }

            string UserEmail = User.Identity.Name;

            var UserSettings = _context.FilterSetting.Where(m => m.UserEmail == UserEmail);
            if(UserSettings == null)
            {
                return NotFound();
            }

            // 嘗試將新封鎖的公司添加到該 User 的所有設定檔
            foreach (var UserSetting in UserSettings)
            {
                // 檢查設定檔的欄位是否為 NULL
                if (string.IsNullOrEmpty(UserSetting.IgnoreCompany))
                {
                    // 賦值給原本為 NULL 的欄位
                    UserSetting.IgnoreCompany = $"{CompanyName}";
                }
                else
                {
                    // 檢查該欄位的新長度是否保持合法
                    if (UserSetting.IgnoreCompany.Length + $",{CompanyName}".Length > FilterSettingManager.Length_limit_IgnoreCompany)
                    {
                        ViewBag.Error = "封鎖未完全，請確認封鎖此公司後字數沒有超過上限(1000字)!";
                        return View("~/Views/Shared/ErrorPage.cshtml");
                    }

                    // 若長度合法則進行串接
                    UserSetting.IgnoreCompany += $",{CompanyName}";
                }
            }

            // 儲存變更
            await _context.SaveChangesAsync();

            // 檢查之前儲存的工作內容是否包含這家公司
            string JobListStr = HttpContext.Session.GetString("jobList");
            if (JobListStr == null)
            {
                ViewBag.Error = "發生錯誤，可能是系統忙碌中，或是104的網站發生問題QQ";
                return View("~/Views/Shared/ErrorPage.cshtml");
            }

            JobList jobList = JsonConvert.DeserializeObject<JobList>(JobListStr);
            jobList = JobFilterManager.GetValidJobList(jobList, CompanyName);
            HttpContext.Session.SetString("jobList", JsonConvert.SerializeObject(jobList));

            // 刷新呈現的工作列表
            return RedirectToRoute(new { controller = "JobFilter", action = "Index" });
        }
    }
}
