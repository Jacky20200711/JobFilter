using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using JobFilter.Data;
using JobFilter.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace JobFilter.Controllers
{
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

        public async Task<IActionResult> Index()
        {
            if (string.IsNullOrEmpty(User.Identity.Name)) return NotFound();

            string UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!AuthorizeManager.InAdminGroup(User.Identity.Name))
            {
                return View(await _context.FilterSetting.Where(m => m.UserId == UserId).ToListAsync());
            }
            else
            {
                return View(await _context.FilterSetting.OrderBy(m => m.UserId).ToListAsync());
            }
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

        public IActionResult Create()
        {
            if (string.IsNullOrEmpty(User.Identity.Name)) return NotFound();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CrawlUrl,ExcludeWord,IgnoreCompany,MinimumWage")] FilterSetting filterSetting)
        {
            if (string.IsNullOrEmpty(User.Identity.Name)) return NotFound();

            if (ModelState.IsValid)
            {
                filterSetting.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                _context.Add(filterSetting);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(filterSetting);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (string.IsNullOrEmpty(User.Identity.Name)) return NotFound();

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

            return View(filterSetting);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CrawlUrl,ExcludeWord,IgnoreCompany,MinimumWage")] FilterSetting filterSetting)
        {
            if (string.IsNullOrEmpty(User.Identity.Name)) return NotFound();

            if (id != filterSetting.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // 令管理員以外的用戶只能編輯自己的設定
                    string UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    FilterSetting Setting = _context.FilterSetting.FirstOrDefault(m => m.Id == id);
                    if (!AuthorizeManager.InAdminGroup(User.Identity.Name) && Setting.UserId != UserId)  return NotFound();

                    // 更新設定
                    Setting.CrawlUrl = filterSetting.CrawlUrl;
                    Setting.MinimumWage = filterSetting.MinimumWage;
                    Setting.ExcludeWord = filterSetting.ExcludeWord;
                    Setting.IgnoreCompany = filterSetting.IgnoreCompany;
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _logger.LogError(ex.ToString());
                }
                return RedirectToAction(nameof(Index));
            }
            return View(filterSetting);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (string.IsNullOrEmpty(User.Identity.Name)) return NotFound();

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
            return RedirectToAction(nameof(Index));
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
