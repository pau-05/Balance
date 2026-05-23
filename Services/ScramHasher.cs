using System.Security.Cryptography;
using System.Text;

namespace Balance.API.Services
{
    public static class ScramHasher
    {
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iterations = 10000;

        public static string HashPassword(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);

            //UTF-16
            byte[] passwordBytes = Encoding.Unicode.GetBytes(password);

            using var pbkdf2 = new Rfc2898DeriveBytes(passwordBytes, salt, Iterations, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(HashSize);

            return $"{Iterations}:{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
        }

        public static bool VerifyPassword(string password, string storedHash)
        {
            var parts = storedHash.Split(':');
            if (parts.Length != 3) return false;

            if (!int.TryParse(parts[0], out int iterations)) return false;

            byte[] salt = Convert.FromBase64String(parts[1]);
            byte[] storedHashBytes = Convert.FromBase64String(parts[2]);

            // 🔥 Usar UTF-16 para ser compatible
            byte[] passwordBytes = Encoding.Unicode.GetBytes(password);

            using var pbkdf2 = new Rfc2898DeriveBytes(passwordBytes, salt, iterations, HashAlgorithmName.SHA256);
            byte[] computedHash = pbkdf2.GetBytes(storedHashBytes.Length);

            return CryptographicOperations.FixedTimeEquals(computedHash, storedHashBytes);
        }
    }
}