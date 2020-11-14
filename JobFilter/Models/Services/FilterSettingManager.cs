using System;

namespace JobFilter.Models
{
    public static class FilterSettingManager
    {
        public static int Length_limit_IgnoreCompany = 600;
        public static string TargetUrlHead = "https://www.104.com.tw/jobs/search/";

        public static bool IsValidString(string TestStr)
        {
            if (string.IsNullOrEmpty(TestStr))
                return true;

            foreach (char c in TestStr)
            {
                int CharCode = Convert.ToInt32(c);
                if (!(c == ',' || c == '_' || c == '+' || c == ' ' ||
                    CharCode > 47 && CharCode < 58 ||
                    CharCode > 64 && CharCode < 91 ||
                    CharCode > 96 && CharCode < 123 ||
                    CharCode > 0x4E00 && CharCode < 0x9FA5))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsValidSetting(FilterSetting filterSetting)
        {
            // 檢查目標網址
            if (filterSetting.CrawlUrl == null ||
                !filterSetting.CrawlUrl.StartsWith(TargetUrlHead)) 
                return false;

            // 檢查最低薪資
            if (filterSetting.MinimumWage < 100 || filterSetting.MinimumWage > 999999) 
                return false;

            // 檢查最高薪資
            if (filterSetting.MaximumWage < filterSetting.MinimumWage) 
                return false;

            // 檢查欲排除的關鍵字
            if (!IsValidString(filterSetting.ExcludeWord))
                return false;

            // 檢查欲排除的公司名稱
            if (!IsValidString(filterSetting.IgnoreCompany))
                return false;

            // 檢查備註
            if (!IsValidString(filterSetting.Remarks))
                return false;

            return true;
        }
    }
}
