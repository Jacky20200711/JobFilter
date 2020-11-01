using System;

namespace JobFilter.Models
{
    public static class JobFilterManager
    {
        public static string TargetUrlHead = "https://www.104.com.tw/jobs";

        public static bool IsValidSetting(FilterSetting filterSetting)
        {
            // 檢查目標網址
            if (filterSetting.CrawlUrl == null ||
                !filterSetting.CrawlUrl.StartsWith(TargetUrlHead)) return false;

            // 檢查最低薪資
            if (filterSetting.MinimumWage < 100 || filterSetting.MinimumWage > 999999) return false;

            // 檢查欲排除的關鍵字
            if(filterSetting.ExcludeWord != null)
            {
                foreach (char c in filterSetting.ExcludeWord)
                {
                    int CharCode = Convert.ToInt32(c);
                    if (!(c == ',' ||
                        CharCode > 47 && CharCode < 58 ||
                        CharCode > 64 && CharCode < 91 ||
                        CharCode > 96 && CharCode < 123 ||
                        CharCode > 0x4E00 && CharCode < 0x9FA5))
                    {
                        return false;
                    }
                }
            }

            // 檢查欲排除的公司名稱
            if (filterSetting.IgnoreCompany != null)
            {
                foreach (char c in filterSetting.IgnoreCompany)
                {
                    int CharCode = Convert.ToInt32(c);
                    if (!(c == ',' ||
                        CharCode > 47 && CharCode < 58 ||
                        CharCode > 64 && CharCode < 91 ||
                        CharCode > 96 && CharCode < 123 ||
                        CharCode > 0x4E00 && CharCode < 0x9FA5))
                    {
                        return false;
                    }
                }
            }
                
            // 全部都通過則返回 true
            return true;
        }
    }
}
