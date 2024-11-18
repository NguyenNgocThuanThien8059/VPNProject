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
        public ServerEncryptionManager()
        {
            aes = Aes.Create();
            aes.KeySize = 256;
        }
        // Decrypt the AES key received from the client
        public void SetAESKey(byte[] encryptedKey, byte[] iv, EncryptionManager encryptionManager)
        {
            aes.Key = encryptionManager.DecryptData(encryptedKey);
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
