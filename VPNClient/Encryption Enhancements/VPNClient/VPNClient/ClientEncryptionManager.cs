using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace VPNClient
{
    public class ClientEncryptionManager
    {
        private RSACryptoServiceProvider rsa;
        private Aes aes;
        public ClientEncryptionManager()
        {
            // Initialize RSA for public key encryption
            rsa = new RSACryptoServiceProvider();

            // Initialize AES for symmetric encryption
            aes = Aes.Create();
            aes.KeySize = 256;
            aes.GenerateKey(); // Generate random AES key
            aes.GenerateIV();  // Generate Initialization Vector (IV)
        }
        // Set the public key received from the server
        public void SetServerPublicKey(string publicKeyXml)
        {
            rsa.FromXmlString(publicKeyXml);
        }
        // Encrypt the AES key using the server's public RSA key
        public byte[] EncryptAESKey()
        {
            return rsa.Encrypt(aes.Key, false);
        }
        // Get the AES IV (Initialization Vector) to send to the server
        public byte[] GetIV()
        {
            return aes.IV;
        }
        // Encrypt data using AES
        public byte[] EncryptData(string plainText)
        {
            using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
            {
                byte[] plainBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
                return encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
            }
        }

    }
}
