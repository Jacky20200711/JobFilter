using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.AspNetCore.Identity;

namespace JobFilter.Models.Services.Tests
{
    [TestClass()]
    public class IdentityUserManagerTests
    {
        [TestMethod()]
        public void IsValidUserTest()
        {
            string ErrorMessage = "輸入資料錯誤!";
            IdentityUser identityUser = new IdentityUser();

            // 測試郵件檢測
            identityUser.PasswordHash = "123456";
            identityUser.Email = null;
            Assert.AreEqual(ErrorMessage, IdentityUserManager.IsValidUser(identityUser));

            identityUser.Email = "";
            Assert.AreEqual(ErrorMessage, IdentityUserManager.IsValidUser(identityUser));

            identityUser.Email = "123456";
            Assert.AreEqual(ErrorMessage, IdentityUserManager.IsValidUser(identityUser));

            identityUser.Email = "@gmail.com";
            Assert.AreEqual(ErrorMessage, IdentityUserManager.IsValidUser(identityUser));

            identityUser.Email = "123456@gmail.com";
            Assert.AreEqual(null, IdentityUserManager.IsValidUser(identityUser));

            identityUser.Email = "123456@yahoo.com.tw";
            Assert.AreEqual(null, IdentityUserManager.IsValidUser(identityUser));

            // 測試密碼檢測
            identityUser.Email = "123456@yahoo.com.tw";
            identityUser.PasswordHash = null;
            Assert.AreEqual(ErrorMessage, IdentityUserManager.IsValidUser(identityUser));

            identityUser.PasswordHash = "";
            Assert.AreEqual(ErrorMessage, IdentityUserManager.IsValidUser(identityUser));

            identityUser.PasswordHash = "12345";
            Assert.AreEqual(ErrorMessage, IdentityUserManager.IsValidUser(identityUser));

            identityUser.PasswordHash = "123456";
            Assert.AreEqual(null, IdentityUserManager.IsValidUser(identityUser));

            identityUser.PasswordHash = "X123456@";
            Assert.AreEqual(null, IdentityUserManager.IsValidUser(identityUser));
        }
    }
}