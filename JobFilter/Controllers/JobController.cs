﻿using JobFilter.Data;
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
                // 檢查 Session 儲存的工作列表是否為空
                page = page == null ? 1 : page;
                string JobListStr = HttpContext.Session.GetString("jobList");
                if (JobListStr == null)
                {
                    ViewBag.Error = "系統忙碌中，請稍後再試 >___<";
                    return View("~/Views/Shared/ErrorPage.cshtml");
                }

                // 取得 Session 儲存的工作列表和總工作數量
                JobList jobList = JsonConvert.DeserializeObject<JobList>(JobListStr);
                ViewBag.numOfJob = jobList.Count;

                // 查看 Session 是否有儲存之前所在的工作列表分頁，若有則將之後要跳轉的分頁設成該分頁
                int? TryGetPage = HttpContext.Session.GetInt32("returnPage");
                if(TryGetPage != null)
                {
                    page = TryGetPage;
                    HttpContext.Session.Remove("returnPage"); // 移除 Session 以避免永遠卡在此分頁
                }

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
                // 若用戶欲執行的設定檔不是自己的，則跳轉到指定的錯誤頁面
                var filterSetting = _context.FilterSetting.FirstOrDefault(x => x.Id == id);
                if (filterSetting == null || filterSetting.UserEmail != User.Identity.Name)
                {
                    ViewBag.Error = "系統忙碌中，請稍後再試 >___<";
                    return View("~/Views/Shared/ErrorPage.cshtml");
                }

                // 取得過濾後的工作列表並存到 Session
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
