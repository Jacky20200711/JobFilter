using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace JobFilter.Models.Tests
{
    [TestClass()]
    public class JobFilterManagerTests
    {
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
                    ExcludeWord = null,
                    IgnoreCompany = null
                },

                new FilterSetting
                {
                    CrawlUrl = "",
                    MinimumWage = -1,
                    ExcludeWord = null,
                    IgnoreCompany = ""
                },

                new FilterSetting
                {
                    CrawlUrl = JobFilterManager.TargetUrlHead,
                    MinimumWage = 1000000,
                    ExcludeWord = "",
                    IgnoreCompany = null
                },

                new FilterSetting
                {
                    CrawlUrl = JobFilterManager.TargetUrlHead,
                    MinimumWage = 999999,
                    ExcludeWord = "<",
                    IgnoreCompany = ""
                },

                new FilterSetting
                {
                    CrawlUrl = JobFilterManager.TargetUrlHead,
                    MinimumWage = 999999,
                    ExcludeWord = "",
                    IgnoreCompany = "<"
                }
            };

            // 設計多個正確的設定
            List<FilterSetting> ValidfilterSettings = new List<FilterSetting>
            {
                new FilterSetting
                {
                    CrawlUrl = JobFilterManager.TargetUrlHead,
                    MinimumWage = 999999,
                    ExcludeWord = "",
                    IgnoreCompany = null
                },

                new FilterSetting
                {
                    CrawlUrl = JobFilterManager.TargetUrlHead,
                    MinimumWage = 999999,
                    ExcludeWord = null,
                    IgnoreCompany = ""
                },

                new FilterSetting
                {
                    CrawlUrl = JobFilterManager.TargetUrlHead,
                    MinimumWage = 999999,
                    ExcludeWord = null,
                    IgnoreCompany = null
                },

                new FilterSetting
                {
                    CrawlUrl = JobFilterManager.TargetUrlHead,
                    MinimumWage = 999999,
                    ExcludeWord = "",
                    IgnoreCompany = ""
                },

                new FilterSetting
                {
                    CrawlUrl = JobFilterManager.TargetUrlHead,
                    MinimumWage = 100,
                    ExcludeWord = "",
                    IgnoreCompany = ""
                },

                new FilterSetting
                {
                    CrawlUrl = JobFilterManager.TargetUrlHead,
                    MinimumWage = 999999,
                    ExcludeWord = "測試A,測試B,321",
                    IgnoreCompany = ""
                },

                new FilterSetting
                {
                    CrawlUrl = JobFilterManager.TargetUrlHead,
                    MinimumWage = 999999,
                    ExcludeWord = "",
                    IgnoreCompany = "測試A,測試B,321"
                }
            };

            // 比對所有錯誤的設定
            foreach (FilterSetting filterSetting in InValidfilterSettings)
            {
                Assert.AreEqual(false, JobFilterManager.IsValidSetting(filterSetting));
            }

            // 比對所有正確的設定
            foreach (FilterSetting filterSetting in ValidfilterSettings)
            {
                Assert.AreEqual(true, JobFilterManager.IsValidSetting(filterSetting));
            }
        }
    }
}