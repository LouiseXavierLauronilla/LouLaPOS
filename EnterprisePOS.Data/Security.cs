using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace EnterprisePOS.Data
{
    public static class PasswordHasher
    {
        public static string CreateSalt()
        {
            var salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            return BitConverter.ToString(salt).Replace("-", string.Empty).ToUpperInvariant();
        }

        public static string Hash(string password)
        {
            return Hash(password, CreateSalt());
        }

        public static string Hash(string password, string saltHex)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password is required.", nameof(password));

            var salt = string.IsNullOrWhiteSpace(saltHex) ? CreateSalt() : saltHex.Trim();
            var payload = Encoding.UTF8.GetBytes(salt + password);

            using (var sha = SHA256.Create())
            {
                return ToHex(sha.ComputeHash(payload));
            }
        }

        public static bool Verify(string password, string encoded, string saltHex = null)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(encoded))
                return false;

            try
            {
                var text = encoded.Trim();
                if (text.Contains(":"))
                {
                    var a = text.Split(':');
                    var it = int.Parse(a[0]);
                    var salt = Convert.FromBase64String(a[1]);
                    var expected = Convert.FromBase64String(a[2]);
                    using (var pbk = new Rfc2898DeriveBytes(password, salt, it, HashAlgorithmName.SHA256))
                    {
                        return pbk.GetBytes(32).SequenceEqual(expected);
                    }
                }

                var candidates = new[]
                {
                    Hash(password, saltHex),
                    Hash(password, saltHex ?? string.Empty),
                    ComputeHash((saltHex ?? string.Empty) + password),
                    ComputeHash(password + (saltHex ?? string.Empty))
                };

                return candidates.Any(candidate => string.Equals(candidate, text, StringComparison.OrdinalIgnoreCase));
            }
            catch
            {
                return false;
            }
        }

        private static string ComputeHash(string value)
        {
            using (var sha = SHA256.Create())
            {
                return ToHex(sha.ComputeHash(Encoding.UTF8.GetBytes(value)));
            }
        }

        private static string ToHex(byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", string.Empty).ToUpperInvariant();
        }

        private static byte[] HexToBytes(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return new byte[0];

            var bytes = new byte[value.Length / 2];
            for (var i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(value.Substring(i * 2, 2), 16);
            }

            return bytes;
        }

        private static string HexToString(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var bytes = HexToBytes(value);
            return Encoding.UTF8.GetString(bytes);
        }
    }
}