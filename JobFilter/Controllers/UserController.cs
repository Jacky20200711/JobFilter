using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using JobFilter.Data;
using JobFilter.Models;
using JobFilter.Models.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using X.PagedList;

namespace JobFilter.Controllers
{
    public class UserController : Controller
    {
        // 每個分頁最多顯示10筆
        private readonly int pageSize = 10;

        // 注入會用到的工具
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger _logger;

        public UserController(ApplicationDbContext usertext, UserManager<IdentityUser> userManager, ILogger<UserController> logger)
        {
            _context = usertext;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            if (!AuthorizeManager.InAdminGroup(User.Identity.Name)) return NotFound();

            // 隱藏超級管理員
            return View(await _context.Users.Where(m => m.Email != AuthorizeManager.SuperAdmin).ToPagedListAsync(page, pageSize));
        }

        public IActionResult Create(int returnPage = 0)
        {
            if (!AuthorizeManager.InAdminGroup(User.Identity.Name)) return NotFound();

            if (returnPage != 0)
            {
                HttpContext.Session.SetInt32("returnPage", returnPage);
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Email,PasswordHash")] IdentityUser identityUser)
        {
            if (!AuthorizeManager.InAdminGroup(User.Identity.Name)) return NotFound();

            // 檢查郵件格式和密碼長度
            string ErrorMessage = IdentityUserManager.IsValidUser(identityUser);
            if (ErrorMessage != null)
            {
                ViewData["CreateUserError"] = ErrorMessage;
                return View();
            }

            // _userManager 會自動幫你檢查該郵件是否已被註冊，若是...則不會進行動作
            var user = new IdentityUser { UserName = identityUser.Email, Email = identityUser.Email };
            await _userManager.CreateAsync(user, identityUser.PasswordHash);
            _logger.LogInformation($"[{User.Identity.Name}]新增了用戶[{user.Email}]");

            // 返回之前的分頁
            int? TryGetPage = HttpContext.Session.GetInt32("returnPage");
            int page = TryGetPage != null ? (int)TryGetPage : 1;
            return RedirectToAction("Index", new { page });
        }

        public ActionResult Delete(string id, int returnPage = 0)
        {
            if (!AuthorizeManager.InAdminGroup(User.Identity.Name)) return NotFound();

            if (returnPage != 0)
            {
                HttpContext.Session.SetInt32("returnPage", returnPage);
            }

            var user = _context.Users.FirstOrDefault(u => u.Id == id);

            // 令超級管理員不能被刪除
            if (user.Email == AuthorizeManager.SuperAdmin)
            {
                return NotFound();
            }

            // 刪除該使用者
            _context.Users.Remove(user);
            _context.SaveChanges();
            _logger.LogWarning($"[{User.Identity.Name}]刪除了用戶[{user.Email}]");

            // 返回之前的分頁
            int? TryGetPage = HttpContext.Session.GetInt32("returnPage");
            int page = TryGetPage != null ? (int)TryGetPage : 1;
            return RedirectToAction("Index", new { page });
        }

        public ActionResult Edit(string id, int returnPage = 0)
        {
            if (!AuthorizeManager.InAdminGroup(User.Identity.Name)) return NotFound();

            if (returnPage != 0)
            {
                HttpContext.Session.SetInt32("returnPage", returnPage);
            }

            var user = _context.Users.FirstOrDefault(u => u.Id == id);

            // 令超級管理員不能被編輯
            if (user.Email == AuthorizeManager.SuperAdmin)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(IdentityUser identityUser)
        {
            if (!AuthorizeManager.InAdminGroup(User.Identity.Name)) return NotFound();

            var user = _context.Users.FirstOrDefault(u => u.Id == identityUser.Id);

            // 令超級管理員不能被編輯
            if (user.Email == AuthorizeManager.SuperAdmin)
            {
                return NotFound();
            }

            // 修改會員郵件
            user.Email = identityUser.Email;
            user.UserName = identityUser.Email;

            // 修改會員密碼(若沒先 RemovePassword 則 LOG 會出現 Warning)
            await _userManager.RemovePasswordAsync(user);
            await _userManager.AddPasswordAsync(user, identityUser.PasswordHash);
            _logger.LogInformation($"[{User.Identity.Name}]修改了[{user.Email}]的資料");

            // 返回之前的分頁
            int? TryGetPage = HttpContext.Session.GetInt32("returnPage");
            int page = TryGetPage != null ? (int)TryGetPage : 1;
            return RedirectToAction("Index", new { page });
        }

        public async Task<IActionResult> DeleteAll()
        {
            if (User.Identity.Name != AuthorizeManager.SuperAdmin) return NotFound();
            _context.RemoveRange(_context.Users.Where(m => m.Email != AuthorizeManager.SuperAdmin));
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}