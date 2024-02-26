using System.Windows;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System;
using System.Diagnostics;
using System.Text;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Linq.Expressions;

namespace UDPGraffiti
{
    /// <summary>
    /// Udp Scribble
    /// </summary>
    public partial class MainWindow : Window
    {
        UdpClient udpClient;
        Thread listenThread;
        System.Windows.Point startPoint;
        string drawCoordinatePoint;
        public MainWindow()
        {
            InitializeComponent();

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }
        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            startPoint = e.GetPosition(this);
            drawCoordinatePoint = startPoint.X.ToString() + "," + startPoint.ToString();
        }

        private void Window_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed && Connect_button.IsEnabled==false)
            {
                Line line = new Line();
                line.X1 = startPoint.X;
                line.Y1 = startPoint.Y;
                line.X2 = e.GetPosition(this).X;
                line.Y2 = e.GetPosition(this).Y;
                if (Redrb.IsChecked == true) { line.Stroke = Brushes.Red; }
                if (Greenrb.IsChecked == true) { line.Stroke = Brushes.Lime; }
                if (Bluerb.IsChecked == true) { line.Stroke = Brushes.Blue; }
                if (Blackrb.IsChecked == true) { line.Stroke = Brushes.Black; }
                line.StrokeThickness = 2;
                LocalCanvas.Children.Add(line);
                startPoint = e.GetPosition(this);
                drawCoordinatePoint += "/" + startPoint.X.ToString() + "," + startPoint.Y.ToString();
            }
        }

        private void Window_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (string.IsNullOrEmpty(TargetPort_textbox.Text)) return;
            int port = int.Parse(TargetPort_textbox.Text);
            UdpClient targetClient = new UdpClient(TargetIP_textbox.Text, port);
            if (Redrb.IsChecked == true) { drawCoordinatePoint =   "1_" + drawCoordinatePoint; }
            if (Greenrb.IsChecked == true) { drawCoordinatePoint = "2_" + drawCoordinatePoint; }
            if (Bluerb.IsChecked == true) { drawCoordinatePoint =  "3_" + drawCoordinatePoint; }
            if (Blackrb.IsChecked == true) { drawCoordinatePoint = "4_" + drawCoordinatePoint; }
            Debug.WriteLine(drawCoordinatePoint);
            byte[] message = Encoding.Default.GetBytes(drawCoordinatePoint);
            targetClient.Send(message, message.Length);
            targetClient.Close();
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            try
            {
                if (listenThread !=null)
                {
                    listenThread.Abort();
                }
                if (udpClient !=null)
                {
                    udpClient.Close();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void Connect_button_Click(object sender, RoutedEventArgs e)
        {
            string port = ListenPort_textbox.Text;
            listenThread = new Thread(()=>Listen(port));
            listenThread.Start();
            Connect_button.IsEnabled = false;
        }

        private void Listen(string listenPort)
        {
            int port = int.Parse(listenPort);
            udpClient = new UdpClient(port);
            IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);
            while (true)
            {
                byte[] messageByte = udpClient.Receive(ref iPEndPoint);
                string message = Encoding.Default.GetString(messageByte);
                Debug.WriteLine(message);
                string[] colorCode = message.Split('_');
                string[] lineCoordinates = colorCode[1].Split('/');
                Point[] point = new Point[lineCoordinates.Length];
                for (int i=0;i<lineCoordinates.Length;i++)
                {
                    string[] K = lineCoordinates[i].Split(',');
                    point[i].X = double.Parse(K[0]);
                    point[i].Y = double.Parse(K[1]);
                }
                UpdateDistantCanvas(colorCode,point, lineCoordinates.Length - 1);
            }
        }
        private void UpdateDistantCanvas(string[] colorCode,Point[] point,int pointNum)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                for (int i = 0; i < pointNum; i++)
                {
                    Line line = new Line();
                    line.X1 = point[i].X;
                    line.Y1 = point[i].Y;
                    line.X2 = point[i + 1].X;
                    line.Y2 = point[i + 1].Y;
                    switch (colorCode[0])
                    {
                        case "1":
                            line.Stroke = Brushes.Red;
                            break;
                        case "2":
                            line.Stroke = Brushes.Green;
                            break;
                        case "3":
                            line.Stroke = Brushes.Blue;
                            break;
                        case "4":
                            line.Stroke = Brushes.Black;
                            break;
                    }
                    DistantCanvas.Children.Add(line);
                }
            }));
        }
    }
}
