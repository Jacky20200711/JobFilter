using JobFilter.Data;
using JobFilter.Models.DataStructure;
using JobFilter.Models.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using X.PagedList;

namespace JobFilter.Controllers
{
    public class JobFilterController : Controller
    {
        private readonly ApplicationDbContext _context;

        public JobFilterController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? page = 1)
        {
            page = page == null ? 1 : page;

            return View(await JsonConvert.DeserializeObject<Jobs>(HttpContext.Session.GetString("Jobs")).ToPagedListAsync((int)page, 10));
        }

        public IActionResult DoCrawl(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }

            // 取得設定檔的目標網址
            var filterSetting = _context.FilterSetting.FirstOrDefault(x => x.Id == id);
            if(filterSetting == null)
            {
                return NotFound();
            }
            string TargetUrl = filterSetting.CrawlUrl;

            // 創建多個爬蟲
            List<JobCrawler> JobCrawlers = new List<JobCrawler>
            {
                new JobCrawler($"{TargetUrl}&page=1"),
                new JobCrawler($"{TargetUrl}&page=2"),
            };

            // 創建爬蟲容器 & 各自對應到一個 Thread (令爬蟲就位XD)
            JobFilterThread JobFilterThread0 = new JobFilterThread(JobCrawlers[0]);
            JobFilterThread JobFilterThread1 = new JobFilterThread(JobCrawlers[1]);

            List<Thread> JobFilterThreads = new List<Thread>
            {
                new Thread(JobFilterThread0.DoFilter),
                new Thread(JobFilterThread1.DoFilter),
            };

            // 執行所有的 Thread
            foreach(Thread thread in JobFilterThreads)
            {
                thread.Start();
            }

            // 等待所有 Thread 完成任務
            while (!JobCrawlers.All(jobCrawler => jobCrawler.IsMissionComplete() == true)) ;

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
