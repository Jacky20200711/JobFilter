using JobFilter.Models.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.AspNetCore.Identity;

namespace JobFilter.Models.Services.Tests
{
    [TestClass()]
    public class UserServiceTests
    {
        [TestMethod()]
        public void IsValidUserTest()
        {
            string ErrorMessage = "輸入資料錯誤!";
            IdentityUser identityUser = new IdentityUser();

            // 測試郵件檢測
            identityUser.PasswordHash = "123456";
            identityUser.Email = null;
            Assert.AreEqual(ErrorMessage, UserService.IsValidUser(identityUser));

            identityUser.Email = "";
            Assert.AreEqual(ErrorMessage, UserService.IsValidUser(identityUser));

            identityUser.Email = "123456";
            Assert.AreEqual(ErrorMessage, UserService.IsValidUser(identityUser));

            identityUser.Email = "@gmail.com";
            Assert.AreEqual(ErrorMessage, UserService.IsValidUser(identityUser));

            identityUser.Email = "123456@gmail.com";
            Assert.AreEqual(null, UserService.IsValidUser(identityUser));

            identityUser.Email = "123456@yahoo.com.tw";
            Assert.AreEqual(null, UserService.IsValidUser(identityUser));

            // 測試密碼檢測
            identityUser.Email = "123456@yahoo.com.tw";
            identityUser.PasswordHash = null;
            Assert.AreEqual(ErrorMessage, UserService.IsValidUser(identityUser));

            identityUser.PasswordHash = "";
            Assert.AreEqual(ErrorMessage, UserService.IsValidUser(identityUser));

            identityUser.PasswordHash = "12345";
            Assert.AreEqual(ErrorMessage, UserService.IsValidUser(identityUser));

            identityUser.PasswordHash = "123456";
            Assert.AreEqual(null, UserService.IsValidUser(identityUser));

            identityUser.PasswordHash = "X123456@";
            Assert.AreEqual(null, UserService.IsValidUser(identityUser));
        }

        [TestMethod()]
        public void InAdminGroupTest()
        {
            string SuperAdmin = UserService.SuperAdmin;
            if (!UserService.InAdminGroup(SuperAdmin))
            {
                Assert.Fail();
            }
        }
    }
}