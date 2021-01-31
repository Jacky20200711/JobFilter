using System.Linq;
using System.Threading.Tasks;
using JobFilter.Data;
using JobFilter.Models.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using X.PagedList;

namespace JobFilter.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger _logger;

        public UserController(ApplicationDbContext context, UserManager<IdentityUser> userManager, ILogger<UserController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int? page = 1)
        {
            page = page == null ? 1 : page;
            return View(await _context.Users.Where(m => m.Email != UserService.SuperAdmin).ToPagedListAsync(page, 10)); // 隱藏超級管理員
        }

        public IActionResult Create(int? returnPage = 0)
        {
            // 紀錄之前所在的分頁
            returnPage = returnPage == null ? 0 : returnPage;
            if (returnPage != 0)
            {
                HttpContext.Session.SetInt32("returnPage", (int)returnPage);
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Email,PasswordHash")] IdentityUser identityUser)
        {
            // 檢查郵件格式和密碼長度
            string ErrorMessage = UserService.IsValidUser(identityUser);
            if (ErrorMessage != null)
            {
                ViewData["CreateUserError"] = ErrorMessage;
                return View();
            }

            // _userManager 會自動幫你檢查該郵件是否已被註冊，若已被註冊則不會進行動作
            var user = new IdentityUser { UserName = identityUser.Email, Email = identityUser.Email };
            await _userManager.CreateAsync(user, identityUser.PasswordHash);
            _logger.LogInformation($"[{User.Identity.Name}]新增了用戶[{user.Email}]");

            // 返回之前的分頁
            int? TryGetPage = HttpContext.Session.GetInt32("returnPage");
            int page = TryGetPage != null ? (int)TryGetPage : 1;
            return RedirectToAction("Index", new { page });
        }

        public ActionResult Delete(string id, int? returnPage = 0)
        {
            // 紀錄之前所在的分頁
            returnPage = returnPage == null ? 0 : returnPage;
            if (returnPage != 0)
            {
                HttpContext.Session.SetInt32("returnPage", (int)returnPage);
            }

            // 檢查id是否有效，並且令超級管理員不能被刪除
            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null || user.Email == UserService.SuperAdmin)
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

        public ActionResult Edit(string id, int? returnPage = 0)
        {
            // 紀錄之前所在的分頁
            returnPage = returnPage == null ? 0 : returnPage;
            if (returnPage != 0)
            {
                HttpContext.Session.SetInt32("returnPage", (int)returnPage);
            }

            // 檢查id是否有效
            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            // 儲存欲修改的用戶郵件
            HttpContext.Session.SetString("UserEmail", user.Email);
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(IdentityUser identityUser)
        {
            // 令超級管理員不能被編輯
            string UserEmail = HttpContext.Session.GetString("UserEmail");
            if (UserEmail == UserService.SuperAdmin)
            {
                return NotFound();
            }

            // 修改會員郵件
            var user = _context.Users.FirstOrDefault(u => u.Email == UserEmail);
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
            _context.RemoveRange(_context.Users.Where(m => m.Email != UserService.SuperAdmin));
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}