using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace VPNServer
{
    public static class PasswordHasher
    {
        // Hash password with SHA256 for simplicity (you can use PBKDF2 for more security)
        public static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] salt = GenerateSalt();
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                byte[] saltedPassword = new byte[salt.Length + passwordBytes.Length];
                Buffer.BlockCopy(salt, 0, saltedPassword, 0, salt.Length);
                Buffer.BlockCopy(passwordBytes, 0, saltedPassword, salt.Length, passwordBytes.Length);

                byte[] hashBytes = sha256.ComputeHash(saltedPassword);

                // Combine salt and hash for storage
                byte[] hashWithSalt = new byte[salt.Length + hashBytes.Length];
                Buffer.BlockCopy(salt, 0, hashWithSalt, 0, salt.Length);
                Buffer.BlockCopy(hashBytes, 0, hashWithSalt, salt.Length, hashBytes.Length);

                return Convert.ToBase64String(hashWithSalt);
            }
        }

        // Verify password with stored hash
        public static bool VerifyPassword(string password, string storedHash)
        {
            byte[] hashWithSaltBytes = Convert.FromBase64String(storedHash);
            byte[] salt = new byte[16];
            Buffer.BlockCopy(hashWithSaltBytes, 0, salt, 0, 16);

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                byte[] saltedPassword = new byte[salt.Length + passwordBytes.Length];
                Buffer.BlockCopy(salt, 0, saltedPassword, 0, salt.Length);
                Buffer.BlockCopy(passwordBytes, 0, saltedPassword, salt.Length, passwordBytes.Length);

                byte[] hashBytes = sha256.ComputeHash(saltedPassword);

                // Compare computed hash with stored hash
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    if (hashBytes[i] != hashWithSaltBytes[16 + i])
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        // Generate a random salt
        private static byte[] GenerateSalt()
        {
            byte[] salt = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }
    }
}
