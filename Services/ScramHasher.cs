using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;

namespace Balance.API.Services
{
    public class ScramHasher
    {
        private const int SaltSize = 16;      // 128 bits
        private const int HashSize = 32;      // 256 bits
        private const int Iterations = 10000; // Número de iteraciones

        public static string HashPassword(string password)
        {
            // Generar salt aleatorio
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);

            // Generar hash usando PBKDF2 (similar a SCRAM)
            byte[] hash = KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: Iterations,
                numBytesRequested: HashSize
            );

            // Combinar salt + hash en un solo string (formato: salt:hash)
            string saltBase64 = Convert.ToBase64String(salt);
            string hashBase64 = Convert.ToBase64String(hash);

            return $"{Iterations}:{saltBase64}:{hashBase64}";
        }

        public static bool VerifyPassword(string password, string storedHash)
        {
            // Dividir el storedHash en partes
            var parts = storedHash.Split(':');
            if (parts.Length != 3)
                return false;

            if (!int.TryParse(parts[0], out int iterations))
                return false;

            byte[] salt = Convert.FromBase64String(parts[1]);
            byte[] storedHashBytes = Convert.FromBase64String(parts[2]);

            // Calcular hash con los mismos parámetros
            byte[] computedHash = KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: iterations,
                numBytesRequested: storedHashBytes.Length
            );

            // Comparar hashes de manera segura (tiempo constante)
            return CryptographicOperations.FixedTimeEquals(computedHash, storedHashBytes);
        }
    }
}