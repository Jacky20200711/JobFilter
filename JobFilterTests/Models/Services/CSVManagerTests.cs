﻿using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JobFilter.Models.Services.Tests
{
    [TestClass()]
    public class CSVManagerTests
    {
        [TestMethod()]
        public void GetFilePathTest()
        {
            string TableName = "FilterSetting";

            // 取得完整檔名
            string FilePath = CSVManager.GetFilePath(TableName);

            // 切出檔案名稱
            string Fname = FilePath.Split("\\")[^1];

            // 檢查檔案名稱
            Assert.AreEqual("FilterSetting.csv", Fname);
        }
    }
}