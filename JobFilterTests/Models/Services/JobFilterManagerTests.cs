using JobFilter.Models.Services;
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

            foreach (var verifier in VerifyGroups)
            {
                Assert.AreEqual(verifier.GetValid(), JobFilterManager.IsValidJob(verifier.GetFilterSetting(), verifier.GetJob()));
            }
        }

        [TestMethod()]
        public void GetValidJobListTest()
        {
            bool PassTest;
            List<Job> validJobList;
            JobList jobList = new JobList
            {
                new Job { Company = "A公司" },
                new Job { Company = "B公司" },
            };

            // 不封鎖任何公司
            PassTest = false;
            validJobList = JobFilterManager.GetValidJobList(jobList, null).JobItems;
            if (validJobList.Count == 2 && validJobList[0].Company == "A公司" && validJobList[1].Company == "B公司")
            {
                PassTest = true;
            }
            Assert.AreEqual(true, PassTest);

            // 封鎖A公司
            PassTest = false;
            validJobList = JobFilterManager.GetValidJobList(jobList, "A公司").JobItems;
            if (validJobList.Count == 1 && validJobList[0].Company == "B公司")
            {
                PassTest = true;
            }
            Assert.AreEqual(true, PassTest);

            // 封鎖B公司
            PassTest = false;
            validJobList = JobFilterManager.GetValidJobList(jobList, "B公司").JobItems;
            if (validJobList.Count == 1 && validJobList[0].Company == "A公司")
            {
                PassTest = true;
            }
            Assert.AreEqual(true, PassTest);
        }
    }
}