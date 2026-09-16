
using System;
using System.Security.Cryptography;
using System.Text;

namespace PandoraWeb.Helpers
{
    public static class SecurityHelper
    {
        // Legacy hashing (Mật khẩu cũ)
        public static string HashSHA256(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        // New secure hashing using BCrypt
        public static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password)) return string.Empty;
            return BCrypt.Net.BCrypt.EnhancedHashPassword(password, 13);
        }

        public static bool VerifyPassword(string password, string hash)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hash)) return false;

            // Optional: Support legacy SHA256 hashes if length is exactly 64 (hex)
            if (hash.Length == 64 && !hash.StartsWith("$2"))
            {
                return HashSHA256(password) == hash;
            }

            try 
            {
                return BCrypt.Net.BCrypt.EnhancedVerify(password, hash);
            } 
            catch 
            {
                return false;
            }
        }
    }
}
