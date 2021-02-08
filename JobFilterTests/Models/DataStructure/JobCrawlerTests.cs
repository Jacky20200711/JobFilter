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
            Assert.AreEqual("", jobCrawler.GetValueBetweenTwoString("ABC", "A", ""));
            Assert.AreEqual("", jobCrawler.GetValueBetweenTwoString("", "A", "C"));
            Assert.AreEqual("ABC", jobCrawler.GetValueBetweenTwoString("<li>ABC</li>", "<li>", "</li>"));

            // 測試若不包含欲擷取的右界，則返回的字串是否 = "左界到尾端的字元" + "..."
            string TargetSection = ">ABCDE";
            Assert.AreEqual(8, jobCrawler.GetValueBetweenTwoString(TargetSection, ">", "<").Length);
        }
    }
}