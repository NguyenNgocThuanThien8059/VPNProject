using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace VPNServer
{
    public class ServerEncryptionManager
    {
        private Aes aes;
        private RSACryptoServiceProvider rsa;
        private string publicKey;

        public ServerEncryptionManager()
        {
            // Initialize AES
            aes = Aes.Create();
            aes.KeySize = 256;

            // Initialize RSA with RSACryptoServiceProvider
            rsa = new RSACryptoServiceProvider(2048); // Set key size for RSA

            // Generate public RSA key as an XML string
            publicKey = rsa.ToXmlString(false); // false for public key only
        }

        // Method to get the public RSA key in XML format
        public string GetPublicKey()
        {
            return publicKey;
        }

        // Decrypt the AES key received from the client
        public void SetAESKey(byte[] encryptedKey, byte[] iv)
        {
            // Use RSA to decrypt the AES key
            byte[] decryptedKey = rsa.Decrypt(encryptedKey, false);

            // Set the AES key and IV
            aes.Key = decryptedKey;
            aes.IV = iv;
        }

        // Decrypt data using AES
        public string DecryptData(byte[] encryptedData)
        {
            using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
            {
                byte[] decryptedBytes = decryptor.TransformFinalBlock(encryptedData, 0, encryptedData.Length);
                return System.Text.Encoding.UTF8.GetString(decryptedBytes);
            }
        }
    }
}
