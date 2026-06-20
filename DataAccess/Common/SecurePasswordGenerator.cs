using System.Security.Cryptography;

namespace DataAccess.Common
{
    public static class SecurePasswordGenerator
    {
        private static readonly char[] Chars =
            "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789!@#$%^&*".ToCharArray();

        public static string Generate(int length = 16)
        {
            var bytes = RandomNumberGenerator.GetBytes(length);
            return new string(bytes.Select(b => Chars[b % Chars.Length]).ToArray());
        }
    }
}
