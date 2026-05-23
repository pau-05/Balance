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
            Console.WriteLine($"=== VERIFY DEBUG ===");
            Console.WriteLine($"Password recibida: {password}");
            Console.WriteLine($"StoredHash: {storedHash}");

            var parts = storedHash.Split(':');
            if (parts.Length != 3)
            {
                Console.WriteLine($"ERROR: Formato de hash inválido. Partes: {parts.Length}");
                return false;
            }

            if (!int.TryParse(parts[0], out int iterations))
            {
                Console.WriteLine($"ERROR: Iteraciones inválidas: {parts[0]}");
                return false;
            }

            byte[] salt = Convert.FromBase64String(parts[1]);
            byte[] storedHashBytes = Convert.FromBase64String(parts[2]);

            Console.WriteLine($"Iteraciones: {iterations}");
            Console.WriteLine($"Salt (base64): {parts[1]}");
            Console.WriteLine($"StoredHash (base64): {parts[2]}");

            // Calcular hash con UTF-8
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            Console.WriteLine($"Password bytes length: {passwordBytes.Length}");
            Console.WriteLine($"Password bytes (hex): {BitConverter.ToString(passwordBytes).Replace("-", "")}");

            using var pbkdf2 = new Rfc2898DeriveBytes(passwordBytes, salt, iterations, HashAlgorithmName.SHA256);
            byte[] computedHash = pbkdf2.GetBytes(storedHashBytes.Length);
            string computedHashBase64 = Convert.ToBase64String(computedHash);

            Console.WriteLine($"ComputedHash (base64): {computedHashBase64}");
            Console.WriteLine($"StoredHash (base64): {parts[2]}");
            Console.WriteLine($"¿Coinciden? {computedHashBase64 == parts[2]}");

            bool result = CryptographicOperations.FixedTimeEquals(computedHash, storedHashBytes);
            Console.WriteLine($"FixedTimeEquals result: {result}");
            Console.WriteLine($"=== END VERIFY ===");

            return result;
        }
    }
}