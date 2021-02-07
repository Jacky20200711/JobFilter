using JobFilter.Models.DataStructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JobFilter.Models.DataStructure.Tests
{
    [TestClass()]
    public class JobCrawlerTests
    {
        [TestMethod()]
        public void GetFirstNumTest()
        {
            JobCrawler jobCrawler = new JobCrawler("");
            Assert.AreEqual(30000, jobCrawler.GetFirstNum("月薪30000~50000"));
            Assert.AreEqual(30000, jobCrawler.GetFirstNum("月薪30,000~50,000"));
            Assert.AreEqual(800000, jobCrawler.GetFirstNum("年薪80,0000"));
            Assert.AreEqual(900000, jobCrawler.GetFirstNum("年薪90,0000~100,0000"));
            Assert.AreEqual(1000000, jobCrawler.GetFirstNum("年薪~100,0000"));
        }

        [TestMethod()]
        public void GetMinWageTest()
        {
            JobCrawler jobCrawler = new JobCrawler("");
            Assert.AreEqual(30000, jobCrawler.GetMinWage("月薪30000~50000"));
            Assert.AreEqual(30000, jobCrawler.GetMinWage("月薪30,000~50,000"));
            Assert.AreEqual(800000 / 12, jobCrawler.GetMinWage("年薪80,0000"));
            Assert.AreEqual(900000 / 12, jobCrawler.GetMinWage("年薪90,0000~100,0000"));
        }

        [TestMethod()]
        public void GetMaxWageTest()
        {
            // 若只有寫一個數字則返回最大值，確保[只寫一個數字但有符合最低月薪的工作]不會被過濾掉
            JobCrawler jobCrawler = new JobCrawler("");
            Assert.AreEqual(50000, jobCrawler.GetMaxWage("月薪30000~50000"));
            Assert.AreEqual(50000, jobCrawler.GetMaxWage("月薪30,000~50,000"));
            Assert.AreEqual(2147483647, jobCrawler.GetMaxWage("月薪50,000"));
            Assert.AreEqual(2147483647, jobCrawler.GetMaxWage("年薪80,0000"));
            Assert.AreEqual(2147483647, jobCrawler.GetMaxWage("待遇面議"));
            Assert.AreEqual(1000000 / 12, jobCrawler.GetMaxWage("年薪90,0000~100,0000"));
        }

        [TestMethod()]
        public void GetValueBetweenTwoStringTest()
        {
            JobCrawler jobCrawler = new JobCrawler("");
            Assert.AreEqual("B", jobCrawler.GetValueBetweenTwoString("ABC", "A", "C"));
            Assert.AreEqual("B", jobCrawler.GetValueBetweenTwoString("ACB\"AC", "AC", "\""));
            Assert.AreEqual("B", jobCrawler.GetValueBetweenTwoString("<li>B</li>", "<li>", "</li>"));
            Assert.AreEqual("", jobCrawler.GetValueBetweenTwoString("ABC", "", "C"));
            Assert.AreEqual("", jobCrawler.GetValueBetweenTwoString("ABC", "A", ""));
            Assert.AreEqual("", jobCrawler.GetValueBetweenTwoString("", "A", "C"));
            Assert.AreEqual("ABC", jobCrawler.GetValueBetweenTwoString("<li>ABC</li>", "<li>", "</li>"));

            // 測試若目標字串超過80字，且不包含欲擷取的右界，則是否會正確擷取80個字
            string TargetSection = ">AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAB";
            Assert.AreEqual(80, jobCrawler.GetValueBetweenTwoString(TargetSection, ">", "C", 80).Length);
        }

        [TestMethod()]
        public void GetSubStringTest()
        {
            // 檢查第一個參數是否為 null 由 JobCrawler.ExtractJobData 處理
            // 檢查第二個參數是否大於字串由 JobCrawler.ExtractJobData 處理
            // 不需要在此對上述情況進行測試
            JobCrawler jobCrawler = new JobCrawler("");
            Assert.AreEqual("BCDE", jobCrawler.GetSubString("ABCDE", 1, 4));
            Assert.AreEqual("ABC", jobCrawler.GetSubString("ABC", 0, 5));
            Assert.AreEqual("C", jobCrawler.GetSubString("ABC", 2, 5));
            Assert.AreEqual("測", jobCrawler.GetSubString("測", 0, 1));
            Assert.AreEqual("測", jobCrawler.GetSubString("測", 0, 5));
        }
    }
}