using JobFilter.Data;
using JobFilter.Models.DataStructure;
using JobFilter.Models.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using X.PagedList;

namespace JobFilter.Controllers
{
    public class JobFilterController : Controller
    {
        private readonly List<Job> Jobs = new List<Job>();
        private readonly ApplicationDbContext _context;

        public JobFilterController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? page = 1)
        {
            page = page == null ? 1 : page;

            return View(await Jobs.ToPagedListAsync((int)page, 10));
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

            // 創建三個爬蟲(一次爬三頁)
            JobCrawler JobCrawler1 = new JobCrawler($"{TargetUrl}&page=1");
            JobCrawler JobCrawler2 = new JobCrawler($"{TargetUrl}&page=2");
            JobCrawler JobCrawler3 = new JobCrawler($"{TargetUrl}&page=3");

            // 創建三個爬蟲容器(只爬三頁)
            JobFilterThread JobFilterThread1 = new JobFilterThread(JobCrawler1);
            JobFilterThread JobFilterThread2 = new JobFilterThread(JobCrawler2);
            JobFilterThread JobFilterThread3 = new JobFilterThread(JobCrawler3);

            // 令每個容器各自對應到一個 Thread (令爬蟲就位XD)
            Thread thread1 = new Thread(JobFilterThread1.DoFilter);
            Thread thread2 = new Thread(JobFilterThread2.DoFilter);
            Thread thread3 = new Thread(JobFilterThread3.DoFilter);

            // 令爬蟲同時執行(DoFilter 中有等待機制，所以不需要額外進行等待)
            thread1.Start();
            thread2.Start();
            thread3.Start();

            // 裝載新的工作內容(此步驟是為了美化之後的語法)
            List<List<Job>> JobsContainer = new List<List<Job>>
            {
                JobCrawler1.GetJobs(),
                JobCrawler2.GetJobs(),
                JobCrawler3.GetJobs(),
            };

            // 清空舊的爬取內容
            Jobs.Clear();

            // 根據設定檔的內容進行過濾
            foreach(var jobs in JobsContainer)
            {
                JobFilterManager.GetFilterJobs(
                    Jobs,
                    jobs,
                    filterSetting.MinimumWage,
                    filterSetting.ExcludeWord,
                    filterSetting.IgnoreCompany
                );
            }

            return RedirectToAction("Index");
        }
    }
}
