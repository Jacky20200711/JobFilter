﻿using System;
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
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger _logger;

        public UserController(ApplicationDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, ILogger<UserController> logger)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index(int? page = 1)
        {
            page = page == null ? 1 : page;
            return View(await _context.Users.Where(m => m.Email != UserService.SuperAdmin).ToPagedListAsync(page, 10)); // 隱藏超級管理員
        }

        [Authorize(Roles = "Admin")]
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

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Email,PasswordHash")] IdentityUser identityUser)
        {
            // 調用 UserService 提供的函數來檢查郵件格式和密碼長度，若沒有通過檢查則顯示對應的錯誤訊息
            string ErrorMessage = UserService.IsValidUser(identityUser);
            if (ErrorMessage != null)
            {
                ViewData["CreateUserError"] = ErrorMessage;
                return View();
            }

            // 若成功通過檢查則建立該用戶
            // _userManager 會自動幫你檢查該郵件是否已被註冊，若已被註冊則不會進行動作
            var user = new IdentityUser { UserName = identityUser.Email, Email = identityUser.Email };
            await _userManager.CreateAsync(user, identityUser.PasswordHash);
            _logger.LogInformation($"[{User.Identity.Name}]新增了用戶[{user.Email}]");

            // 返回之前所在的用戶列表分頁
            int? TryGetPage = HttpContext.Session.GetInt32("returnPage");
            int page = TryGetPage != null ? (int)TryGetPage : 1;
            return RedirectToAction("Index", new { page });
        }

        [Authorize(Roles = "Admin")]
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

            // 返回之前所在的用戶列表分頁
            int? TryGetPage = HttpContext.Session.GetInt32("returnPage");
            int page = TryGetPage != null ? (int)TryGetPage : 1;
            return RedirectToAction("Index", new { page });
        }

        [Authorize(Roles = "Admin")]
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

        [Authorize(Roles = "Admin")]
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

            // 返回之前所在的用戶列表分頁
            int? TryGetPage = HttpContext.Session.GetInt32("returnPage");
            int page = TryGetPage != null ? (int)TryGetPage : 1;
            return RedirectToAction("Index", new { page });
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAll()
        {
            // 刪除超級管理員以外的用戶
            _context.RemoveRange(_context.Users.Where(m => m.Email != UserService.SuperAdmin));
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> CreateAdminRole()
        {
            // 只有超級管理員可以創建管理員群組
            if(User.Identity.Name != UserService.SuperAdmin)
            {
                return NotFound();
            }

            try
            {
                // 創建管理員群組
                var roleCheck = await _roleManager.RoleExistsAsync("Admin");
                if (!roleCheck)
                {
                    await _roleManager.CreateAsync(new IdentityRole("Admin"));
                }

                // 將超級管理員加入該群組
                IdentityUser user = await _userManager.FindByEmailAsync(UserService.SuperAdmin);
                await _userManager.AddToRoleAsync(user, "Admin");
                return RedirectToAction(nameof(Index));
            }
            catch(Exception ex)
            {
                _logger.LogError($"CreateAdminRole error = {ex}");
                return RedirectToAction(nameof(Index));
            }
        }
    }
}