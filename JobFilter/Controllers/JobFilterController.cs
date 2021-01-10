using JobFilter.Data;
using JobFilter.Models.DataStructure;
using JobFilter.Models.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;
using X.PagedList;

namespace JobFilter.Controllers
{
    [Authorize]
    public class JobFilterController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        public JobFilterController(ApplicationDbContext context, ILogger<JobFilterController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int? page = 1)
        {
            try
            {
                page = page == null ? 1 : page;

                string JobListStr = HttpContext.Session.GetString("jobList");
                if (JobListStr == null)
                {
                    ViewBag.Error = "發生錯誤，可能是系統忙碌中，或是104的網站發生問題QQ";
                    return View("~/Views/Shared/ErrorPage.cshtml");
                }

                JobList jobList = JsonConvert.DeserializeObject<JobList>(JobListStr);
                HttpContext.Session.SetString("JobNum", jobList.Count.ToString());
                return View(await jobList.ToPagedListAsync((int)page, 10));
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.ToString());
                ViewBag.Error = "系統忙碌中，請稍後再試 >___<";
                return View("~/Views/Shared/ErrorPage.cshtml");
            }
        }

        public IActionResult DoCrawl(int? id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound();
                }

                var filterSetting = _context.FilterSetting.FirstOrDefault(x => x.Id == id);
                if (filterSetting == null)
                {
                    return NotFound();
                }

                // 令用戶只能執行自己的設定
                if (filterSetting.UserEmail != User.Identity.Name)
                {
                    return NotFound();
                }

                // 取得過濾後的工作
                JobList jobList = JobFilterManager.GetValidJobList(filterSetting);

                // 將過濾後的工作儲存到 Session
                HttpContext.Session.SetString("jobList", JsonConvert.SerializeObject(jobList));

                // 跳轉到顯示工作列表的頁面
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                ViewBag.Error = "系統忙碌中，請稍後再試 >___<";
                return View("~/Views/Shared/ErrorPage.cshtml");
            }
        }
    }
}
