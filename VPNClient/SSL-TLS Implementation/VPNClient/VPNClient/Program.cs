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
        static void Main(string[] args)
        {
            Console.WriteLine("VPN CLIENT");
            
            // Connect to the server
            TcpClient client = new TcpClient();
            client.Connect("127.0.0.1", 5000);
            Console.WriteLine("Connected to server.");

            // Get network stream and create SslStream on top of it
            NetworkStream networkStream = client.GetStream();
            SslStream sslStream = new SslStream(networkStream, false,
                new RemoteCertificateValidationCallback(ValidateServerCertificate), null);

            try
            {
                // Authenticate the client (can also pass a client certificate here if needed)
                sslStream.AuthenticateAsClient("localhost");
                Console.WriteLine("SSL/TLS connection established.");

                // From this point, use sslStream instead of networkStream
                SendMessage(sslStream);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Authentication failed: " + ex.Message);
                client.Close();
                return;
            }

            client.Close();
        }
        static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            // For development purposes, accept any server certificate
            // In production, you should validate the server certificate properly
            return true;
        }

        static void SendMessage(SslStream sslStream)
        {
            // Get user input message
            Console.Write("Enter message to send securely to the server: ");
            string message = Console.ReadLine();
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);

            // Send message to server using the SslStream
            sslStream.Write(messageBytes);
            Console.WriteLine("Message sent securely.");
        }
    }
}
