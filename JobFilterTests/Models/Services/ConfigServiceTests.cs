﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace JobFilter.Models.Services.Tests
{
    [TestClass()]
    public class ConfigServiceTests
    {
        [TestMethod()]
        public void GetValueByKeyTest()
        {
            // 傳入單個KEY(先手動到設定檔確認這些KEY存在)
            List<string> ConfigValues = new List<string>
            {
                ConfigService.GetValueByKey("ExportPath"),
                ConfigService.GetValueByKey("ImportPath")
            };

            // 測試取得的值是否皆有效
            foreach (string value in ConfigValues)
            {
                if (string.IsNullOrEmpty(value))
                {
                    Assert.Fail();
                }
            }

            // 傳入多個KEY(先手動到設定檔確認這些KEY存在)
            Dictionary<string, string> ConfigDict = ConfigService.GetValueByKey(new List<string>
            {
                "ExportPath",
                "ImportPath"
            });

            // 測試取得的值是否皆有效
            foreach (var pair in ConfigDict)
            {
                if (string.IsNullOrEmpty(pair.Value))
                {
                    Assert.Fail();
                }
            }
        }

        [TestMethod()]
        public void GetAllPairTest()
        {
            // 餵入一些最外層的KEY(先手動到設定檔確認這些KEY存在)
            HashSet<string> ConfigKeys = new HashSet<string>
            {
                "ConnectionStrings",
                "Logging",
                "Authentication",
                "AppSetting"
            };

            // 比對這些 KEY 是否存在於設定檔
            List<bool> ValidChecker = new List<bool>();
            var config = ConfigService.GetAllPair();
            foreach (KeyValuePair<string, string> pair in config)
            {
                if (ConfigKeys.Contains(pair.Key))
                {
                    ValidChecker.Add(true);
                }
            }

            // 測試比對成功的次數，是否等於餵入的KEY數量
            Assert.AreEqual(ValidChecker.Count, ConfigKeys.Count);
        }
    }
}