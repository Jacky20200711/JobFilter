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

            string SessionValue = HttpContext.Session.GetString("jobList");
            if (SessionValue == null || SessionValue == "Error")
            {
                return View("Error");
            }

            var jobs = JsonConvert.DeserializeObject<JobList>(SessionValue);
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

            // 創建多個爬蟲 & 在目標網址尾端添加頁數的參數
            string TargetUrl = filterSetting.CrawlUrl;
            char ConnectionChar = TargetUrl.Last() == '/' ? '?' : '&';
            List<JobCrawler> JobCrawlers = new List<JobCrawler>
            {
                new JobCrawler($"{TargetUrl}{ConnectionChar}page=1"),
                new JobCrawler($"{TargetUrl}{ConnectionChar}page=2"),
                new JobCrawler($"{TargetUrl}{ConnectionChar}page=3"),
                new JobCrawler($"{TargetUrl}{ConnectionChar}page=4"),
                new JobCrawler($"{TargetUrl}{ConnectionChar}page=5"),
            };

            // 創建多個爬蟲愛好者，每個人認養一隻爬蟲
            List<JobCrawlerOwner> JobCrawlerOwners = new List<JobCrawlerOwner>();
            foreach(var jobCrawler in JobCrawlers)
            {
                JobCrawlerOwners.Add(new JobCrawlerOwner(jobCrawler));
            }

            // 指派任務給每一隻爬蟲，並令爬蟲們就位XD
            List<Thread> JobCrawlerThreads = new List<Thread>();
            foreach (var JobCrawlerOwner in JobCrawlerOwners)
            {
                JobCrawlerThreads.Add(new Thread(JobCrawlerOwner.PutCrawlerInPlace));
            }
            
            // 令所有爬蟲開始執行任務
            foreach(Thread thread in JobCrawlerThreads)
            {
                thread.Start();
            }

            // 等待爬蟲們完成並回報
            while (!JobCrawlers.All(jobCrawler => jobCrawler.IsMissionComplete() == true))
            {
                Thread.Sleep(300);
            }

            // 詢問爬蟲們執行任務的過程是否順利
            foreach (JobCrawler jobCrawler in JobCrawlers)
            {
                if (jobCrawler.IsEncounterError())
                {
                    HttpContext.Session.SetString("jobList", "Error");
                    return RedirectToAction("Index");
                }
            }

            // 過濾掉不符合條件的工作
            JobList jobList = new JobList();
            foreach (JobCrawler jobCrawler in JobCrawlers)
            {
                foreach (Job job in jobCrawler.GetJobs())
                {
                    if (JobFilterManager.IsValidJob(filterSetting, job))
                    {
                        jobList.Add(job);
                    }
                }
            }

            // 將過濾後的工作儲存到 Session
            HttpContext.Session.SetString("jobList", JsonConvert.SerializeObject(jobList));

            return RedirectToAction("Index");
        }
    }
}
