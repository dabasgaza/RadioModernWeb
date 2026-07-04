// ============================================================
// SecurePasswordGenerator — مولد كلمات المرور
// ============================================================
// المسؤولية: تعريف مولد كلمات المرور.
// ============================================================
using System.Security.Cryptography;

namespace DataAccess.Common
{
    /// <summary>
    /// صنف مولد كلمات المرور.
    /// </summary>
    public static class SecurePasswordGenerator
    {
        private static readonly char[] Chars =
            "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789!@#$%^&*".ToCharArray();

        /// <summary>
        /// توليد مولد كلمات المرور.
        /// </summary>
        public static string Generate(int length = 16)
        {
            var bytes = RandomNumberGenerator.GetBytes(length);
            return new string(bytes.Select(b => Chars[b % Chars.Length]).ToArray());
        }
    }
}
