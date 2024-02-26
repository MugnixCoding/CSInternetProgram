using System.Windows;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Windows.Controls;
using System;

namespace UDPIM
{
    /// <summary>
    /// UDP Internet Messaging
    /// </summary>
    public partial class MainWindow : Window
    {
        UdpClient udpClient;
        Thread listenThread;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Title += " - " + LocalIP();
        }
        #region Function

        private void Listen()
        {
            int port = 0;
            this.Dispatcher.Invoke((Action)(() =>
            {
                port = int.Parse(ReceivePort_textbox.Text);
                udpClient = new UdpClient(port);
            }));
            //Create a local endpoinrt
            IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);
            while (true)
            {
                byte[] message = udpClient.Receive(ref iPEndPoint);
                this.Dispatcher.Invoke((Action)(() =>
                {
                    ReceiveText_textbox.Text = Encoding.Default.GetString(message);
                }));
            }
        }

        private string LocalIP()
        {
            string hn = Dns.GetHostName();
            IPAddress[] ip = Dns.GetHostEntry(hn).AddressList;
            foreach(IPAddress it in ip)
            {
                if (it.AddressFamily == AddressFamily.InterNetwork)
                {
                    return it.ToString();
                }
            }
            return "";
        }

        #endregion

        #region Active
        private void SendText_button_Click(object sender, RoutedEventArgs e)
        {
            string ip = TargetIP_textbox.Text;
            int port = int.Parse(TargetPort_textbox.Text);
            byte[] message = Encoding.Default.GetBytes(SendText_textbox.Text);
            UdpClient targetClient = new UdpClient();
            targetClient.Send(message, message.Length, ip, port);
            targetClient.Close();
        }

        private void StartReceive_button_Click(object sender, RoutedEventArgs e)
        {
            listenThread = new Thread(Listen);
            listenThread.Start();
            StartReceive_button.IsEnabled = false;
        }
        #endregion

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (listenThread!=null)
            {
                listenThread.Abort();
            }
            if (udpClient!=null)
            {
                udpClient.Close();
            }
        }
    }
}
