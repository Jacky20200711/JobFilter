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

            if (!AuthorizeManager.InAdminGroup(User.Identity.Name))
            {
                string UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                return View(await _context.FilterSetting.Where(m => m.UserId == UserId).OrderByDescending(m => m.Id).ToPagedListAsync(page, 5));
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
                    string UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                    // 查看自己的設定
                    return View(await _context.FilterSetting.Where(m => m.UserId == UserId).OrderByDescending(m => m.Id).ToPagedListAsync(page, 5));
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
        public async Task<IActionResult> Create([Bind("Id,CrawlUrl,ExcludeWord,IgnoreCompany,MinimumWage,MaximumWage")] FilterSetting filterSetting)
        {
            if (ModelState.IsValid)
            {
                string UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // 查看設定檔的數量是否已達上限
                List<FilterSetting> filterSettings = _context.FilterSetting.Where(m => m.UserId == UserId).ToList();

                if(filterSettings.Count > 9)
                {
                    TempData["CreateSettingError"] = "建立失敗，您的設定數量已達上限!";
                    return RedirectToAction(nameof(Index));
                }

                // 在後端進行表單驗證
                if (!FilterSettingManager.IsValidSetting(filterSetting)) 
                    return Content("表單資料錯誤，請檢查輸入的內容!");

                filterSetting.UserEmail = User.Identity.Name;
                filterSetting.UserId = UserId;
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
            string UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!AuthorizeManager.InAdminGroup(User.Identity.Name) && filterSetting.UserId != UserId) return NotFound();

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
        public async Task<IActionResult> Edit(int id, [Bind("Id,CrawlUrl,ExcludeWord,IgnoreCompany,MinimumWage,MaximumWage")] FilterSetting filterSetting)
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
                    string UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    FilterSetting Setting = _context.FilterSetting.FirstOrDefault(m => m.Id == id);
                    if (!AuthorizeManager.InAdminGroup(User.Identity.Name) && Setting.UserId != UserId)
                    {
                        return NotFound();
                    }

                    // 更新設定
                    Setting.CrawlUrl = filterSetting.CrawlUrl;
                    Setting.MinimumWage = filterSetting.MinimumWage;
                    Setting.MaximumWage = filterSetting.MaximumWage;
                    Setting.ExcludeWord = filterSetting.ExcludeWord;
                    Setting.IgnoreCompany = filterSetting.IgnoreCompany;
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
            string UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!AuthorizeManager.InAdminGroup(User.Identity.Name) && filterSetting.UserId != UserId) return NotFound();

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
    }
}
