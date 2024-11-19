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
        static void Main(string[] args)
        {
            Console.WriteLine("VPN SERVER");

            // Load the server's SSL certificate
            X509Certificate2 serverCertificate = new X509Certificate2("C:\\Users\\Lenovo\\source\\repos\\VPNServer\\VPNServer\\vpnservercert.pfx", "mypassword");

            // Set up TCP listener
            TcpListener listener = new TcpListener(IPAddress.Any, 5000);
            listener.Start();
            Console.WriteLine("Server started. Waiting for client...");

            // Accept client connection
            TcpClient client = listener.AcceptTcpClient();
            Console.WriteLine("Client connected.");

            // Get network stream and create SslStream on top of it
            NetworkStream networkStream = client.GetStream();
            SslStream sslStream = new SslStream(networkStream, false);

            try
            {
                // Authenticate the server using the certificate
                sslStream.AuthenticateAsServer(serverCertificate, clientCertificateRequired: false, checkCertificateRevocation: false);
                Console.WriteLine("SSL/TLS connection established.");

                // From this point, use sslStream instead of networkStream
                HandleClient(sslStream);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Authentication failed: " + ex.Message);
                client.Close();
                return;
            }

            listener.Stop();
        }
        static void HandleClient(SslStream sslStream)
        {
            // Your existing code goes here to handle encrypted communication using sslStream.
            // For example:
            byte[] buffer = new byte[2048];
            int bytesRead = sslStream.Read(buffer, 0, buffer.Length);
            string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Console.WriteLine("Received: " + receivedMessage);

            // Close the connection
            sslStream.Close();
        }
    }
}