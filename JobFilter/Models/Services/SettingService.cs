using JobFilter.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using JobFilter.Models.Services;

namespace JobFilter.Models
{
    public static class SettingService
    {
        // 目標網址的開頭
        public static string TargetUrlHead = "https://www.104.com.tw/jobs/search/";

        // 這裡的長度限制是參照 Models\DataStructure\FilterSetting.cs
        public static int limitLenOfUrl = 800;
        public static int limitLenOfExcludeWord = 50;
        public static int limitLenOfIgnoreCompany = 1500;
        public static int limitLenOfRemark = 5;

        public static bool IsValidString(string TestStr, int LengthLimit = 1000)
        {
            if (string.IsNullOrEmpty(TestStr))
            {
                return true;
            }

            if (TestStr.Length > LengthLimit)
            {
                return false;
            }

            foreach (char c in TestStr)
            {
                int CharCode = Convert.ToInt32(c);
                if (!(c == ',' || c == '_' || c == '+' ||
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
                filterSetting.CrawlUrl.Length > limitLenOfUrl ||
                !filterSetting.CrawlUrl.StartsWith(TargetUrlHead))
            {
                return false;
            }

            // 薪資範圍必須為3~6位數
            if (filterSetting.MinimumWage < 100 || filterSetting.MinimumWage > 999999)
            {
                return false;
            }

            // 最高月薪必須大於最低月薪
            if (filterSetting.MaximumWage < filterSetting.MinimumWage)
            {
                return false;
            }

            // 檢查欲排除的關鍵字
            if (!IsValidString(filterSetting.ExcludeWord, limitLenOfExcludeWord))
            {
                return false;
            }

            // 檢查欲排除的公司名稱
            if (!IsValidString(filterSetting.IgnoreCompany, limitLenOfIgnoreCompany))
            {
                return false;
            }

            // 檢查備註
            if (!IsValidString(filterSetting.Remarks, limitLenOfRemark))
            {
                return false;
            }

            return true;
        }

        public static string CreateSetting(ApplicationDbContext _context, FilterSetting filterSetting, string UserEmail)
        {
            // 查看設定檔的數量是否已達上限
            List<FilterSetting> filterSettings = _context.FilterSetting.Where(m => m.UserEmail == UserEmail).ToList();

            if (filterSettings.Count > 2)
            {
                return "建立失敗，您的設定數量已達上限!";
            }

            // 在後端進行表單驗證
            if (!IsValidSetting(filterSetting))
            {
                return "系統忙碌中，請稍後再試 >___<";
            }

            // 若通過驗證則創建表單
            filterSetting.UserEmail = UserEmail;
            _context.Add(filterSetting);
            return null;
        }

        public static string EditSetting(ApplicationDbContext _context, FilterSetting filterSetting, string UserEmail, int id)
        {
            if (id != filterSetting.Id)
            {
                return "系統忙碌中，請稍後再試 >___<";
            }

            FilterSetting TargetSetting = _context.FilterSetting.FirstOrDefault(m => m.Id == id);

            // 令管理員以外的用戶只能編輯自己的設定
            if (!UserService.InAdminGroup(UserEmail) && TargetSetting.UserEmail != UserEmail)
            {
                return "系統忙碌中，請稍後再試 >___<";
            }

            // 在後端進行表單驗證
            if (!IsValidSetting(filterSetting))
            {
                return "系統忙碌中，請稍後再試 >___<";
            }

            // 若通過驗證則更新資料
            TargetSetting.CrawlUrl = filterSetting.CrawlUrl;
            TargetSetting.MinimumWage = filterSetting.MinimumWage;
            TargetSetting.MaximumWage = filterSetting.MaximumWage;
            TargetSetting.ExcludeWord = filterSetting.ExcludeWord;
            TargetSetting.IgnoreCompany = filterSetting.IgnoreCompany;
            TargetSetting.Remarks = filterSetting.Remarks;
            return null;
        }

        public static string DeleteSetting(ApplicationDbContext _context, string UserEmail, int? id)
        {
            if (id == null)
            {
                return "系統忙碌中，請稍後再試 >___<";
            }

            FilterSetting TargetSetting = _context.FilterSetting.FirstOrDefault(m => m.Id == id);
            if (TargetSetting == null)
            {
                return "系統忙碌中，請稍後再試 >___<";
            }

            // 令管理員以外的用戶只能刪除自己的設定
            if (!UserService.InAdminGroup(UserEmail) && TargetSetting.UserEmail != UserEmail)
            {
                return "系統忙碌中，請稍後再試 >___<";
            }

            _context.FilterSetting.Remove(TargetSetting);
            return null;
        }

        public static string AddBlockCompany(ApplicationDbContext _context, string UserEmail, string BlockCompany = null)
        {
            
            // 檢查該公司的名稱與長度
            if (!IsValidString(BlockCompany, 50))
            {
               return "封鎖失敗，此公司的名稱含有不支援的字元或是字數超過限制(50字)!";
            }

            var TargetSetting = _context.FilterSetting.Where(m => m.UserEmail == UserEmail);
            if (TargetSetting == null)
            {
                return "系統忙碌中，請稍後再試 >___<";
            }

            // 嘗試將新封鎖的公司添加到該 User 的所有設定檔
            foreach (var UserSetting in TargetSetting)
            {
                // 檢查設定檔的欄位是否為 NULL
                if (string.IsNullOrEmpty(UserSetting.IgnoreCompany))
                {
                    // 賦值給原本為 NULL 的欄位
                    UserSetting.IgnoreCompany = $"{BlockCompany}";
                }
                else
                {
                    // 檢查該欄位的新長度是否保持合法
                    if (UserSetting.IgnoreCompany.Length + $",{BlockCompany}".Length > limitLenOfIgnoreCompany)
                    {
                        return $"封鎖未完全，請確認您所有的設定檔在封鎖此公司後，字數皆不會超過{limitLenOfIgnoreCompany}字!";
                    }

                    // 若長度合法則進行串接
                    UserSetting.IgnoreCompany += $",{BlockCompany}";
                }
            }
            return null;
        }
    }
}
