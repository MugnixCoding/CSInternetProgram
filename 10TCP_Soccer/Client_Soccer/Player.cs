
using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Client_Soccer
{
    public class Player : IDisposable
    {
        static string filepath = System.Environment.CurrentDirectory + "\\GameImage\\";
        Label label;
        Image image;
        int nameGab;
        public int width { get; private set; }
        public int height { get; private set; }
        public Player()
        {
            label = new Label();
            image = new Image();
        }
        public void SetPlayer(string name,int nameTag,int size)
        {
            label.Name = "L"+nameTag.ToString();
            label.Content = name;
            label.Tag = 0; // 0 mean pic = point0.png
            image.Width = 30;
            image.Height = 30;
            image.Source = GetImageFromPath(filepath + "Player.png");
            nameGab = (label.Content.ToString().Length) * (label.Content.ToString().Length - 1);
            width = size;
            height = size;
        }
        public void SetRival(string name, int nameTag, int size)
        {
            label.Name = "L" + nameTag.ToString();
            label.Content = name;
            label.Tag = 0; // 0 mean pic = point0.png
            image.Width = 30;
            image.Height = 30;
            image.Source = GetImageFromPath(filepath + "Rival.png");
            nameGab = (label.Content.ToString().Length) * (label.Content.ToString().Length - 1);
            width = size;
            height = size;
        }
        public void SetUser(int nameTag, int size)
        {
            label.Name = "L" + nameTag.ToString();
            label.Content = "";
            label.Tag = 0; // 0 mean pic = point0.png
            image.Width = 30;
            image.Height = 30;
            image.Source = GetImageFromPath(filepath + "Point0.png");
            nameGab = (label.Content.ToString().Length) * (label.Content.ToString().Length - 1);
            width = size;
            height = size;
        }
        public void AddtoCanvas(Canvas canvas,double left,double top)
        {
            canvas.Children.Add(image);
            canvas.Children.Add(label);
            SetPosition(left, top);
        }
        public void SetPosition(double left, double top)
        {
            Canvas.SetLeft(label, left - nameGab);
            Canvas.SetTop(label, top - (height / 4) * 3);
            Canvas.SetLeft(image, left);
            Canvas.SetTop(image, top);
        }
        public void Move(double x,double y)
        {
            double moveX = Canvas.GetLeft(image) + x;
            double moveY = Canvas.GetTop(image) - y;
            if (moveX<0)
            {
                moveX = 0;
            }
            else if (moveX>(600-width))
            {
                moveX = (600 - width);
            }
            if (moveY < 0)
            {
                moveY = 0;
            }
            else if (moveY > (600 - height))
            {
                moveY = (600 - height);
            }
            SetPosition(moveX,moveY);
        }
        public double GetLeft()
        {
            return Canvas.GetLeft(image);
        }
        public double GetTop()
        {
            return Canvas.GetTop(image);
        }
        public object GetDir()
        {
            return label.Tag;
        }
        public void SetDir(int dir)
        {
             label.Tag=dir;
        }
        public void UserTurn(int dir)
        {
            if (dir<0 || dir>7)
            {
                dir = 0;
            }
            image.Source = GetImageFromPath(filepath + "Point"+dir+".png");
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
        

        public void Dispose()
        {
            this.label = null;
            this.image = null;
        }
    }
}
