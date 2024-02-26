using System;
using System.Collections.Generic;
using System.Windows;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Text;

namespace UDPServer
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        Thread listenThread;
        const short Port = 2019;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            listenThread = new Thread(Listen);
            listenThread.IsBackground = true;
            listenThread.Start();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            listenThread.Abort();
        }
        private void Listen()
        {
            UdpClient udpClient = new UdpClient(Port);
            while (true)
            {
                IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, Port);
                byte[] messageByte = udpClient.Receive(ref ipEndPoint);
                string message = Encoding.Default.GetString(messageByte);
                string returnMessage = "Unkown Command";
                if (message == "Time?")
                {
                    returnMessage = DateTime.Now.ToString();
                }
                messageByte = Encoding.Default.GetBytes(returnMessage);
                udpClient.Send(messageByte, messageByte.Length, ipEndPoint);
            }
        }
    }
}
