using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Input;

namespace Client_Poker
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
        bool myTurn=false;
        List<Image> myHandCard;
        string[] cardBoardRecord;
        string nextOne;
        List<string> win;
        #endregion
        public MainWindow()
        {
            InitializeComponent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            myHandCard = new List<Image>();
            win = new List<string>();
            cardBoardRecord = new string[8];
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
            }
            catch (Exception ex)
            {
                //MessageBox.Show("無法連接伺服器");
                SystemMessage_textbox.Text = "無法連接伺服器!" + "\r\n";
                return;
            }
            ConnectingUIChange(true);

        }

        private void DealCard_button_Click(object sender, RoutedEventArgs e)
        {
            if(OnlineUser_listbox.Items.Count!=2){
                MessageBox.Show("遊戲需要四人");
                return;
            }
            List<string> myList = new List<string>();
            string folderName = System.Environment.CurrentDirectory + "\\CardImage";
            foreach (string fname in System.IO.Directory.GetFiles(folderName))
            {
                if (fname=="Space.png")
                {
                    continue;
                }
                myList.Add(fname);
            }

            List<string> ranList = new List<string>();
            Random random = new Random();
            while (myList.Count>0)
            {
                int randKey = random.Next(0, myList.Count - 1);
                string s = myList[randKey];
                ranList.Add(s);
                myList.RemoveAt(randKey);
            }
            int eachoneCardnum = 52 / 4;
            int eachoneCardnumCounter = 0;
            int Pid = -1;
            string sendCard = "";
            for (int i =0;i<52;i++)
            {
                string[] Ps = ranList[i].Split('\\'); //cut the file path
                string[] Pn = Ps[Ps.Length - 1].Split('.');//cut the file name and extension

                eachoneCardnumCounter++;
                if (eachoneCardnumCounter==1 || eachoneCardnumCounter> eachoneCardnum)
                {
                    eachoneCardnumCounter = 1;
                    Pid += 1;

                    string player = OnlineUser_listbox.Items[Pid].ToString();
                    sendCard += player + "," + Pn[0] + ",";
                }
                else
                {
                    if (eachoneCardnumCounter < eachoneCardnum)
                    {
                        sendCard += Pn[0] + ",";
                    }
                    if (eachoneCardnumCounter == eachoneCardnum)
                    {
                        sendCard += Pn[0] + "|";
                    }
                }
            }
            Send("1" + "A" + sendCard);

            myTurn = true;
            SystemMessage_textbox.Text = "輪到你出牌了!";
            Skip_button.IsEnabled = true;
        }

        private void Skip_button_Click(object sender, RoutedEventArgs e)
        {
            Send("2"+"輪到你出牌了"+"|"+nextOne);
            UpdateSystemMessage("等待...");
            SkipButtonSwitch(false);
        }
        #endregion

        #region TCP communicate
        private void GetMessage(string receiveMessage)
        {
            string cmd = receiveMessage.Substring(0, 1);
            string message = receiveMessage.Substring(1);
            switch (cmd)
            {
                case "A":
                    DealCards(message);
                    break;
                case "B":
                    PlayCards(message);
                    break;
                case "C":
                    string[] winner = message.Split('|');
                    PlayCards(winner[0]);
                    WinCards(winner[1]);
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
                    case "1":
                        GetMessage(message);
                        break;
                    case "2":
                        UpdateSystemMessage(message);
                        myTurn = true;
                        SkipButtonSwitch(true);
                        break;
                }
            }
        }
        #endregion

        #region Game and Draw

        private bool CheckRules(Image cardImage)
        {
            int cardNumber = int.Parse(cardImage.Tag.ToString().Split('_')[0]);
            string cardKind = cardImage.Tag.ToString().Split('_')[2];
            if (cardNumber==7)
            {
                return true;
            }
            else
            {
                for (int i = 0;i<=6;i+=2)
                {
                    if (cardBoardRecord[i] == null) continue;
                    int max = int.Parse(cardBoardRecord[i].Split('_')[0]);
                    int min = int.Parse(cardBoardRecord[i+1].Split('_')[0]);
                    string kind = cardBoardRecord[i].Split('_')[3];
                    if (cardKind != kind)
                    {
                        continue;
                    }
                    if (cardNumber==max+1)
                    {
                        return true;
                    }
                    else if (cardNumber==min-1)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        private void WinCards(string player)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                win.Add(player);
                Rank_listbox.Items.Add("第"+win.Count.ToString()+"名:"+player);
                if (win.Count==3)
                {
                    UpdateSystemMessage("遊戲結束");
                    myTurn = false;
                    SkipButtonSwitch(false);
                }
                else
                {
                    nextOne = "";
                    int myid = -1;
                    for (int i=0;i<OnlineUser_listbox.Items.Count;i++)
                    {
                        if (OnlineUser_listbox.Items[i].ToString()==userName)
                        {
                            myid = i;
                            break;
                        }
                    }
                    int nextid = myid + 1;
                    if (nextid == OnlineUser_listbox.Items.Count) nextid = 0;

                    while (nextid != myid)
                    {
                        bool ck = false;
                        nextOne = OnlineUser_listbox.Items[nextid].ToString();
                        for (int  i = 0;i<win.Count;i++)
                        {
                            if(nextOne == win[i])
                            {
                                ck = true;
                                break;
                            }
                        }
                        if (ck == false) break;

                        if(nextid < OnlineUser_listbox.Items.Count)
                        {
                            nextid += 1;
                        }
                        else
                        {
                            nextid = 0;
                        }
                    }
                }
            }));

        }
        private void SkipButtonSwitch(bool isOn)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                Skip_button.IsEnabled = isOn;
            }));
        }
        private void PlayCards(string card)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                int cardNumber = int.Parse(card.Substring(0, 1));
                string cardKind = card.Split('_')[2];
                string cardfile = System.Environment.CurrentDirectory + "\\CardImage\\" + card + ".png";
                if (cardNumber == 7)
                {
                    for (int i = 0; i <= 6; i += 2)
                    {
                        if (cardBoardRecord[i] == null)
                        {
                            cardBoardRecord[i] = card;
                            cardBoardRecord[i + 1] = card;
                            foreach (object child in CardBoard_canvas.Children)
                            {
                                if (child is Image)
                                {
                                    if (((Image)child).Name == "CardBoard"+i)
                                    {
                                        ((Image)child).Source = GetImageFromPath(cardfile);
                                    }
                                    else if (((Image)child).Name == "CardBoard" + (i+1))
                                    {
                                        ((Image)child).Source = GetImageFromPath(cardfile);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    for (int i = 0; i <= 6; i += 2)
                    {
                        if (cardBoardRecord[i] == null)
                        {
                            continue;
                        }
                        int max = int.Parse(cardBoardRecord[i].Substring(0, 1));
                        int min = int.Parse(cardBoardRecord[i + 1].Substring(0, 1));
                        string boardKind = cardBoardRecord[i].Split('_')[2];
                        if (cardKind!=boardKind)
                        {
                            continue;
                        }
                        if (cardNumber == max+1)
                        {
                            cardBoardRecord[i] = card;
                            foreach (object child in CardBoard_canvas.Children)
                            {
                                if (child is Image)
                                {
                                    if (((Image)child).Name == "CardBoard" + i)
                                    {
                                        ((Image)child).Source = GetImageFromPath(cardfile);
                                        break;
                                    }
                                }
                            }
                            break;
                        }
                        else if(cardNumber==min-1)
                        {
                            cardBoardRecord[i+1] = card;
                            foreach (object child in CardBoard_canvas.Children)
                            {
                                if (child is Image)
                                {
                                    if (((Image)child).Name == "CardBoard" + (i+1))
                                    {
                                        ((Image)child).Source = GetImageFromPath(cardfile);
                                        break;
                                    }
                                }
                            }
                            break;
                        }

                    }
                }
            }));
        }
        private BitmapImage GetImageFromPath(string path)
        {
            BitmapImage imageSource = new BitmapImage();
            imageSource.BeginInit();
            imageSource.UriSource = new Uri(path, UriKind.RelativeOrAbsolute);
            imageSource.CacheOption = BitmapCacheOption.OnLoad;
            imageSource.EndInit();
            return imageSource;
        }
        private void UpdateCardInHandNumber(int number)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                CardInHand_label.Content = "剩餘牌數:"+number;
            }));
        }
        private void DealCards(string cardstring)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                foreach (object child in PlayerCard_canvas.Children)
                {
                    if (child is Image)
                    {
                        Image dump = (Image)child;
                        PlayerCard_canvas.Children.Remove(dump);
                        dump = null;
                    }
                }
                foreach (object child in CardBoard_canvas.Children)
                {
                    if (child is Image)
                    {
                        ((Image)child).Source = GetImageFromPath(System.Environment.CurrentDirectory + "\\CardImage\\" + "Space.png");
                    }
                }
                cardBoardRecord = new string[8];


                string[] F = cardstring.Split('|');
                int eachCounter = 0;
                for (int i = 0; i < F.Length; i++)
                {
                    string[] U = F[i].Split(',');  //PlayerName,Card1,Card2,....
                    eachCounter = U.Length - 1;
                    if (U[0] == userName)
                    {
                        for (int j = 1; j < U.Length; j++)
                        {
                            string fn1 = System.Environment.CurrentDirectory + "\\CardImage\\" + U[j] + ".png";
                            Image newCard = new Image();
                            newCard.Name = "P" + j.ToString();
                            newCard.Tag = U[j];
                            newCard.Width = 110;

                            newCard.Source = GetImageFromPath(fn1);

                            newCard.MouseDown += new System.Windows.Input.MouseButtonEventHandler(ImageCard_MouseDown);

                            PlayerCard_canvas.Children.Add(newCard);
                            Canvas.SetLeft(newCard, 20 * j);
                        }
                        break;
                    }
                }
                CardInHand_label.Content = "剩餘牌數:" + eachCounter.ToString();
                DealCard_button.IsEnabled = false;

                for (int i = 0;i<OnlineUser_listbox.Items.Count-1;i++)
                {
                    if (userName == OnlineUser_listbox.Items[i].ToString())
                    {
                        nextOne = OnlineUser_listbox.Items[i + 1].ToString();
                        break;
                    }
                }
                if (userName == OnlineUser_listbox.Items[OnlineUser_listbox.Items.Count - 1].ToString())
                {
                    nextOne = OnlineUser_listbox.Items[0].ToString();
                }
            }));
        }
        private void ImageCard_MouseDown(object sender,MouseEventArgs e)
        {
            if (myTurn==false)
            {
                return;
            }
            Image card = (Image)sender;
            //mouse left to select , right to place card
            if(e.LeftButton == MouseButtonState.Pressed)
            {
                for (int i = 0;i< myHandCard.Count;i++)
                {
                    Canvas.SetBottom(myHandCard[i], 5);
                }
                Canvas.SetBottom(card, 20);

            }
            else if(e.RightButton == MouseButtonState.Pressed)
            {
                if (CheckRules(card)==false)
                {
                    return;
                }
                if (myHandCard.Count>1)
                {
                    Send("1" + "B" + card.Tag);
                    Send("2" + "輪到你出牌!" + "|" + nextOne);
                    UpdateSystemMessage("等待...");
                }
                else
                {
                    Send("1" + "C" + card.Tag + "|" + userName);
                    if (win.Count<2)
                    {
                        Send("2" + "輪到你出牌!" + "|" + nextOne);
                    }
                    UpdateSystemMessage("結束遊戲!");
                }
                for (int i = 0; i < myHandCard.Count; i++)
                {
                    Image dump = myHandCard[i];
                    if (dump.Name == card.Name)
                    {
                        myHandCard.RemoveAt(i);
                        dump = null;
                        break;
                    }
                }
                UpdateCardInHandNumber(myHandCard.Count);
                myTurn = false;
                SkipButtonSwitch(false);
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
        private void UpdateSystemMessage(string msg)
        {
            this.Dispatcher.Invoke((Action)(() => 
            {
                SystemMessage_textbox.Text = msg;
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
    }
}
