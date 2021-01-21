using JobFilter.Models.DataStructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace JobFilter.Models.Services
{
    public static class JobService
    {
        public static HashSet<string> GetSplitedDataSet(string DataStr)
        {
            HashSet<string> DataSet = new HashSet<string>();

            if (string.IsNullOrEmpty(DataStr))
            {
                return DataSet;
            }

            foreach (string data in DataStr.Split(','))
            {
                if (!string.IsNullOrEmpty(data))
                {
                    DataSet.Add(data);
                }
            }

            return DataSet;
        }

        public static bool IsValidJob(FilterSetting filterSetting, Job job)
        {
            HashSet<string> IgnoreKeywords = GetSplitedDataSet(filterSetting.ExcludeWord);
            HashSet<string> IgnoreCompanys = GetSplitedDataSet(filterSetting.IgnoreCompany);

            // 檢查這一份工作的薪資範圍和公司名稱
            if (job.MinimumWage < filterSetting.MinimumWage ||
                job.MaximumWage < filterSetting.MaximumWage ||
                IgnoreCompanys.Contains(job.Company))
            {
                return false;
            }

            // 檢查這一份工作的 title 是否包含欲排除的關鍵字
            foreach (string word in IgnoreKeywords)
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
            // 令所有爬蟲開始任務
            foreach (JobCrawler jobCrawler in JobCrawlers)
            {
                _ = jobCrawler.LoadPage();
            }

            // 等待爬蟲們結束任務
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
                    _ = jobCrawler.ExtractTags();
                }
            }

            // 等待爬蟲們結束任務
            while (JobCrawlers.Any(jobCrawler => !jobCrawler.IsEncounterError() && !jobCrawler.IsExtractFinished()))
            {
                Thread.Sleep(200);
            }

            // 令萃取成功的爬蟲再進一步萃取出各項工作
            foreach (JobCrawler jobCrawler in JobCrawlers)
            {
                if (!jobCrawler.IsEncounterError())
                {
                    jobCrawler.ExtractJobData();
                }
            }
        }

        public static JobList GetValidJobs(FilterSetting filterSetting)
        {
            // 創建多個爬蟲(其數量不超過 NumberOfLogicalProcessors)
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
            };

            // 令爬蟲們抓取頁面 & 取得工作列表
            GetTargetPages(JobCrawlers);
            GetJobSections(JobCrawlers);

            // 根據設定檔來過濾工作列表
            JobList validJobs = new JobList();
            foreach (JobCrawler jobCrawler in JobCrawlers)
            {
                if (!jobCrawler.IsEncounterError())
                {
                    foreach (Job job in jobCrawler.GetJobs())
                    {
                        if (IsValidJob(filterSetting, job))
                        {
                            validJobs.Add(job);
                        }
                    }
                }
            }
            return validJobs;
        }

        public static JobList GetValidJobs(JobList jobList, string blockCompany = null)
        {
            // 去除該公司所提供的工作
            JobList validJobs = new JobList();
            foreach (Job job in jobList)
            {
                if (job.Company != blockCompany)
                {
                    validJobs.Add(job);
                }
            }

            return validJobs;
        }
    }
}
