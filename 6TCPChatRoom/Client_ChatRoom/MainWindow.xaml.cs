using System;
using System.Collections.Generic;
using System.Windows;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Xml.Linq;

namespace Client_ChatRoom
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        Socket clientSocket;
        Thread listenThread;
        string userName;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (Connect_button.IsEnabled == false)
            {
                Send("9" + userName);
                clientSocket.Close();
                if (listenThread!=null)
                {
                    listenThread.Abort();
                }
            }
        }
        private void Connect_button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(ServerIP_textbox.Text) || string.IsNullOrEmpty(ServerPort_textbox.Text) ||
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
                listenThread = new Thread(Listen);
                listenThread.IsBackground = true;
                listenThread.Start();
                ChatRoom_textbox.Text = "已連線伺服器!"+"\r\n";
                Send("0" + userName);
            }
            catch (Exception ex)
            {
                //MessageBox.Show("無法連接伺服器");
                ChatRoom_textbox.Text = "無法連接伺服器!" + "\r\n";
                return;
            }
            ConnectingUIChange(true);
        }
        private void Broadcast_button_Click(object sender, RoutedEventArgs e)
        {
            OnlineUser_listbox.SelectedIndex = -1;
        }

        private void SendMessage_button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Message_textbox.Text)) return;
            if (OnlineUser_listbox.SelectedIndex<0)
            {
                Send("1" + userName + "公告:" + Message_textbox.Text);
            }
            else
            {
                Send("2"+"來自" + userName + ":" + Message_textbox.Text+"|"+OnlineUser_listbox.SelectedItem);
                ChatRoom_textbox.Text += "告訴" + OnlineUser_listbox.SelectedItem + ":" + Message_textbox.Text + "\r\n";
            }
            Message_textbox.Text = "";
        }
        private void Send(string str)
        {
            byte[] messageByte = Encoding.Default.GetBytes(str);
            clientSocket.Send(messageByte, 0, messageByte.Length, SocketFlags.None);
        }

        private void Listen()
        {
            EndPoint serverEP = (EndPoint)clientSocket.RemoteEndPoint;
            byte[] messageByte = new byte[1023];
            int messageLen = 0;
            string recieveString;//== cmd+message;
            string cmd;
            string message;
            while (true)
            {
                try
                {
                    messageLen = clientSocket.ReceiveFrom(messageByte, ref serverEP);
                }
                catch(Exception ex)
                {
                    clientSocket.Close();
                    MessageBox.Show("與伺服器斷線了!");
                    ConnectingUIChange(false);
                    listenThread.Abort();
                    return;
                }
                recieveString = Encoding.Default.GetString(messageByte, 0, messageLen);
                cmd = recieveString.Substring(0, 1);
                message = recieveString.Substring(1);
                switch (cmd)
                {
                    case "L":   //get online list
                        UpdateOnlineUserList(message);
                        break;

                    case "1":   //get broadcast message
                        UpdateChatRoomText("(公開)" + message + "\r\n");
                        break;

                    case "2":   //get private message
                        UpdateChatRoomText("(私密)" + message + "\r\n");
                        break;
                }

            }
        }
        private void ConnectingUIChange(bool isConnect)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                if (!isConnect)
                {
                    OnlineUser_listbox.Items.Clear();
                }
                Connect_button.IsEnabled = !isConnect;
                SendMessage_button.IsEnabled = isConnect;
            }));
        }

        private void UpdateChatRoomText(string message)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                ChatRoom_textbox.Text += message;
                ChatRoom_textbox.SelectionStart = ChatRoom_textbox.Text.Length;
                ChatRoom_textbox.ScrollToEnd();
            }));
        }
        private void UpdateOnlineUserList(string userList)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                OnlineUser_listbox.Items.Clear();
                string[] user = userList.Split(',');
                for (int i =0;i<user.Length;i++)
                {
                    OnlineUser_listbox.Items.Add(user[i]);
                }
            }));
        }

    }
}
