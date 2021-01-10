using JobFilter.Models.DataStructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace JobFilter.Models.Services
{
    public static class JobFilterManager
    {
        public static HashSet<string> GetExcludeWordSet(string ExcludeWord)
        {
            HashSet<string> ExcludeWordSet = new HashSet<string>();

            if (string.IsNullOrEmpty(ExcludeWord))
            {
                return ExcludeWordSet;
            }

            foreach (string excludeWord in ExcludeWord.Split(','))
            {
                if (!string.IsNullOrEmpty(excludeWord))
                {
                    ExcludeWordSet.Add(excludeWord);
                }
            }

            return ExcludeWordSet;
        }

        public static HashSet<string> GetIgnoreCompanySet(string IgnoreCompany)
        {
            HashSet<string> IgnoreCompanySet = new HashSet<string>();

            if (string.IsNullOrEmpty(IgnoreCompany))
            {
                return IgnoreCompanySet;
            }

            foreach (string ignoreCompany in IgnoreCompany.Split(','))
            {
                if (!string.IsNullOrEmpty(ignoreCompany))
                {
                    IgnoreCompanySet.Add(ignoreCompany);
                }
            }

            return IgnoreCompanySet;
        }

        public static bool IsValidJob(FilterSetting filterSetting, Job job)
        {
            HashSet<string> ExcludeWordSet = GetExcludeWordSet(filterSetting.ExcludeWord);
            HashSet<string> IgnoreCompanySet = GetIgnoreCompanySet(filterSetting.IgnoreCompany);

            // 檢查這一份工作的薪資範圍和公司名稱
            if (job.MinimumWage < filterSetting.MinimumWage ||
                job.MaximumWage < filterSetting.MaximumWage ||
                IgnoreCompanySet.Contains(job.Company))
            {
                return false;
            }

            // 檢查這一份工作的 Title 是否包含欲排除的關鍵字
            foreach (string word in ExcludeWordSet)
            {
                // 比對時忽略大小寫
                if (job.Title.IndexOf(word, StringComparison.OrdinalIgnoreCase) > -1)
                {
                    return false;
                }
            }
            return true;
        }

        private static void GetTargetPages(List<JobCrawler> JobCrawlers)
        {
            // 令所有爬蟲開始爬取目標頁面
            foreach (JobCrawler jobCrawler in JobCrawlers)
            {
                jobCrawler.LoadPage();
            }

            // 等待所有的爬蟲爬取完畢
            while (JobCrawlers.Any(jobCrawler => !jobCrawler.IsCrawlFinished()))
            {
                Thread.Sleep(200);
            }
        }

        private static void GetJobSections(List<JobCrawler> JobCrawlers)
        {
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
        }

        private static JobList GetJobList(List<JobCrawler> JobCrawlers)
        {
            JobList jobList = new JobList();

            foreach (JobCrawler jobCrawler in JobCrawlers)
            {
                if (!jobCrawler.IsEncounterError())
                {
                    foreach (Job job in jobCrawler.GetJobs())
                    {
                        jobList.Add(job);
                    }
                }
            }

            return jobList;
        }

        public static JobList GetValidJobList(FilterSetting filterSetting)
        {
            // 取得多個爬蟲
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

            // 令爬蟲們抓取頁面 & 解析頁面並取得所有工作
            GetTargetPages(JobCrawlers);
            GetJobSections(JobCrawlers);
            JobList jobList = GetJobList(JobCrawlers);

            // 根據設定檔來過濾不想要的工作
            JobList validJobList = new JobList();
            foreach (Job job in jobList)
            {
                if (IsValidJob(filterSetting, job))
                {
                    validJobList.Add(job);
                }
            }

            return validJobList;
        }

        public static JobList GetValidJobList(JobList jobList, string blockCompany = null)
        {
            JobList validJobList = new JobList();
            foreach (Job job in jobList)
            {
                if (job.Company != blockCompany)
                {
                    validJobList.Add(job);
                }
            }

            return validJobList;
        }
    }
}
