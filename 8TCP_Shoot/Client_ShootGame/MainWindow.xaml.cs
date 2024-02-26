
using System.Windows;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media;
using System.Text;
using System;
using System.Windows.Input;
using System.Diagnostics;
using System.Windows.Threading;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Client_ShootGame
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        #region TCP value
        Socket clientSocket;
        Thread listenThread;
        string userName;
        #endregion

        #region Game Value
        double planeSpeed = 10;
        double bulletSpeed = 10;
        double canvasLimit;
        bool opponentShoot = false;

        DispatcherTimer playerBulletTimer;
        DispatcherTimer opponentShootCheckTimer;
        #endregion
        public MainWindow()
        {
            InitializeComponent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            canvasLimit = Fighting_canvas.Width - PlayerPlane_image.Width - 10;

            ClearAndReplay_button.Focus();
            ClearAndReplay_button.Visibility = Visibility.Hidden;
            playerBulletTimer = new DispatcherTimer();
            playerBulletTimer.Tick += new EventHandler(BulletTimer);
            playerBulletTimer.Interval = TimeSpan.FromSeconds(0.1);
            playerBulletTimer.Start();

            opponentShootCheckTimer = new DispatcherTimer();
            opponentShootCheckTimer.Tick += new EventHandler(OpponentShootChecker);
            opponentShootCheckTimer.Interval = TimeSpan.FromSeconds(0.01);
            opponentShootCheckTimer.Start();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (ConnectServer_button.IsEnabled == false)
            {
                Send("9" + userName);
                clientSocket.Close();
                if (listenThread != null)
                {
                    listenThread.Abort();
                }
            }
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            double left = 0;
            double right = 0;
            switch (e.Key)
            {
                case Key.A:
                    left = Canvas.GetLeft(PlayerPlane_image) - planeSpeed;
                    right = Canvas.GetRight(PlayerPlane_image) + planeSpeed;
                    Canvas.SetLeft(PlayerPlane_image,left<=0 ? 0 : left);
                    Canvas.SetRight(PlayerPlane_image, right > canvasLimit ? canvasLimit : right);
                    break;
                case Key.D:
                    left = Canvas.GetLeft(PlayerPlane_image) + planeSpeed;
                    right = Canvas.GetRight(PlayerPlane_image) - planeSpeed;
                    Canvas.SetLeft(PlayerPlane_image, left > canvasLimit ? canvasLimit : left);
                    Canvas.SetRight(PlayerPlane_image, right <=0 ?0 : right);
                    break;
                case Key.S:
                    MyShot();
                    break;
            }
            if (OnlineUser_listbox.SelectedIndex>=0)
            {
                switch (e.Key)
                {
                    case Key.A:
                        Send("3" + Canvas.GetLeft(PlayerPlane_image).ToString() + "|" + OnlineUser_listbox.SelectedItem); ;
                        break;
                    case Key.D:
                        Send("3" + Canvas.GetLeft(PlayerPlane_image).ToString() + "|" + OnlineUser_listbox.SelectedItem);
                        break;
                    case Key.S:
                        Send("4" + "S" + "|" + OnlineUser_listbox.SelectedItem);
                        break;
                }
            }
        }


        private void ConnectServer_button_Click(object sender, RoutedEventArgs e)
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
                SystemMessage_textbox.Text = "已連線伺服器!" + "\r\n";
                Send("0" + userName);
                ClearAndReplay_button.Focus();
            }
            catch (Exception ex)
            {
                //MessageBox.Show("無法連接伺服器");
                SystemMessage_textbox.Text = "無法連接伺服器!" + "\r\n";
                return;
            }
            ConnectingUIChange(true);

        }

        #region TCP communicate
        private void Send(string message)
        {
            byte[] messageByte = Encoding.Default.GetBytes(message);
            clientSocket.Send(messageByte, 0, messageByte.Length, SocketFlags.None);
        }
        private void Listen()
        {
            EndPoint serverEP = (EndPoint)clientSocket.RemoteEndPoint;
            byte[] recieveByte = new byte[1023];
            int recieveLen = 0;
            string recieveString;
            string cmd;//a char that present self define cmd code
            string message;
            while (true)
            {
                try
                {
                    recieveLen = clientSocket.ReceiveFrom(recieveByte, ref serverEP);
                }
                catch (Exception ex)
                {
                    clientSocket.Close();
                    MessageBox.Show("與伺服器斷線了!");
                    ConnectingUIChange(false);
                    listenThread.Abort();
                    listenThread = null;
                    return;
                }
                recieveString = Encoding.Default.GetString(recieveByte, 0, recieveLen);
                cmd = recieveString.Substring(0, 1);
                message = recieveString.Substring(1);
                switch (cmd)
                {
                    case "L":
                        UpdateOnlineUserList(message);
                        break;
                    case "3":
                        OpponentMove(int.Parse(message));
                        break;
                    case "4":
                        opponentShoot = true;
                        break;
                }
            }
        }
        #endregion

        #region Game and Draw
        private void OpponentMove(int left)
        {
            this.Dispatcher.Invoke((Action)(() => 
            {
                Canvas.SetRight(EnemyPlane_image, left);
                Canvas.SetLeft(EnemyPlane_image, 500-left);
            }));
        }
        private void MyShot()
        {
            Label bullet = new Label();
            bullet.Tag = "B";
            bullet.Width = 3;
            bullet.Height = 6;
            bullet.Background = Brushes.Red;
            Fighting_canvas.Children.Add(bullet);
            Canvas.SetLeft(bullet, Canvas.GetLeft(PlayerPlane_image)+35);
            Canvas.SetRight(bullet, Canvas.GetRight(PlayerPlane_image) +35);
            Canvas.SetTop(bullet, Canvas.GetTop(PlayerPlane_image));
            Canvas.SetBottom(bullet, Canvas.GetBottom(PlayerPlane_image)+70);

        }
        private void OpponentShot()
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                Label bullet = new Label();
                bullet.Tag = "X";
                bullet.Width = 3;
                bullet.Height = 6;
                bullet.Background = Brushes.Gray;
                Fighting_canvas.Children.Add(bullet);
                Canvas.SetLeft(bullet, Canvas.GetLeft(EnemyPlane_image) + 35);
                Canvas.SetRight(bullet, Canvas.GetRight(EnemyPlane_image) + 35);
                Canvas.SetTop(bullet, Canvas.GetTop(EnemyPlane_image) + 70);
                Canvas.SetBottom(bullet, Canvas.GetBottom(EnemyPlane_image));
            }));
        }
        private bool CheckHit(UIElement bullet,Image plane)
        {
            try
            {
                if ((Canvas.GetLeft(bullet) >= Canvas.GetLeft(plane)) &&
                    (Canvas.GetRight(bullet) >= Canvas.GetRight(plane)) &&
                    (Canvas.GetTop(bullet) >= Canvas.GetTop(plane)) &&
                    (Canvas.GetBottom(bullet) >= (Canvas.GetBottom(plane))))
                {
                    return true;
                }
                return false ;
            }
            catch (Exception)
            {
                return false;
            }
        }
        private void UpdateScore(int score)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                if (score == 0)
                {
                    PlayerScore_label.Content = "0";
                }
                else
                {
                    PlayerScore_label.Content = int.Parse(PlayerScore_label.Content.ToString()) + score;
                }
            }));
        }
        private void BulletTimer(object sender,EventArgs e)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                List<UIElement> dumpList = new List<UIElement>();
                foreach (UIElement b in Fighting_canvas.Children)
                {
                    if (b is Label)
                    {
                        switch (((Label)b).Tag)
                        {
                            case "B":
                                if (CheckHit(b, EnemyPlane_image))
                                {
                                    dumpList.Add(b);
                                    UpdateScore(1);
                                }
                                else if (Canvas.GetTop(b) - bulletSpeed < 0)
                                {
                                    dumpList.Add(b);
                                }
                                Canvas.SetTop(b, Canvas.GetTop(b) - bulletSpeed);
                                Canvas.SetBottom(b, Canvas.GetBottom(b) + bulletSpeed);
                                break;
                            case "X":
                                if (CheckHit(b, PlayerPlane_image))
                                {
                                    dumpList.Add(b);
                                }
                                else if (Canvas.GetBottom(b) - bulletSpeed < 0)
                                {
                                    dumpList.Add(b);
                                }
                                Canvas.SetTop(b, Canvas.GetTop(b) + bulletSpeed);
                                Canvas.SetBottom(b, Canvas.GetBottom(b) - bulletSpeed);
                                break;
                            default:
                                break;
                        }
                    }
                }
                for (int i = 0;i< dumpList.Count;i++)
                {
                    UIElement dump = dumpList[i];
                    Fighting_canvas.Children.Remove(dump);
                    dump = null;
                    
                }
            }));
        }
        private void OpponentShootChecker(object sender, EventArgs e)
        {
            if (opponentShoot)
            {
                OpponentShot();
                opponentShoot = false;
            }
        }
        #endregion

        #region UI change Function

        private void ConnectingUIChange(bool isConnect)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                if (!isConnect)
                {
                    OnlineUser_listbox.Items.Clear();
                }
                ConnectServer_button.IsEnabled = !isConnect;
            }));
        }
        private void UpdateOnlineUserList(string userList)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                OnlineUser_listbox.Items.Clear();
                string[] user = userList.Split(',');
                for (int i = 0; i < user.Length; i++)
                {
                    OnlineUser_listbox.Items.Add(user[i]);
                }
            }));
        }
        #endregion

        private void OnlineUser_listbox_GotFocus(object sender, RoutedEventArgs e)
        {
            ClearAndReplay_button.Focus();
        }
    }
}
