using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace JobFilter.Models.Services.Tests
{
    [TestClass()]
    public class SettingServiceTests
    {
        [TestMethod()]
        public void IsValidSetringTest()
        {
            Assert.AreEqual(true, SettingService.IsValidString(",_+測試"));
            Assert.AreEqual(true, SettingService.IsValidString(""));
            Assert.AreEqual(true, SettingService.IsValidString(null));
            Assert.AreEqual(false, SettingService.IsValidString("!"));
            Assert.AreEqual(false, SettingService.IsValidString("<"));
            Assert.AreEqual(false, SettingService.IsValidString("'"));
            Assert.AreEqual(false, SettingService.IsValidString("\""));
        }

        [TestMethod()]
        public void IsValidSettingTest()
        {
            // 設計多個錯誤的設定
            List<FilterSetting> InValidfilterSettings = new List<FilterSetting>
            {
                new FilterSetting
                {
                    CrawlUrl = null,
                    MinimumWage = -1,
                    MaximumWage = -1,
                    ExcludeWord = null,
                    IgnoreCompany = null
                },

                new FilterSetting
                {
                    CrawlUrl = "",
                    MinimumWage = -1,
                    MaximumWage = -1,
                    ExcludeWord = null,
                    IgnoreCompany = null
                },

                new FilterSetting
                {
                    CrawlUrl = SettingService.TargetUrlHead,
                    MinimumWage = 1000000,
                    MaximumWage = 2000000,
                    ExcludeWord = "",
                    IgnoreCompany = ""
                },

                new FilterSetting
                {
                    CrawlUrl = SettingService.TargetUrlHead,
                    MinimumWage = 999999,
                    MaximumWage = 999998,
                    ExcludeWord = "",
                    IgnoreCompany = ""
                },

                new FilterSetting
                {
                    CrawlUrl = SettingService.TargetUrlHead,
                    MinimumWage = 999999,
                    MaximumWage = 999999,
                    ExcludeWord = "",
                    IgnoreCompany = "",
                    Remarks = "123456",
                },
            };

            // 設計多個正確的設定
            List<FilterSetting> ValidfilterSettings = new List<FilterSetting>
            {
                new FilterSetting
                {
                    CrawlUrl = SettingService.TargetUrlHead,
                    MinimumWage = 999999,
                    MaximumWage = 999999,
                    ExcludeWord = null,
                    IgnoreCompany = null
                },

                new FilterSetting
                {
                    CrawlUrl = SettingService.TargetUrlHead,
                    MinimumWage = 999999,
                    MaximumWage = 999999,
                    ExcludeWord = "",
                    IgnoreCompany = ""
                },

                new FilterSetting
                {
                    CrawlUrl = SettingService.TargetUrlHead,
                    MinimumWage = 999999,
                    MaximumWage = 999999,
                    ExcludeWord = "測試A,測試B,321",
                    IgnoreCompany = ""
                },

                new FilterSetting
                {
                    CrawlUrl = SettingService.TargetUrlHead,
                    MinimumWage = 999999,
                    MaximumWage = 999999,
                    ExcludeWord = "",
                    IgnoreCompany = "測試A,測試B,321"
                },

                new FilterSetting
                {
                    CrawlUrl = SettingService.TargetUrlHead,
                    MinimumWage = 999999,
                    MaximumWage = 999999,
                    ExcludeWord = "",
                    IgnoreCompany = "",
                    Remarks = "12測測測",
                },
            };

            // 比對所有錯誤的設定
            foreach (FilterSetting filterSetting in InValidfilterSettings)
            {
                Assert.AreEqual(false, SettingService.IsValidSetting(filterSetting));
            }

            // 比對所有正確的設定
            foreach (FilterSetting filterSetting in ValidfilterSettings)
            {
                Assert.AreEqual(true, SettingService.IsValidSetting(filterSetting));
            }
        }
    }
}