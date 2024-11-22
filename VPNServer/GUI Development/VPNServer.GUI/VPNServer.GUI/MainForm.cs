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

            while (true)
            {
                TcpClient client = await listener.AcceptTcpClientAsync(); // Use async method to accept client
                UpdateStatus("Client connected!");

                // Use SSL/TLS
                SslStream sslStream = new SslStream(client.GetStream(), false);
                await sslStream.AuthenticateAsServerAsync(serverCertificate, clientCertificateRequired: false, checkCertificateRevocation: true);

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

                client.Close();
            }
        }

        // Update the status message on the form
        private void UpdateStatus(string message)
        {
            StatusLabel.Text = message;
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
