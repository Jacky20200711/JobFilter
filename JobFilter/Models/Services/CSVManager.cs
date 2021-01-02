using CsvHelper;
using CsvHelper.Configuration;
using JobFilter.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace JobFilter.Models.Services
{
    public static class CSVManager
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
            string ExportPath = ConfigManager.GetValueByKey("ExportPath");

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

        public static void ImportFilterSetting(ApplicationDbContext _context)
        {
            // 從設定檔取得匯入檔的路徑
            string ImportPath = ConfigManager.GetValueByKey("ImportPath");

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
                    _context.FilterSetting.AddRange(DataList);
                    _context.SaveChanges();
                }
            }
        }
    }
}
