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
    public class JobController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;

        public JobController(ApplicationDbContext context, ILogger<JobController> logger)
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
                    ViewBag.Error = "系統忙碌中，請稍後再試 >___<";
                    return View("~/Views/Shared/ErrorPage.cshtml");
                }

                JobList jobList = JsonConvert.DeserializeObject<JobList>(JobListStr);
                ViewBag.numOfJob = jobList.Count;
                return View(await jobList.ToPagedListAsync((int)page, 10));
            }
            catch(Exception)
            {
                ViewBag.Error = "系統忙碌中，請稍後再試 >___<";
                return View("~/Views/Shared/ErrorPage.cshtml");
            }
        }

        public IActionResult GetValidJobs(int? id)
        {
            try
            {
                // 令用戶只能執行自己的設定
                var filterSetting = _context.FilterSetting.FirstOrDefault(x => x.Id == id);
                if (filterSetting == null || filterSetting.UserEmail != User.Identity.Name)
                {
                    ViewBag.Error = "系統忙碌中，請稍後再試 >___<";
                    return View("~/Views/Shared/ErrorPage.cshtml");
                }

                // 取得過濾後的工作並存到Session
                JobList validJobs = JobService.GetValidJobs(filterSetting);
                HttpContext.Session.SetString("jobList", JsonConvert.SerializeObject(validJobs));
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                ViewBag.Error = "系統忙碌中，請稍後再試 >___<";
                return View("~/Views/Shared/ErrorPage.cshtml");
            }
        }
    }
}
