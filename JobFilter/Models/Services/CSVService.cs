using CsvHelper;
using CsvHelper.Configuration;
using JobFilter.Data;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace JobFilter.Models.Services
{
    public static class CSVService
    {
        // 讀取的時候忽略ID屬性
        private class FilterSettingtMap : ClassMap<FilterSetting>
        {
            public FilterSettingtMap()
            {
                AutoMap(CultureInfo.InvariantCulture);
                Map(m => m.Id).Ignore();
            }
        }

        public static string GetFilePath(string TableName)
        {
            // 從設定檔取得匯出路徑
            string ExportPath = ConfigService.GetValueByKey("ExportPath");

            // 串成完整的檔案路徑
            string[] PathSplit = { ExportPath, TableName, ".csv" };
            return string.Join("", PathSplit);
        }

        public static void ExportFilterSetting(ApplicationDbContext _context)
        {
            List<FilterSetting> DataList = _context.FilterSetting.OrderBy(m => m.UserEmail).ToList();
            using var writer = new StreamWriter(GetFilePath("FilterSetting"), false, Encoding.UTF8);
            using var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture);
            csvWriter.WriteRecords(DataList);
        }

        public static List<FilterSetting> GetValidDataBeforeImport(List<FilterSetting> DataList)
        {
            List<FilterSetting> validDataList = new List<FilterSetting>();
            int limitLenOfUrl = SettingService.limitLenOfUrl;
            int limitLenOfExcludeWord = SettingService.limitLenOfExcludeWord;
            int limitLenOfIgnoreCompany = SettingService.limitLenOfIgnoreCompany;
            int limitLenOfRemark = SettingService.limitLenOfRemark;

            // 匯入前先檢查所有的設定
            foreach(FilterSetting f in DataList)
            {
                // 若資料有誤，可能是因為最近有變更欄位長度，所以先檢查並修正長度
                if (!SettingService.IsValidSetting(f))
                {
                    // 檢查並修正長度
                    if (f.CrawlUrl.Length > limitLenOfUrl)
                    {
                        f.CrawlUrl = f.CrawlUrl.Substring(0, limitLenOfUrl);
                    }
                    if (f.ExcludeWord.Length > limitLenOfExcludeWord)
                    {
                        f.ExcludeWord = f.ExcludeWord.Substring(0, limitLenOfExcludeWord);
                    }
                    if (f.IgnoreCompany.Length > limitLenOfIgnoreCompany)
                    {
                        f.IgnoreCompany = f.IgnoreCompany.Substring(0, limitLenOfIgnoreCompany);
                    }
                    if (f.Remarks.Length > limitLenOfRemark)
                    {
                        f.Remarks = f.Remarks.Substring(0, limitLenOfRemark);
                    }

                    // 若修正長度後合法，則添加這筆資料
                    if (SettingService.IsValidSetting(f))
                    {
                        validDataList.Add(f);
                    }
                }
                // 若資料無誤則添加
                else
                {
                    validDataList.Add(f);
                }
            }
            return validDataList;
        }

        public static void ImportFilterSetting(ApplicationDbContext _context)
        {
            // 從設定檔取得匯入檔的路徑
            string ImportPath = ConfigService.GetValueByKey("ImportPath");

            // 找到目標檔案並匯入
            foreach (string FilePath in Directory.GetFileSystemEntries(ImportPath, "*.csv"))
            {
                string fname = Path.GetFileNameWithoutExtension(FilePath);

                if (fname.StartsWith("FilterSetting"))
                {
                    using var reader = new StreamReader(FilePath, Encoding.UTF8);
                    var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);
                    csvReader.Configuration.RegisterClassMap<FilterSettingtMap>();
                    var DataList = csvReader.GetRecords<FilterSetting>().ToList();
                    _context.FilterSetting.AddRange(GetValidDataBeforeImport(DataList));
                    _context.SaveChanges();
                }
            }
        }

        public static void ExportUser(ApplicationDbContext _context)
        {
            string SuperAdmin = UserService.SuperAdmin;
            List<IdentityUser> DataList = _context.Users.Where(u => u.Email != SuperAdmin).ToList();
            using var writer = new StreamWriter(GetFilePath("User"), false, Encoding.UTF8);
            using var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture);
            csvWriter.WriteRecords(DataList);
        }

        public static void ImportUser(ApplicationDbContext _context)
        {
            // 從設定檔取得匯入檔的路徑
            string ImportPath = ConfigService.GetValueByKey("ImportPath");

            // 找到目標檔案並匯入
            foreach (string FilePath in Directory.GetFileSystemEntries(ImportPath, "*.csv"))
            {
                string fname = Path.GetFileNameWithoutExtension(FilePath);

                if (fname.StartsWith("User"))
                {
                    using var reader = new StreamReader(FilePath, Encoding.UTF8);
                    var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture);
                    var DataList = csvReader.GetRecords<IdentityUser>().ToList();
                    _context.Users.AddRange(DataList);
                    _context.SaveChanges();
                }
            }
        }
    }
}
