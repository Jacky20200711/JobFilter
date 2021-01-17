using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace JobFilter.Models.Services.Tests
{
    [TestClass()]
    public class SettingServiceTests
    {
        [TestMethod()]
        public void IsValidStringTest()
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
                // 爬取頁面不能為null
                new FilterSetting
                {
                    CrawlUrl = null,
                    MinimumWage = 30000,
                    MaximumWage = 40000,
                },
                // 爬取頁面不能為空字串
                new FilterSetting
                {
                    CrawlUrl = "",
                    MinimumWage = 30000,
                    MaximumWage = 40000,
                },
                // 最低月薪必須大於9999
                new FilterSetting
                {
                    CrawlUrl = SettingService.TargetUrlHead,
                    MinimumWage = 9999,
                    MaximumWage = 40000,
                },
                // 最高月薪必須小於999999
                new FilterSetting
                {
                    CrawlUrl = SettingService.TargetUrlHead,
                    MinimumWage = 30000,
                    MaximumWage = 1000000,
                },
                // 備註長度必須小於等於5
                new FilterSetting
                {
                    CrawlUrl = SettingService.TargetUrlHead,
                    MinimumWage = 30000,
                    MaximumWage = 40000,
                    Remarks = "123456",
                },
                // 最高月薪必須大於等於最低月薪
                new FilterSetting
                {
                    CrawlUrl = SettingService.TargetUrlHead,
                    MinimumWage = 50000,
                    MaximumWage = 40000,
                },
            };

            // 設計多個正確的設定
            List<FilterSetting> ValidfilterSettings = new List<FilterSetting>
            {
                // 欲排除的關鍵字或公司、備註可為null
                new FilterSetting
                {
                    CrawlUrl = SettingService.TargetUrlHead,
                    MinimumWage = 999999,
                    MaximumWage = 999999,
                },

                // 備註長度必須小於等於5
                new FilterSetting
                {
                    CrawlUrl = SettingService.TargetUrlHead,
                    MinimumWage = 10000,
                    MaximumWage = 999999,
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