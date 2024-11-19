using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace VPNClient
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Asynchronous VPN CLIENT with SSL/TLS and Encryption");

            // Setup client encryption manager
            ClientEncryptionManager clientEncryptionManager = new ClientEncryptionManager();

            // Connect to the server
            using (TcpClient client = new TcpClient())
            {
                await client.ConnectAsync("127.0.0.1", 5000);
                Console.WriteLine("Connected to server.");

                using (NetworkStream networkStream = client.GetStream())
                using (SslStream sslStream = new SslStream(networkStream, false, ValidateServerCertificate))
                {
                    try
                    {
                        // Authenticate the client asynchronously
                        await sslStream.AuthenticateAsClientAsync("VPNServer");
                        Console.WriteLine("SSL/TLS connection established.");

                        // 1. Receive the server's public RSA key
                        byte[] buffer = new byte[2048];
                        int bytesRead = await sslStream.ReadAsync(buffer, 0, buffer.Length);
                        string serverPublicKey = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        clientEncryptionManager.SetServerPublicKey(serverPublicKey);
                        Console.WriteLine("Received server's public key.");

                        // 2. Encrypt AES key using the server's public RSA key
                        byte[] encryptedAESKey = clientEncryptionManager.EncryptAESKey();
                        byte[] iv = clientEncryptionManager.GetIV();

                        // Send encrypted AES key and IV to server
                        await sslStream.WriteAsync(encryptedAESKey, 0, encryptedAESKey.Length);
                        await sslStream.WriteAsync(iv, 0, iv.Length);
                        Console.WriteLine("Encrypted AES key and IV sent to server.");

                        // 3. Encrypt and send a user-input message
                        Console.Write("Enter the message you want to send: ");
                        string message = Console.ReadLine();
                        byte[] encryptedMessage = clientEncryptionManager.EncryptData(message);

                        // Send encrypted message to server
                        await sslStream.WriteAsync(encryptedMessage, 0, encryptedMessage.Length);
                        Console.WriteLine("Encrypted message sent to server.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Connection or communication failed: " + ex.Message);
                    }
                }
            }
        }
        // Optional validation of server certificate (can be modified as needed)
        private static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true; // Accept all certificates (not recommended for production)
        }
    }
}
