using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using JobFilter.Models.DataStructure;

namespace JobFilter.Models.Services.Tests
{
    [TestClass()]
    public class JobFilterManagerTests
    {
        // 將要比對的資料封裝到一個類別
        public class Verifier
        {
            FilterSetting _filterSetting;
            Job _job;
            bool _valid;

            public Verifier(FilterSetting filterSetting, Job job, bool valid)
            {
                _filterSetting = filterSetting;
                _job = job;
                _valid = valid;
            }

            public FilterSetting GetFilterSetting()
            {
                return _filterSetting;
            }

            public Job GetJob()
            {
                return _job;
            }

            public bool GetValid()
            {
                return _valid;
            }
        } 

        [TestMethod()]
        public void IsValidJobTest()
        {
            List<Verifier> VerifyGroups = new List<Verifier>
            {
                // 測試是否排除過低的薪資
                new Verifier(
                        new FilterSetting()
                        {
                            MinimumWage = 40000,
                            ExcludeWord = "",
                            IgnoreCompany = ""
                        },
                        new Job()
                        {
                            MinimumWage = 39999,
                            Title = "123",
                            Company = "測試股份有限公司"
                        },
                        false
                    ),
                // 測試是否排除該關鍵字(不論大小寫)
                new Verifier(
                        new FilterSetting()
                        {
                            MinimumWage = 40000,
                            ExcludeWord = "Java,PHP,JavaScript",
                            IgnoreCompany = "測試有限公司,測試股份有限公司"
                        },
                        new Job()
                        {
                            MinimumWage = 40000,
                            Title = "javascript",
                            Company = "123股份有限公司"
                        },
                        false
                    ),
                // 測試是否排除該公司
                new Verifier(
                        new FilterSetting()
                        {
                            MinimumWage = 40000,
                            ExcludeWord = "",
                            IgnoreCompany = "測試有限公司,測試股份有限公司"
                        },
                        new Job()
                        {
                            MinimumWage = 40000,
                            Title = "java",
                            Company = "測試股份有限公司"
                        },
                        false
                    ),
            };

            foreach(var verifier in VerifyGroups)
            {
                Assert.AreEqual(verifier.GetValid(), JobFilterManager.IsValidJob(verifier.GetFilterSetting(), verifier.GetJob()));
            }
        }
    }
}