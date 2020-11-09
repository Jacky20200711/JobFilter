using Microsoft.VisualStudio.TestTools.UnitTesting;
using JobFilter.Models.DataStructure;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobFilter.Models.DataStructure.Tests
{
    [TestClass()]
    public class JobCrawlerTests
    {
        [TestMethod()]
        public void GetLowestWageTest()
        {
            JobCrawler jobCrawler = new JobCrawler("");

            Assert.AreEqual(30000, jobCrawler.GetLowestWage("月薪30000~50000"));
            Assert.AreEqual(30000, jobCrawler.GetLowestWage("月薪30,000~50,000"));
            Assert.AreEqual(800000 / 12, jobCrawler.GetLowestWage("年薪80,0000"));
            Assert.AreEqual(900000 / 12, jobCrawler.GetLowestWage("年薪90,0000~100,0000"));
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
        }
    }
}