using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VPNClient.GUI
{
    public partial class MainForm : Form
    {
        private Socket rawSocket; // Raw socket for packet handling

        public MainForm()
        {
            InitializeComponent();
        }
        private async void ConnectButton_Click(object sender, EventArgs e)
        {
            string serverIp = ServerIpTextBox.Text;
            string username = UsernameTextBox.Text;
            string password = PasswordTextBox.Text;

            TcpClient client = new TcpClient();
            await client.ConnectAsync(serverIp, 12345);
            StatusLabel.Text = "Connected to VPN Server!";

            // Use SSL/TLS
            SslStream sslStream = new SslStream(client.GetStream(), false,
                new RemoteCertificateValidationCallback(ValidateServerCertificate), null);
            await sslStream.AuthenticateAsClientAsync(serverIp);

            // Encrypt credentials
            string credentials = username + ":" + password;
            string encryptedCredentials = EncryptionHelper.Encrypt(credentials);
            byte[] data = Encoding.UTF8.GetBytes(encryptedCredentials);
            await sslStream.WriteAsync(data, 0, data.Length);

            // Receive encrypted response from the server
            byte[] buffer = new byte[1024];
            int bytesRead = await sslStream.ReadAsync(buffer, 0, buffer.Length);
            string encryptedResponse = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            // Decrypt the response
            string response = EncryptionHelper.Decrypt(encryptedResponse);
            ServerResponseLabel.Text = "Server response: " + response;

            client.Close();

            // Initialize raw socket for sending packets
            InitializeRawSocket();
        }
        // Initialize raw socket for sending custom packets
        private void InitializeRawSocket()
        {
            rawSocket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.IP);
            rawSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.HeaderIncluded, true);

            // Example of sending a custom packet
            Task.Run(() => SendCustomPacket("Test data from client"));
        }
        // Method to send custom packets to the server
        private void SendCustomPacket(string data)
        {
            byte[] packetData = Encoding.UTF8.GetBytes(data);
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 0);
            rawSocket.SendTo(packetData, endPoint);
        }

        // Validate the server certificate
        public static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true; // Bypass certificate validation for testing
        }
        private void MainForm_Load(object sender, EventArgs e)
        {

        }
    }
}
