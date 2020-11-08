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

            if(HttpContext.Session.GetString("Jobs") == null)
            {
                return RedirectToRoute(new { controller = "Home", action = "Index" });
            }

            var jobs = JsonConvert.DeserializeObject<Jobs>(HttpContext.Session.GetString("Jobs"));
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

            // 令每個爬蟲各自對應到一個 Thread (令爬蟲就位XD)
            List<JobFilterThread> JobFilterThreads = new List<JobFilterThread>();
            foreach(var jobCrawler in JobCrawlers)
            {
                JobFilterThreads.Add(new JobFilterThread(jobCrawler));
            }

            // 指定 Thread 要執行的 Function
            List<Thread> Threads = new List<Thread>();
            foreach (var jobFilterThread in JobFilterThreads)
            {
                Threads.Add(new Thread(jobFilterThread.DoFilter));
            }
            
            // 執行所有的 Thread
            foreach(Thread thread in Threads)
            {
                thread.Start();
            }

            // 等待爬蟲們將各自頁面的工作資訊萃取完畢
            while (!JobCrawlers.All(jobCrawler => jobCrawler.IsMissionComplete() == true))
            {
                Thread.Sleep(300);
            }

            // 過濾掉不符合條件的工作
            Jobs jobs = new Jobs();
            foreach (JobCrawler jobCrawler in JobCrawlers)
            {
                foreach (Job job in jobCrawler.GetJobs())
                {
                    if (JobFilterManager.IsValidJob(filterSetting, job))
                    {
                        jobs.Add(job);
                    }
                }
            }

            // 將過濾後的工作儲存到 Session
            HttpContext.Session.SetString("Jobs", JsonConvert.SerializeObject(jobs));

            return RedirectToAction("Index");
        }
    }
}
