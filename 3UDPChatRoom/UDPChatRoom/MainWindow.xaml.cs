using System;
using System.Collections.Generic;
using System.Windows;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;
using System.Collections;
using System.Text;

namespace UDPChatRoom
{
    /// <summary>
    /// UDP Multiple User ChatRoom
    /// </summary>
    public partial class MainWindow : Window
    {
        const short Port = 2019;

        UdpClient udpClient;
        Thread listenThread;

        string userName;
        ArrayList clientIPs;
        string broadcastAddress = IPAddress.Broadcast.ToString();
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            clientIPs = new ArrayList();
            broadcastAddress = IPAddress.Broadcast.ToString();
            this.Title += " : " + LocalIP();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            try
            {
                if (listenThread != null)
                {
                    listenThread.Abort();
                }
                if (udpClient != null)
                {
                    udpClient.Close();
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private string LocalIP()
        {
            string hn = Dns.GetHostName();
            IPAddress[] ip = Dns.GetHostEntry(hn).AddressList;
            foreach (IPAddress it in ip)
            {
                if (it.AddressFamily == AddressFamily.InterNetwork)
                {
                    return it.ToString();
                }
            }
            return "";
        }
        private void Send(string TargetIP, string msg , string TargetName)
        {
            //Message Format : I am (who) + IP + Message + to Whom
            string message = userName + ":" + LocalIP() + ":" + msg + ":" + TargetName;
            byte[] messageByte = Encoding.Default.GetBytes(message);
            UdpClient targetClient = new UdpClient(TargetIP, Port);
            targetClient.Send(messageByte, messageByte.Length);
        }

        private void Online_button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(UserName_textbox.Text))
            {
                MessageBox.Show("請輸入姓名");
                return;
            }
            userName = UserName_textbox.Text;
            OnlineUser_listbox.Items.Clear();
            clientIPs.Clear();
            if (Online_button.Content.ToString() == "上線")
            {
                listenThread = new Thread(Listen);
                listenThread.Start();
                Send(broadcastAddress, "OnLine", "");
                Online_button.Content = "離線";
            }
            else
            {
                Send(broadcastAddress, "OffLine", "");
                listenThread.Abort();
                udpClient.Close();
                Online_button.Content = "上線";
            }

        }
        private void AddNewOnlineClient(string newClientName,string newClientIP)
        {

            this.Dispatcher.Invoke((Action)(() =>
            {
                OnlineUser_listbox.Items.Add(newClientName);
                clientIPs.Add(newClientIP);
            }));
        }
        private void RemoveOnlineClient(string clientName,string clientIP)
        {

            this.Dispatcher.Invoke((Action)(() =>
            {
                OnlineUser_listbox.Items.Remove(clientName);
                clientIPs.Remove(clientIP);
            }));
        }
        private void UpdateChatRoom(string message)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                ChatRoom_listbox.Items.Add(message);
            }));
        }
        private void Listen()
        {
            udpClient = new UdpClient(Port);
            IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Parse(LocalIP()),Port);
            while (true)
            {
                byte[] messageByte = udpClient.Receive(ref iPEndPoint);
                string message = Encoding.Default.GetString(messageByte);
                string[] cutMessage = message.Split(':');
                switch (cutMessage[2])
                {
                    case "OnLine":
                        AddNewOnlineClient(cutMessage[0], cutMessage[1]);
                        if (cutMessage[0] != userName) Send(cutMessage[1], "AddMe", cutMessage[0]);
                        break;
                    case "AddMe":
                        AddNewOnlineClient(cutMessage[0],cutMessage[1]);
                        break;
                    case "OffLine":
                        RemoveOnlineClient(cutMessage[0], cutMessage[1]);
                        break;
                    default:
                        if (cutMessage[3]=="")
                        {
                            UpdateChatRoom(cutMessage[0] + "(廣播):" + cutMessage[2]);
                        }
                        else
                        {
                            UpdateChatRoom(cutMessage[0] + " to " + cutMessage[3] + ":" + cutMessage[2]);
                        }
                        break;
                }
            }
        }

        private void Send_button_Click(object sender, RoutedEventArgs e)
        {
            OnlineUser_listbox.SelectedIndex = -1;
        }

        private void Message_textbox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                if (OnlineUser_listbox.SelectedIndex<0)
                {
                    Send(broadcastAddress, Message_textbox.Text, "");
                }
                else
                {
                    Send(clientIPs[OnlineUser_listbox.SelectedIndex].ToString(), Message_textbox.Text, OnlineUser_listbox.SelectedItem.ToString());
                    ChatRoom_listbox.Items.Add(userName + " to " + OnlineUser_listbox.SelectedItem.ToString() + ":" + Message_textbox.Text);
                    Message_textbox.Text = "";
                }
            }
        }
    }
}
