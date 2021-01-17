using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using JobFilter.Models.DataStructure;

namespace JobFilter.Models.Services.Tests
{
    [TestClass()]
    public class JobServiceTests
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
                Assert.AreEqual(verifier.GetValid(), JobService.IsValidJob(verifier.GetFilterSetting(), verifier.GetJob()));
            }
        }

        [TestMethod()]
        public void GetValidJobListTest()
        {
            List<Job> validJobList;
            JobList jobList = new JobList
            {
                new Job { Company = "A公司" },
                new Job { Company = "B公司" },
            };

            // 不做封鎖
            validJobList = JobService.GetValidJobList(jobList, null).JobItems;
            Assert.AreEqual(true, validJobList.Count == 2 && validJobList[0].Company == "A公司" && validJobList[1].Company == "B公司");

            // 封鎖A公司
            validJobList = JobService.GetValidJobList(jobList, "A公司").JobItems;
            Assert.AreEqual(true, validJobList.Count == 1 && validJobList[0].Company == "B公司");

            // 封鎖B公司
            validJobList = JobService.GetValidJobList(jobList, "B公司").JobItems;
            Assert.AreEqual(true, validJobList.Count == 1 && validJobList[0].Company == "A公司");
        }
    }
}