using JobFilter.Data;
using JobFilter.Models.DataStructure;
using JobFilter.Models.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
            page = page == null ? 1 : page;

            string JobListStr = HttpContext.Session.GetString("jobList");
            if (JobListStr == null)
            {
                return View("Error");
            }

            JobList jobs = JsonConvert.DeserializeObject<JobList>(JobListStr);
            HttpContext.Session.SetString("JobNum", jobs.Count.ToString());
            return View(await jobs.ToPagedListAsync((int)page, 10));
        }

        public IActionResult DoCrawl(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }

            var filterSetting = _context.FilterSetting.FirstOrDefault(x => x.Id == id);
            if(filterSetting == null)
            {
                return NotFound();
            }

            // 令用戶只能執行自己的設定
            if(filterSetting.UserEmail != User.Identity.Name)
            {
                return NotFound();
            }

            JobList jobList;

            // 若這次爬的網址和上次一樣，則直接過濾之前儲存的 session 內容即可
            //string crawlUrlOfLastTime = HttpContext.Session.GetString("crawlUrlOfLastTime");
            //if (crawlUrlOfLastTime == filterSetting.CrawlUrl)
            //{
            //    // 確認之前儲存的 session 內容仍然存在
            //    string JobListStr = HttpContext.Session.GetString("jobList");
            //    if (JobListStr != null)
            //    {
            //        // 過濾之前儲存的 session 內容並更新
            //        JobList jobs = JsonConvert.DeserializeObject<JobList>(JobListStr);
            //        jobList = JobFilterManager.GetValidJobList(filterSetting);
            //        HttpContext.Session.SetString("jobList", JsonConvert.SerializeObject(jobList));
            //        return RedirectToAction("Index");
            //    }
            //}

            // 取得過濾後的工作
            jobList = JobFilterManager.GetValidJobList(filterSetting);

            // 將過濾後的工作儲存到 Session
            HttpContext.Session.SetString("jobList", JsonConvert.SerializeObject(jobList));

            // 將這次爬取的網址存到 Session
            HttpContext.Session.SetString("crawlUrlOfLastTime", filterSetting.CrawlUrl);

            return RedirectToAction("Index");
        }
    }
}
