using System;

namespace JobFilter.Models
{
    public static class FilterSettingManager
    {
        // 這裡的長度限制是參照 Models\DataStructure\FilterSetting.cs
        public static int Length_limit_CrawlUrl = 800;
        public static int Length_limit_ExcludeWord = 200;
        public static int Length_limit_IgnoreCompany = 1000;
        public static int Length_limit_Remarks = 10;

        public static string TargetUrlHead = "https://www.104.com.tw/jobs/search/";

        public static bool IsValidString(string TestStr, int LengthLimit = 1000)
        {
            if (string.IsNullOrEmpty(TestStr))
                return true;

            if (TestStr.Length > LengthLimit)
                return false;

            foreach (char c in TestStr)
            {
                int CharCode = Convert.ToInt32(c);
                if (!(c == ',' || c == '_' || c == '+' || c == ' ' || c == '.' ||
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
            // 檢查目標網址(不允許NULL)
            if (filterSetting.CrawlUrl == null ||
                filterSetting.CrawlUrl.Length > Length_limit_CrawlUrl ||
                !filterSetting.CrawlUrl.StartsWith(TargetUrlHead)) 
                return false;

            // 檢查最低薪資
            if (filterSetting.MinimumWage < 100 || filterSetting.MinimumWage > 999999) 
                return false;

            // 檢查最高薪資
            if (filterSetting.MaximumWage < filterSetting.MinimumWage) 
                return false;

            // 檢查欲排除的關鍵字
            if (!IsValidString(filterSetting.ExcludeWord, Length_limit_ExcludeWord))
                return false;

            // 檢查欲排除的公司名稱
            if (!IsValidString(filterSetting.IgnoreCompany, Length_limit_IgnoreCompany))
                return false;

            // 檢查備註
            if (!IsValidString(filterSetting.Remarks, Length_limit_Remarks))
                return false;

            return true;
        }
    }
}
