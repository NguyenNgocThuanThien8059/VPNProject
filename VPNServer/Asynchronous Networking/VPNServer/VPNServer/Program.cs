using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace VPNServer
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Asynchronous VPN SERVER with SSL/TLS and Encryption");

            // Initialize the ServerEncryptionManager
            ServerEncryptionManager encryptionManager = new ServerEncryptionManager();

            TcpListener listener = new TcpListener(IPAddress.Any, 5000);
            listener.Start();
            Console.WriteLine("Server listening on port 5000...");

            while (true)
            {
                // Accept client connection asynchronously
                TcpClient client = await listener.AcceptTcpClientAsync();
                Console.WriteLine("Client connected.");

                _ = Task.Run(async () =>
                {
                    using (NetworkStream networkStream = client.GetStream())
                    using (SslStream sslStream = new SslStream(networkStream, false, null))
                    {
                        try
                        {
                            // Authenticate server asynchronously
                            X509Certificate2 serverCertificate = new X509Certificate2("C:\\Users\\Lenovo\\source\\repos\\VPNServer\\VPNServer\\vpnservercert.pfx", "mypassword");
                            await sslStream.AuthenticateAsServerAsync(serverCertificate);

                            Console.WriteLine("SSL/TLS connection established.");

                            // 1. Send server's public RSA key to client
                            byte[] publicKeyBytes = Encoding.UTF8.GetBytes(encryptionManager.GetPublicKey());
                            await sslStream.WriteAsync(publicKeyBytes, 0, publicKeyBytes.Length);
                            Console.WriteLine("Sent server's public RSA key to client.");

                            // 2. Receive encrypted AES key and IV from client
                            byte[] encryptedAESKey = new byte[256]; // Size for RSA-encrypted AES key
                            int aesKeyLength = await sslStream.ReadAsync(encryptedAESKey, 0, encryptedAESKey.Length);

                            byte[] iv = new byte[16]; // 16 bytes for AES IV
                            int ivLength = await sslStream.ReadAsync(iv, 0, iv.Length);

                            encryptionManager.SetAESKey(encryptedAESKey, iv);
                            Console.WriteLine("Received and decrypted AES key and IV.");

                            // 3. Receive and decrypt the encrypted message from client
                            byte[] buffer = new byte[4096];
                            int bytesRead = await sslStream.ReadAsync(buffer, 0, buffer.Length);
                            byte[] encryptedMessage = new byte[bytesRead];
                            Array.Copy(buffer, encryptedMessage, bytesRead);

                            string decryptedMessage = encryptionManager.DecryptData(encryptedMessage);
                            Console.WriteLine($"Decrypted message from client: {decryptedMessage}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error: " + ex.Message);
                        }
                        finally
                        {
                            client.Close();
                        }
                    }
                });
            }
        }
    }
}