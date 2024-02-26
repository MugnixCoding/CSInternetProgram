using System;
using System.Collections.Generic;
using System.Windows;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections;
using System.Text;
using System.Diagnostics;
using System.Xml.Linq;

namespace TCPConnect_Server
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        TcpListener server;
        Socket client;
        Thread serverThread;
        Thread clientThread;
        Hashtable clientSocketHashtable;
        public MainWindow()
        {
            InitializeComponent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            clientSocketHashtable = new Hashtable();
        }

        private void Window_Closed(object sender, EventArgs e)
        {

        }

        private void ServerSwitch_button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(ServerIP_textbox.Text) || string.IsNullOrEmpty(ServerPort_textbox.Text))
            {
                return;
            }
            string ip = ServerIP_textbox.Text;
            string port = ServerPort_textbox.Text;
            serverThread = new Thread(()=>ServerSub(ip,port ));
            serverThread.IsBackground = true;
            serverThread.Start();
            ServerSwitch_button.IsEnabled = false;
        }
        private void JoinOnlineUser(string name)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                OnlineUser_listbox.Items.Add(name);
            }));
        }
        private void RemoveOnlineUser(string name)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                OnlineUser_listbox.Items.Remove(name);
            }));
        }

        private void ServerSub(string ip,string port)
        {
            //server ip and port
            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(ip),int.Parse(port));
            server = new TcpListener(ipEndPoint);
            server.Start(100);//Max connecting number
            while (true)
            {
                client = server.AcceptSocket();
                clientThread = new Thread(Listen);
                clientThread.IsBackground = true;
                clientThread.Start();
            }
        }
        private void Listen()
        {
            Socket newSocket = client;
            Thread newThread = clientThread;
            while (true)
            {
                try
                {
                    byte[] recieveByte = new byte[1023];
                    int recieveLength = newSocket.Receive(recieveByte);
                    string recieveString = Encoding.Default.GetString(recieveByte, 0, recieveLength);

                    string cmd = recieveString.Substring(0, 1);
                    string clientName = recieveString.Substring(1);
                    switch (cmd)
                    {
                        case "0":
                            clientSocketHashtable.Add(clientName, newSocket);
                            JoinOnlineUser(clientName);
                            break;
                        case "9":
                            clientSocketHashtable.Remove(clientName);
                            RemoveOnlineUser(clientName);
                            newThread.Abort();
                            break;
                    }
                }
                catch(Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }
    }
}
