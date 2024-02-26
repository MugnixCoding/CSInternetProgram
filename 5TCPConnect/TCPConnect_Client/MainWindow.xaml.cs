using System;
using System.Collections.Generic;
using System.Windows;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;

namespace TCPConnect_Client
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        Socket clientSocket;
        string userName;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Connect_button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(ServerIP_textbox.Text) || string.IsNullOrEmpty(ServerPort_textbox.Text)||
                string.IsNullOrEmpty(UserName_textbox.Text))
            {
                return;
            }
            string ip = ServerIP_textbox.Text;
            int port = int.Parse(ServerPort_textbox.Text);
            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            userName = UserName_textbox.Text;
            try
            {
                clientSocket.Connect(ipEndPoint);
                Send("0" + userName);
            }
            catch(Exception ex)
            {
                MessageBox.Show("無法連接伺服器");
                return;
            }
            Connect_button.IsEnabled = false;
        }
        private void Send(string str)
        {
            byte[] messageByte = Encoding.Default.GetBytes(str);
            clientSocket.Send(messageByte, 0, messageByte.Length, SocketFlags.None);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (Connect_button.IsEnabled==false)
            {
                Send("9" + userName);
                clientSocket.Close();
            }
        }
    }
}
