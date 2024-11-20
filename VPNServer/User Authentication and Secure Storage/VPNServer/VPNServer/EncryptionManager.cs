using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace VPNServer
{
    public class EncryptionManager
    {
        private RSACryptoServiceProvider rsa;
        public EncryptionManager()
        {
            // Generate a new 2048-bit RSA key pair
            rsa = new RSACryptoServiceProvider(2048);
        }
        public string GetPublicKey()
        {
            return rsa.ToXmlString(false); // false = public key only
        }
        public byte[] DecryptData(byte[] encryptedData)
        {
            return rsa.Decrypt(encryptedData, false);
        }
    }
}
