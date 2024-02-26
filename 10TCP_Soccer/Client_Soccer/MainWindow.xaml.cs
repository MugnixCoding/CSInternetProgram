using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Client_Soccer
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

        #region Game value
        Hashtable onlineUserList;
        int teamID;
        int tickID;
        int ballVector = 0;
        int ballpower = 0;
        DispatcherTimer BallRolling;
        #endregion
        public MainWindow()
        {
            InitializeComponent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            onlineUserList = new Hashtable();
            BallRolling = new DispatcherTimer();
            BallRolling.Tick += new EventHandler(BallTimer);
            BallRolling.Interval = TimeSpan.FromSeconds(0.05);
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
            if (ConnectServer_button.IsEnabled==true)
            {
                return;
            }
            Player user = onlineUserList[userName] as Player;
            switch (e.Key)
            {
                case Key.Left:
                    user.Move(-5,0);
                    break;
                case Key.Right:
                    user.Move(5, 0);
                    break;
                case Key.Up:
                    user.Move(5, 0);
                    break;
                case Key.Down:
                    user.Move(-5, 0);
                    break;
                case Key.Z:
                    int nz = int.Parse(user.GetDir().ToString()) - 1;
                    if (nz==-1)
                    {
                        nz = 7;
                    }
                    user.SetDir(nz);
                    UserTurn(nz, user);
                    break;
                case Key.X:
                    int nx = int.Parse(user.GetDir().ToString()) + 1;
                    if (nx == 8)
                    {
                        nz = 0;
                    }
                    user.SetDir(nx);
                    UserTurn(nx, user);
                    break;
                case Key.V:
                    int nv = int.Parse(Power_label.Content.ToString()) +5;
                    if (nv > 50)
                    {
                        nv = 50;
                    }
                    Power_label.Content = nv;
                    break;
                case Key.C:
                    int nc = int.Parse(Power_label.Content.ToString()) - 5;
                    if (nc <0)
                    {
                        nc = 0;
                    }
                    Power_label.Content = nc;
                    break;
            }
            Send("14" + userName + "," + user.GetLeft().ToString() + "," + user.GetTop().ToString() 
                + "," + teamID.ToString());
            if (IsHit(user))
            {
                Send("15" + teamID.ToString()+","+user.GetDir().ToString()+","+Power_label.Content);
            }
        }

        #region Action

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
                ConnectingUIChange(true);
            }
            catch (Exception ex)
            {
                //MessageBox.Show("無法連接伺服器");
                SystemMessage_textbox.Text = "無法連接伺服器!" + "\r\n";
                return;
            }

        }

        private void StartGame_button_Click(object sender, RoutedEventArgs e)
        {
            if (OpponentTeam_listbox.Items.Count==0 || AllyTeam_listbox.Items.Count!= OpponentTeam_listbox.Items.Count)
            {
                return;
            }
            Random random = new Random();
            int whichTeam = random.Next(0, 1);//1 to odd team, 0 to even team
            double x = 280;
            double y = 390;
            Send("13" + whichTeam.ToString() + "," + x.ToString() + "," + y.ToString());
        }
        #endregion

        #region TCP communicate
        private void GetMessage(string receiveMessage)
        {
            string cmd = receiveMessage.Substring(0, 1);
            string message = receiveMessage.Substring(1);
            switch (cmd)
            {
                case "0":
                    Online(message);
                    break;
                case "9":
                    Offline(message);
                    break;
                case "3":
                    StartGame(message);
                    break;
                case "4":
                    PlayerMove(message);
                    break;
                case "5":
                    TickBall(message);
                    break;
            }
        }
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
                        string[] member = message.Split(',');
                        string team = "";
                        for (int i = 0;i<member.Length;i++)
                        {
                            if (member[i]==userName)
                            {
                                if (i%2==0)
                                {
                                    team = "0";
                                }
                                else
                                {
                                    team = "1";
                                }
                                break;
                            }
                        }
                        GetMessage("0" + team + message);
                        break;
                    case "E":
                        GetMessage("9" + message);
                        break;
                    case "2":
                        break;
                }
            }
        }
        #endregion

        #region Game and Draw
        private void StartGame(string receiveStr)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                string[] message = receiveStr.Split(','); // [0] = team,[1]=ball x  ,[2]=ball y
                if (int.Parse(message[0]) == teamID)
                {
                    //Ally First
                    Canvas.SetLeft(Ball_image, double.Parse(message[1]));
                    Canvas.SetTop(Ball_image,  double.Parse(message[2]));
                }
                else
                {
                    //Opponent First
                    Canvas.SetLeft(Ball_image, 560 - double.Parse(message[1]));
                    Canvas.SetTop(Ball_image, 560 - double.Parse(message[2]));
                }
                Ball_image.Visibility = Visibility.Visible;
                StartGame_button.IsEnabled = false;
                BallRolling.Start();
            }));
        }
        private void GameOver()
        {
            this.Dispatcher.Invoke((Action)(() => 
            {
                //ball in ally gate
                if (Canvas.GetTop(Ball_image)+Ball_image.Height>=580 &&
                    Canvas.GetLeft(Ball_image)>Canvas.GetLeft(AllyGate_label) &&
                    Canvas.GetLeft(Ball_image)+Ball_image.Width < Canvas.GetLeft(AllyGate_label)+AllyGate_label.Width)
                {
                    ballpower = 0;
                    int score = int.Parse(OpponentScore_label.Content.ToString().Substring(5)) + 1;
                    OpponentScore_label.Content = "敵隊得分:" + score;
                    if (score<3)
                    {
                        Canvas.SetLeft(Ball_image, 280);
                        Canvas.SetTop(Ball_image, 390);
                    }
                    else
                    {
                        MessageBox.Show("本隊輸了!");
                        StartGame_button.IsEnabled = true;
                        AllyScore_label.Content = "本隊得分:0";
                        OpponentScore_label.Content = "敵隊得分:0";

                        BallRolling.Stop();
                    }
                }
                if (Canvas.GetTop(Ball_image) < 20 &&
                    Canvas.GetLeft(Ball_image) > Canvas.GetLeft(OpponentGate_label) &&
                    Canvas.GetLeft(Ball_image) + Ball_image.Width < Canvas.GetLeft(OpponentGate_label) + OpponentGate_label.Width)
                {
                    ballpower = 0;
                    int score = int.Parse(AllyGate_label.Content.ToString().Substring(5)) + 1;
                    AllyGate_label.Content = "本隊得分:" + score;
                    if (score < 3)
                    {
                        Canvas.SetLeft(Ball_image, 280);
                        Canvas.SetTop(Ball_image, 170);
                    }
                    else
                    {
                        MessageBox.Show("本隊贏了!");
                        StartGame_button.IsEnabled = true;
                        AllyScore_label.Content = "本隊得分:0";
                        OpponentScore_label.Content = "敵隊得分:0";

                        BallRolling.Stop();
                    }
                }
            }));
        }
        private void BallTimer(object sender,EventArgs e)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                if (ballpower == 0)
                {
                    return;
                }
                double x=0;
                double y=0;
                switch (ballVector)
                {
                    case 0:
                        y = Canvas.GetTop(Ball_image) - ballpower;
                        y = y < 0 ? y : 0;
                        Canvas.SetTop(Ball_image,y);
                        break;
                    case 1:
                        x = Canvas.GetLeft(Ball_image) + ballpower;
                        x = x > 560 ? 560 : x;
                        Canvas.SetLeft(Ball_image, x);
                        y = Canvas.GetTop(Ball_image) - ballpower;
                        y = y < 0 ? y : 0;
                        Canvas.SetTop(Ball_image, y);
                        break;
                    case 2:
                        x = Canvas.GetLeft(Ball_image) + ballpower;
                        x = x > 560 ? 560 : x;
                        Canvas.SetLeft(Ball_image, x);
                        break;
                    case 3:
                        x = Canvas.GetLeft(Ball_image) + ballpower;
                        x = x > 560 ? 560 : x;
                        Canvas.SetLeft(Ball_image, x);
                        y = Canvas.GetTop(Ball_image) + ballpower;
                        y = y > 560 ? 560 : y;
                        Canvas.SetTop(Ball_image, y);
                        break;
                    case 4:
                        y = Canvas.GetTop(Ball_image) + ballpower;
                        y = y > 560 ? 560 : y;
                        Canvas.SetTop(Ball_image, y);
                        break;
                    case 5:
                        x = Canvas.GetLeft(Ball_image) - ballpower;
                        x = x < 0 ? 0 : x;
                        Canvas.SetLeft(Ball_image, x);
                        y = Canvas.GetTop(Ball_image) + ballpower;
                        y = y > 560 ? 560 : y;
                        Canvas.SetTop(Ball_image, y);
                        break;
                    case 6:
                        x = Canvas.GetLeft(Ball_image) - ballpower;
                        x = x < 0 ? 0 : x;
                        Canvas.SetLeft(Ball_image, x);
                        break;
                    case 7:
                        x = Canvas.GetLeft(Ball_image) - ballpower;
                        x = x < 0 ? 0 : x;
                        Canvas.SetLeft(Ball_image, x);
                        y = Canvas.GetTop(Ball_image) - ballpower;
                        y = y < 0 ? y : 0;
                        Canvas.SetTop(Ball_image, y);
                        break;
                }
                ballpower -= 1;
                GameOver();
            }));
        }
        private void TickBall(string receiveStr)
        {
            this.Dispatcher.Invoke((Action)(() => 
            {
                string[] message = receiveStr.Split(',');
                tickID = int.Parse(message[0]);
                ballVector = int.Parse(message[1]);
                ballpower = int.Parse(message[2]);
                if (tickID == teamID) { return; }
                ballVector = (ballVector + 4) % 8;//Opponent kick need Change dir on client draw
            }));
        }
        private bool IsHit(Player player)
        {
            int dir = int.Parse(player.GetDir().ToString());
            Point point = new Point(); // direction of Pointer
            switch (dir)
            {
                case 0:
                    point = new Point(player.GetLeft() + player.width / 2, player.GetTop());
                    break;
                case 1:
                    point = new Point(player.GetLeft() + player.width, player.GetTop());
                    break;
                case 2:
                    point = new Point(player.GetLeft() + player.width, player.GetTop() - player.height/2);
                    break;
                case 3:
                    point = new Point(player.GetLeft() + player.width, player.GetTop()+player.height);
                    break;
                case 4:
                    point = new Point(player.GetLeft() + player.width / 2, player.GetTop() + player.height);
                    break;
                case 5:
                    point = new Point(player.GetLeft() , player.GetTop() + player.height);
                    break;
                case 6:
                    point = new Point(player.GetLeft() , player.GetTop() + player.height/2);
                    break;
                case 7:
                    point = new Point(player.GetLeft(), player.GetTop());
                    break;
            }
            if (point.X > Canvas.GetLeft(Ball_image)+Ball_image.Width)
            {
                return false;
            }
            else if (point.X < Canvas.GetLeft(Ball_image))
            {
                return false;
            }
            else if (point.Y > Canvas.GetTop(Ball_image) + Ball_image.Height)
            {
                return false;
            }
            else if (point.Y > Canvas.GetTop(Ball_image) + Ball_image.Height)
            {
                return false;
            }

            return true;
        }
        private void PlayerMove(string receiveStr)
        {
            this.Dispatcher.Invoke((Action)(() => 
            {
                string[] message = receiveStr.Split(','); // [0] = player,[1]= player x,[2] = player y,[3] = player team
                Player player = onlineUserList[message[0]] as Player;
                if (int.Parse(message[3])==teamID)
                {
                    player.SetPosition(int.Parse(message[1]), int.Parse(message[2]));
                }
                else
                {
                    player.SetPosition(560 - int.Parse(message[1]), 560 - int.Parse(message[2]));
                }
            }));
        }
        private void UserTurn(int dir,Player player)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                player.UserTurn(dir);
            }));
        }
        private void Online(string message)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                int tm = int.Parse(message.Substring(0, 1));
                string[] playerList = message.Substring(1).Split(',');
                if (onlineUserList.ContainsKey(userName) == false)
                {
                    for (int i = 0; i < playerList.Length; i++)
                    {
                        string player = playerList[i];
                        if (i % 2 == tm)
                        {
                            if (player == userName)
                            {
                                AddMe(player, i, tm);
                            }
                            else
                            {
                                AddTeam(player, i);
                            }
                        }
                        else
                        {
                            AddRival(player, i);
                        }
                    }
                }
                else
                {
                    string player = playerList[playerList.Length - 1];
                    if ((playerList.Length - 1) % 2 == teamID)
                    {
                        AddTeam(player, playerList.Length - 1);
                    }
                    else
                    {
                        AddRival(player, playerList.Length - 1);
                    }
                }
            }));

        }
        private void Offline(string message)
        {
            this.Dispatcher.Invoke((Action)(() => 
            {
                //delete offline player from listbox
                for (int i =0;i<AllyTeam_listbox.Items.Count;i++)
                {
                    if (AllyTeam_listbox.Items[i].ToString()==message)
                    {
                        AllyTeam_listbox.Items.RemoveAt(i);
                        break;
                    }
                }
                for (int i = 0;i<OpponentTeam_listbox.Items.Count;i++)
                {
                    if (OpponentTeam_listbox.Items[i].ToString()==message)
                    {
                        OpponentTeam_listbox.Items.RemoveAt(i);
                        break;
                    }
                }
                Player dump = onlineUserList[message] as Player;
                dump.Dispose();
            }));
        }
        private void AddMe(string player ,int i ,int team)
        {
            this.Dispatcher.Invoke((Action)(() => 
            {
                AllyTeam_listbox.Items.Add(player);
                teamID = team;

                Player newTeamate = new Player();
                newTeamate.SetUser(i ,30);
                newTeamate.AddtoCanvas(Field_canvas, 25+((AllyTeam_listbox.Items.Count-1)*50), 500);

                onlineUserList.Add(player, newTeamate);
            }));
        }
        private void AddTeam(string player, int i)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                AllyTeam_listbox.Items.Add(player);

                Player newTeamate = new Player();
                newTeamate.SetPlayer(player, i, 30);
                newTeamate.AddtoCanvas(Field_canvas, 25 + ((AllyTeam_listbox.Items.Count - 1) * 50), 500);

                onlineUserList.Add(player, newTeamate);
            }));
        }
        private void AddRival(string player, int i)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                AllyTeam_listbox.Items.Add(player);

                Player newTeamate = new Player();
                newTeamate.SetRival(player, i, 30);
                newTeamate.AddtoCanvas(Field_canvas, 560 - ((AllyTeam_listbox.Items.Count - 1) * 50), 100);

                onlineUserList.Add(player, newTeamate);
            }));
        }
        #endregion

        #region UI change Function

        private void ConnectingUIChange(bool isConnect)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                ServerIP_textbox.IsEnabled = !isConnect;
                ServerPort_textbox.IsEnabled = !isConnect;
                UserName_textbox.IsEnabled = !isConnect;
                ConnectServer_button.IsEnabled = !isConnect;
            }));
        }
        private void UpdateSystemMessage(string msg)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                SystemMessage_textbox.Text = msg;
            }));
        }
        #endregion
    }
}
