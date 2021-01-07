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
            if (SessionValue == null)
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

            // 創建多個爬蟲 & 設置欲爬取的目標網址
            string TargetUrl = filterSetting.CrawlUrl;
            char ConnectionChar = TargetUrl.Last() == '/' ? '?' : '&';
            List<JobCrawler> JobCrawlers = new List<JobCrawler>
            {
                new JobCrawler($"{TargetUrl}{ConnectionChar}page=1"),
                new JobCrawler($"{TargetUrl}{ConnectionChar}page=2"),
                new JobCrawler($"{TargetUrl}{ConnectionChar}page=3"),
                new JobCrawler($"{TargetUrl}{ConnectionChar}page=4"),
                new JobCrawler($"{TargetUrl}{ConnectionChar}page=5"),
                new JobCrawler($"{TargetUrl}{ConnectionChar}page=6"),
                new JobCrawler($"{TargetUrl}{ConnectionChar}page=7"),
                new JobCrawler($"{TargetUrl}{ConnectionChar}page=8"),
            };
            
            // 令所有爬蟲開始爬取目標頁面
            foreach(JobCrawler jobCrawler in JobCrawlers)
            {
                jobCrawler.LoadPage();
            }

            // 等待所有的爬蟲爬取完畢
            while (JobCrawlers.Any(jobCrawler => !jobCrawler.IsCrawlFinished()))
            {
                Thread.Sleep(200);
            }

            // 令成功取得頁面的爬蟲開始萃取包含工作的標籤區塊
            foreach (JobCrawler jobCrawler in JobCrawlers)
            {
                if (!jobCrawler.IsEncounterError())
                {
                    jobCrawler.ExtractTags();
                }
            }

            // 等待所有的爬蟲萃取完畢
            while (JobCrawlers.Any(jobCrawler => !jobCrawler.IsEncounterError() && !jobCrawler.IsExtractFinished()))
            {
                Thread.Sleep(200);
            }

            // 令萃取成功的爬蟲再進一步解析各區塊的工作說明
            foreach (JobCrawler jobCrawler in JobCrawlers)
            {
                if (!jobCrawler.IsEncounterError())
                {
                    jobCrawler.ExtractJobData();
                }
            }

            // 過濾掉不符合條件的工作
            JobList jobList = new JobList();
            foreach (JobCrawler jobCrawler in JobCrawlers)
            {
                if (!jobCrawler.IsEncounterError())
                {
                    foreach (Job job in jobCrawler.GetJobs())
                    {
                        if (JobFilterManager.IsValidJob(filterSetting, job))
                        {
                            jobList.Add(job);
                        }
                    }
                }
            }

            // 將過濾後的工作儲存到 Session
            HttpContext.Session.SetString("jobList", JsonConvert.SerializeObject(jobList));

            return RedirectToAction("Index");
        }
    }
}
