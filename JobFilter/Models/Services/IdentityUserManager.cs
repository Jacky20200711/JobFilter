using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace JobFilter.Models.Services
{
    public static class IdentityUserManager
    {
        public static string IsValidUser(Microsoft.AspNetCore.Identity.IdentityUser identityUser)
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
    }
}
