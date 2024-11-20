using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Net.Sockets;


namespace VPNClient
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("VPN CLIENT with Authentication");

            // Ask user for credentials
            Console.Write("Enter username: ");
            string username = Console.ReadLine();
            Console.Write("Enter password: ");
            string password = Console.ReadLine();

            // Hash the password before sending
            string hashedPassword = HashPassword(password, "randomSalt2");
            string credentials = $"{username}:{hashedPassword}";

            // Connect to the server
            TcpClient client = new TcpClient();
            await client.ConnectAsync("127.0.0.1", 5000);
            Console.WriteLine("Connected to server.");

            NetworkStream stream = client.GetStream();

            // Send credentials to the server
            byte[] credentialsBytes = Encoding.UTF8.GetBytes(credentials);
            await stream.WriteAsync(credentialsBytes, 0, credentialsBytes.Length);

            // Wait for server response
            byte[] buffer = new byte[1024];
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Console.WriteLine($"Server response: {response}");

            if (response == "AUTH_SUCCESS")
            {
                Console.WriteLine("Authenticated successfully!");
            }
            else
            {
                Console.WriteLine("Authentication failed.");
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
    }
}
