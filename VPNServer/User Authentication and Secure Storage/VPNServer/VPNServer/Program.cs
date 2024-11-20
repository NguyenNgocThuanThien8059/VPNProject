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
using System.Security.Cryptography;

namespace VPNServer
{
    internal class Program
    {
        // Simulated user database
        static Dictionary<string, string> userDatabase = new Dictionary<string, string>()
        {
            // Pre-hashed passwords for demonstration purposes
            { "user1", HashPassword("password123", "randomSalt1") },
            { "user2", HashPassword("secret456", "randomSalt2") }
        };
        static async Task Main(string[] args)
        {
            Console.WriteLine("VPN SERVER with Authentication");

            // Start TCP Listener
            TcpListener listener = new TcpListener(IPAddress.Any, 5000);
            listener.Start();
            Console.WriteLine("Server started and listening for connections...");

            while (true)
            {
                TcpClient client = await listener.AcceptTcpClientAsync();
                Console.WriteLine("Client connected.");

                NetworkStream stream = client.GetStream();
                await HandleClientConnection(stream);
            }
        }
        // Hashing function using SHA256
        public static string HashPassword(string password, string salt)
        {
            using (var sha256 = SHA256.Create())
            {
                var combinedBytes = Encoding.UTF8.GetBytes(password + salt);
                var hashedBytes = sha256.ComputeHash(combinedBytes);
                return Convert.ToBase64String(hashedBytes);
            }
        }
        // Handling client connection and authentication
        static async Task HandleClientConnection(NetworkStream stream)
        {
            byte[] buffer = new byte[1024];
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            string credentials = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            string[] parts = credentials.Split(':');
            if (parts.Length != 2)
            {
                Console.WriteLine("Invalid credentials format received.");
                await SendResponse(stream, "AUTH_FAILED");
                return;
            }

            string username = parts[0];
            string password = parts[1];

            Console.WriteLine($"Received credentials: Username = {username}");

            // Check if user exists and verify the hashed password
            if (userDatabase.ContainsKey(username) && userDatabase[username] == password)
            {
                Console.WriteLine("Authentication successful.");
                await SendResponse(stream, "AUTH_SUCCESS");
            }
            else
            {
                Console.WriteLine("Authentication failed. Disconnecting client.");
                await SendResponse(stream, "AUTH_FAILED");
            }

            stream.Close();
        }
        // Send response to the client
        static async Task SendResponse(NetworkStream stream, string message)
        {
            byte[] responseBytes = Encoding.UTF8.GetBytes(message);
            await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
        }
    }
}