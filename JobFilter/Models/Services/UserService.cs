using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace JobFilter.Models.Services
{
    public static class UserService
    {
        public static string IsValidUser(IdentityUser identityUser)
        {
            if (string.IsNullOrEmpty(identityUser.Email) ||
                string.IsNullOrEmpty(identityUser.PasswordHash) ||
                !Regex.IsMatch(identityUser.Email, @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$") ||
                identityUser.PasswordHash.Length < 6)
            {
                return "輸入資料錯誤!";
            }
            return null;
        }

        // 可以控制所有用戶和資料的超級管理員
        public static string SuperAdmin = "fewer135@gmail.com";

        // 將管理員的資訊存入到記憶體(HashTable)以方便做權限比對
        private static readonly HashSet<string> AdminGroup = new HashSet<string> { SuperAdmin };

        public static bool InAdminGroup(string email)
        {
            return AdminGroup.Contains(email);
        }
    }
}
