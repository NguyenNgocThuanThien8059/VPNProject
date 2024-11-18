using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace VPNClient
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("VPN CLIENT");
            // Setup client encryption manager
            ClientEncryptionManager clientEncryptionManager = new ClientEncryptionManager();

            // Connect to the server
            TcpClient client = new TcpClient();
            client.Connect("127.0.0.1", 5000);
            Console.WriteLine("Connected to server.");

            // Get network stream to communicate with the server
            NetworkStream stream = client.GetStream();

            // 1. Receive the server's public RSA key
            byte[] buffer = new byte[2048];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            string serverPublicKey = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            clientEncryptionManager.SetServerPublicKey(serverPublicKey);
            Console.WriteLine("Received server's public key.");

            // 2. Encrypt AES key using the server's public RSA key
            byte[] encryptedAESKey = clientEncryptionManager.EncryptAESKey();
            byte[] iv = clientEncryptionManager.GetIV();

            // Send encrypted AES key and IV to server
            stream.Write(encryptedAESKey, 0, encryptedAESKey.Length);
            stream.Write(iv, 0, iv.Length);
            Console.WriteLine("Encrypted AES key and IV sent to server.");

            // 3. Encrypt a message to send to the server
            string message;
            Console.Write("Enter message:");
            message = Console.ReadLine();
            byte[] encryptedMessage = clientEncryptionManager.EncryptData(message);

            // Send encrypted message to server
            stream.Write(encryptedMessage, 0, encryptedMessage.Length);
            Console.WriteLine("Encrypted message sent to server.");

            // Close connection
            client.Close();
            Console.WriteLine("Client closed.");
        }
    }
}
