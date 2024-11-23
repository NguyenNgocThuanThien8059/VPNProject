using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Security;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VPNServer.GUI
{
    public partial class MainForm : Form
    {
        private TcpListener listener;
        private static Dictionary<string, string> users = new Dictionary<string, string>();
        private string certificateFilePath = "C:\\Users\\Lenovo\\serverCert.pfx"; // Path to your server certificate
        private string certificatePassword = "mypassword"; // Password for the certificate
        private Socket rawSocket;
        public MainForm()
        {
            InitializeComponent();
        }
        private async void StartServerButton_Click(object sender, EventArgs e)
        {
            int port = 12345;
            listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            UpdateStatus("VPN Server started... Waiting for client connection...");

            // Load the certificate
            X509Certificate2 serverCertificate = new X509Certificate2(certificateFilePath, certificatePassword);

            // Add a user (for testing)
            string username = "testuser";
            string password = "password123"; // This is the user's password
            string hashedPassword = HashPassword(password); // Store the hashed password
            users.Add(username, hashedPassword); // Add to dictionary (user list)

            // Initialize raw socket for packet handling
            InitializeRawSocket();

            while (true)
            {
                TcpClient client = await listener.AcceptTcpClientAsync(); // Use async method to accept client
                UpdateStatus("Client connected!");

                // Use SSL/TLS
                SslStream sslStream = new SslStream(client.GetStream(), false);
                await sslStream.AuthenticateAsServerAsync(serverCertificate, clientCertificateRequired: false, checkCertificateRevocation: true);

                // Handle authentication
                await HandleClientAuthentication(sslStream);

                client.Close();
            }
        }
        // Initialize raw socket for capturing and sending packets
        private void InitializeRawSocket()
        {
            try
            {
                rawSocket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.IP);
                rawSocket.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 0)); // Use a specific IP address
                rawSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.HeaderIncluded, true);

                byte[] inValue = BitConverter.GetBytes(1);
                byte[] outValue = new byte[inValue.Length];
                rawSocket.IOControl(IOControlCode.ReceiveAll, inValue, outValue); // Set socket to receive all packets

                // Start listening for packets
                Task.Run(() => StartPacketCapture());
            }
            catch (SocketException ex)
            {
                UpdateStatus($"SocketException: {ex.Message}");
            }
        }

        // Method to capture incoming packets
        private void StartPacketCapture()
        {
            byte[] buffer = new byte[4096];
            while (true)
            {
                int received = rawSocket.Receive(buffer);
                string packetData = BitConverter.ToString(buffer, 0, received);
                UpdateStatus("Captured packet: " + packetData);
            }
        }
        // Handle client authentication asynchronously
        private async Task HandleClientAuthentication(SslStream sslStream)
        {
            // Receive encrypted username and password asynchronously
            byte[] buffer = new byte[1024];
            int bytesRead = await sslStream.ReadAsync(buffer, 0, buffer.Length);
            string encryptedCredentials = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            // Decrypt the received data
            string decryptedCredentials = EncryptionHelper.Decrypt(encryptedCredentials);
            string[] parts = decryptedCredentials.Split(':');
            if (parts.Length == 2)
            {
                string usernameFromClient = parts[0];
                string passwordFromClient = parts[1];
                if (AuthenticateUser(usernameFromClient, passwordFromClient))
                {
                    byte[] encryptedResponse = Encoding.UTF8.GetBytes(EncryptionHelper.Encrypt("Authentication Successful"));
                    await sslStream.WriteAsync(encryptedResponse, 0, encryptedResponse.Length);
                }
                else
                {
                    byte[] encryptedResponse = Encoding.UTF8.GetBytes(EncryptionHelper.Encrypt("Authentication Failed"));
                    await sslStream.WriteAsync(encryptedResponse, 0, encryptedResponse.Length);
                }
            }
        }

        // Update the status message on the form
        private void UpdateStatus(string message)
        {
            StatusLabel.Invoke((Action)(() => StatusLabel.Text = message));
        }

        // Hash password using SHA-256
        private static string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (var b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        // Authenticate user
        private static bool AuthenticateUser(string username, string password)
        {
            if (users.ContainsKey(username))
            {
                string storedHashedPassword = users[username];
                string hashedPasswordFromClient = HashPassword(password);
                return storedHashedPassword == hashedPasswordFromClient;
            }
            return false;
        }
        private void MainForm_Load(object sender, EventArgs e)
        {

        }
    }
}
