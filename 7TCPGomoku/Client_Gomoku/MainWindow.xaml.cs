using System;
using System.Collections.Generic;
using System.Windows;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Data;

namespace Client_Gomoku
{
    /// <summary>
    /// TCP Simple Gomoku Client
    /// </summary>
    public partial class MainWindow : Window
    {
        byte[,] chessRecord;

        #region Draw Size value
        double chessPieceRadious = 13;
        int gridSize = 570;
        int cellSize = 30;
        int cellNumber=1;
        int gridSizeWithoutGap;
        int cellSizeHalf;
        #endregion

        #region TCP value
        Socket clientSocket;
        Thread listenThread;
        string userName;
        #endregion

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            cellSizeHalf = cellSize / 2;
            gridSizeWithoutGap = gridSize - cellSizeHalf;
            Draw_Chessboard();
            cellNumber = gridSize / cellSize;
            chessRecord = new byte[cellNumber, cellNumber];
            //Draw_ChessPiece(0, 0, Brushes.Black);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (ConnectServer_button.IsEnabled==false)
            {
                Send("9" + userName);
                clientSocket.Close();
                if (listenThread!=null)
                {
                    listenThread.Abort();
                }
            }
        }
        private void ChessBoard_canvas_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

            int x = (int)((e.GetPosition(this).X- cellSizeHalf)/cellSize);
            int y = (int)((e.GetPosition(this).Y - cellSizeHalf) / cellSize);
            x = x > 18 ? 18 : x;
            y = y > 18 ? 18 : y;
            if (chessRecord[x,y]==0)
            {
                Draw_ChessPiece(x, y, Brushes.Black);
                chessRecord[x, y] = 1;

                if (OnlineUser_listbox.SelectedIndex>=0) // if have a oppenent
                {
                    Send("6" + x.ToString() + "," + y.ToString() + "|" + OnlineUser_listbox.SelectedItem);
                    ChessBoard_canvas.IsEnabled = false;
                }

                if (isWinGame(x,y,1))
                {
                    MessageBox.Show("你贏了!");
                }
            }
        }

        private void ClearAndReplay_button_Click(object sender, RoutedEventArgs e)
        {
            if (OnlineUser_listbox.SelectedIndex>=0)
            {
                Send("5" + "C" + "|" + OnlineUser_listbox.SelectedItem);
            }
            ClearChessboard();
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
                    case "5":
                        ClearChessboard();
                        break;
                    case "6":
                        string[] chessCoordinate = message.Split(',');
                        int x = int.Parse(chessCoordinate[0]);
                        int y = int.Parse(chessCoordinate[1]);
                        PlaceChess(x, y);
                        break;
                }
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

        #region Drawing Function
        private void PlaceChess(int x,int y)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                Draw_ChessPiece(x, y, Brushes.White);
                chessRecord[x, y] = 2;
                ChessBoard_canvas.IsEnabled = true;
                if (isWinGame(x,y,2))
                {
                    MessageBox.Show("你輸了!");
                }
            }));
        }
        private void ClearChessboard()
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                ChessBoard_canvas.Children.Clear();
                chessRecord = new byte[cellNumber, cellNumber];
                ChessBoard_canvas.IsEnabled = true;
            }));
        }
        private void Draw_Chessboard()
        {

            DrawingVisual drawingVisual = new DrawingVisual();

            // use DrawingContext to draw
            using (DrawingContext drawingContext = drawingVisual.RenderOpen())
            {
                // clear backround
                drawingContext.DrawRectangle(Brushes.White, null, new Rect(0, 0, gridSize, gridSize));

                // draw vertical line
                for (int i = cellSizeHalf; i <= gridSizeWithoutGap; i += cellSize)
                {
                    drawingContext.DrawLine(new Pen(Brushes.Black, 1), new Point(i, cellSizeHalf), new Point(i, gridSizeWithoutGap));
                }

                // draw horizontal line
                for (int i = cellSizeHalf; i <= gridSizeWithoutGap; i += cellSize)
                {
                    drawingContext.DrawLine(new Pen(Brushes.Black, 1), new Point(cellSizeHalf, i), new Point(gridSizeWithoutGap, i));
                }
            }

            DrawingBrush drawingBrush = new DrawingBrush(drawingVisual.Drawing);
            ChessBoard_canvas.Background = drawingBrush;
        }
        private void Draw_ChessPiece(int x,int y,Brush chessPieceColor)
        {
            
            Ellipse ellipse = new Ellipse();
            ellipse.Width = chessPieceRadious * 2;
            ellipse.Height = chessPieceRadious * 2;
            ellipse.Stroke = Brushes.Black;
            ellipse.StrokeThickness = 1;
            ellipse.Fill = chessPieceColor;
            Canvas.SetLeft(ellipse, (x+1)*cellSize - chessPieceRadious - cellSizeHalf); // set x coordinate
            Canvas.SetTop(ellipse, (y+1)*cellSize - chessPieceRadious - cellSizeHalf); // set y coordinate

            ChessBoard_canvas.Children.Add(ellipse);
        }
        #endregion

        private bool isWinGame(int x,int y, byte tg)
        {
            int chessNum = 0;
            int i, j;
            //check horizontal
            for (int k=-4; k<=4 ; k++)
            {
                i = x + k;
                if (i >= 0 && i< cellNumber)
                {
                    if (chessRecord[i,y]==tg)
                    {
                        chessNum += 1;
                        if (chessNum == 5)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        chessNum = 0;
                    }
                }
            }
            //check Vertical
            chessNum = 0;
            for (int k = -4; k <= 4; k++)
            {
                j = y + k;
                if (j >= 0 && j < cellNumber)
                {
                    if (chessRecord[x, j] == tg)
                    {
                        chessNum += 1;
                        if (chessNum == 5)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        chessNum = 0;
                    }
                }
            }
            //check top left
            chessNum = 0;
            for (int k = -4; k <= 4; k++)
            {
                i = x + k;
                j = y + k;
                if (j >= 0 && j < cellNumber && i >= 0 && i < cellNumber)
                {
                    if (chessRecord[i, j] == tg)
                    {
                        chessNum += 1;
                        if (chessNum == 5)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        chessNum = 0;
                    }
                }
            }
            //check top right
            chessNum = 0;
            for (int k = -4; k <= 4; k++)
            {
                i = x - k;
                j = y + k;
                if (j >= 0 && j < cellNumber && i >= 0 && i < cellNumber)
                {
                    if (chessRecord[i, j] == tg)
                    {
                        chessNum += 1;
                        if (chessNum == 5)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        chessNum = 0;
                    }
                }
            }

            return false;
        }
    }
}
