using JobFilter.Models.DataStructure;
using System;
using System.Collections.Generic;

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
    }
}
