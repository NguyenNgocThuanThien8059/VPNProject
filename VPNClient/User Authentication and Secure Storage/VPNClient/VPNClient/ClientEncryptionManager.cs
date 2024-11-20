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
        private Aes aes;
        private RSACryptoServiceProvider rsa;

        public ClientEncryptionManager()
        {
            // Initialize AES
            aes = Aes.Create();
            aes.KeySize = 256;

            // Initialize RSA
            rsa = new RSACryptoServiceProvider(2048);
        }

        // Set the server's public RSA key from XML
        public void SetServerPublicKey(string publicKeyXml)
        {
            rsa.FromXmlString(publicKeyXml);
        }

        // Encrypt the AES key using the server's RSA public key
        public byte[] EncryptAESKey()
        {
            return rsa.Encrypt(aes.Key, false);
        }

        // Get IV for AES
        public byte[] GetIV()
        {
            return aes.IV;
        }

        // Encrypt data using AES
        public byte[] EncryptData(string data)
        {
            using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
            {
                byte[] dataBytes = System.Text.Encoding.UTF8.GetBytes(data);
                return encryptor.TransformFinalBlock(dataBytes, 0, dataBytes.Length);
            }
        }
    }
}
