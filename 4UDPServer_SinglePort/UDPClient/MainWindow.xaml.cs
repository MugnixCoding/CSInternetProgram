using System;
using System.Collections.Generic;
using System.Windows;
using System.Net.Sockets;
using System.Net;
using System.Text;

namespace UDPClient
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        const string ConnectIP = "127.0.0.1";
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Send_button_Click(object sender, RoutedEventArgs e)
        {
            UdpClient udpClient = new UdpClient();
            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(ConnectIP), 2019);

            udpClient.Connect(ipEndPoint);
            byte[] messageByte = Encoding.Default.GetBytes(Command_textbox.Text);
            udpClient.Send(messageByte, messageByte.Length);
            byte[] reciveByte = udpClient.Receive(ref ipEndPoint);
            Recieve_textbox.Text = Encoding.Default.GetString(reciveByte);
            
        }
    }
}
