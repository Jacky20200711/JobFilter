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
        public void GetValueBetweenCharsTest()
        {
            JobCrawler jobCrawler = new JobCrawler("");

            Assert.AreEqual("B", jobCrawler.GetValueBetweenChars("ABC", 'A', 'C'));
            Assert.AreEqual("B", jobCrawler.GetValueBetweenChars("AAA\"B\"AAA", '"', '"'));
            Assert.AreEqual("B", jobCrawler.GetValueBetweenChars("<li>B</li>", '>', '<'));
        }

        [TestMethod()]
        public void GetLowestWageTest()
        {
            JobCrawler jobCrawler = new JobCrawler("");

            Assert.AreEqual(30000, jobCrawler.GetLowestWage("月薪30000~50000"));
            Assert.AreEqual(30000, jobCrawler.GetLowestWage("月薪30,000~50,000"));
            Assert.AreEqual(800000/12, jobCrawler.GetLowestWage("年薪80,0000"));
            Assert.AreEqual(900000 / 12, jobCrawler.GetLowestWage("年薪90,0000~100,0000"));
        }
    }
}