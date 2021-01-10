using System.Collections.Generic;

namespace JobFilter.Models
{
    public static class AuthorizeManager
    {
        // 可以控制所有用戶和資料的超級管理員
        public static string SuperAdmin = "fewer135@gmail.com";

        // 將管理員的資訊存入到記憶體(HashTable)，模擬 Cache 的概念
        private static readonly HashSet<string> AdminGroup = new HashSet<string> { SuperAdmin };

        public static bool InAdminGroup(string email)
        {
            return AdminGroup.Contains(email);
        }
    }
}
