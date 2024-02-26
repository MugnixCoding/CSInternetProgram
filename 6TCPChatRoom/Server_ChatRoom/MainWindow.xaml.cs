using System;
using System.Collections;
using System.Diagnostics;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows;

namespace Server_ChatRoom
{
    /// <summary>
    /// TCP Chat Room Server
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
            ServerIP_textbox.Text = LocalIP();
            ServerPort_textbox.Text = "2019";
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
            serverThread = new Thread(() => ServerSub(ip, port));
            serverThread.IsBackground = true;
            serverThread.Start();
            ServerSwitch_button.IsEnabled = false;
        }
        private string LocalIP()
        {
            string hn = Dns.GetHostName();
            IPAddress[] ip = Dns.GetHostEntry(hn).AddressList;
            foreach(IPAddress it in ip)
            {
                if(it.AddressFamily == AddressFamily.InterNetwork)
                {
                    return it.ToString();
                }
            }
            return "";
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

        private void ServerSub(string ip, string port)
        {
            //server ip and port
            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(ip), int.Parse(port));
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

                    string cmd = recieveString.Substring(0, 1);//the first char should be a self define cmd code
                    string recieveMessage = recieveString.Substring(1);
                    switch (cmd)
                    {
                        case "0":
                            clientSocketHashtable.Add(recieveMessage, newSocket);
                            JoinOnlineUser(recieveMessage);
                            SendAll(OnlineList());
                            break;
                        case "1":
                            SendAll(recieveMessage);
                            break;
                        case "9":
                            clientSocketHashtable.Remove(recieveMessage);
                            RemoveOnlineUser(recieveMessage);
                            SendAll(OnlineList());

                            newThread.Abort();
                            break;
                        default:
                            string[] cutMessage = recieveMessage.Split('|');
                            SendTo(cmd + cutMessage[0], cutMessage[1]);//[0] is message , [1] is reciever
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }

        private void SendTo(string message,string User)
        {
            byte[] messageByte = Encoding.Default.GetBytes(message);
            Socket sck = (Socket)clientSocketHashtable[User];
            sck.Send(messageByte, 0, messageByte.Length, SocketFlags.None);
        }
        private void SendAll(string message)
        {
            byte[] messageByte = Encoding.Default.GetBytes(message);
            foreach (Socket sck in clientSocketHashtable.Values)
            {
                sck.Send(messageByte, 0, messageByte.Length, SocketFlags.None);
            }
        }

        private string OnlineList()
        {
            string onlineUserListString = "L";// request online user list command code
            
            for (int i=0;i<OnlineUser_listbox.Items.Count;i++)
            {
                onlineUserListString += OnlineUser_listbox.Items[i];
                if (i< OnlineUser_listbox.Items.Count-1)
                {
                    onlineUserListString += ",";
                }
            }
            return onlineUserListString;
        }
    }
}
