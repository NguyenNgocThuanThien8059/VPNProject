using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace VPNServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("VPN SERVER");
            // Setup server encryption managers
            EncryptionManager encryptionManager = new EncryptionManager();
            ServerEncryptionManager secureCommServer = new ServerEncryptionManager();

            // Setup TCP listener for client connections
            TcpListener listener = new TcpListener(IPAddress.Any, 5000);
            listener.Start();
            Console.WriteLine("Server started. Waiting for client...");

            // Accept client connection
            TcpClient client = listener.AcceptTcpClient();
            Console.WriteLine("Client connected.");

            // Get network stream to communicate with the client
            NetworkStream stream = client.GetStream();

            // 1. Send the server's public RSA key to the client
            string publicKey = encryptionManager.GetPublicKey();
            byte[] publicKeyBytes = Encoding.UTF8.GetBytes(publicKey);
            stream.Write(publicKeyBytes, 0, publicKeyBytes.Length);
            Console.WriteLine("Server's public key sent to client.");

            // 2. Receive the encrypted AES key and IV from the client
            byte[] buffer = new byte[256];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            byte[] encryptedAESKey = new byte[bytesRead];
            Array.Copy(buffer, 0, encryptedAESKey, 0, bytesRead);

            // Receive IV (Initialization Vector)
            bytesRead = stream.Read(buffer, 0, buffer.Length);
            byte[] iv = new byte[bytesRead];
            Array.Copy(buffer, 0, iv, 0, bytesRead);

            // 3. Decrypt the AES key
            secureCommServer.SetAESKey(encryptedAESKey, iv, encryptionManager);
            Console.WriteLine("AES key received and decrypted.");

            // 4. Receive encrypted message from the client
            bytesRead = stream.Read(buffer, 0, buffer.Length);
            byte[] encryptedMessage = new byte[bytesRead];
            Array.Copy(buffer, 0, encryptedMessage, 0, bytesRead);

            // 5. Decrypt the received message
            string decryptedMessage = secureCommServer.DecryptData(encryptedMessage);
            Console.WriteLine("Decrypted message from client: " + decryptedMessage);

            // Close connection
            client.Close();
            listener.Stop();
            Console.WriteLine("Server closed.");
        }
    }
}