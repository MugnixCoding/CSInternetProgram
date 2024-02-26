using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;

namespace ASP_Gomoku
{
    public partial class Gomoku : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!File.Exists(Server.MapPath("bg.png")))
            {
                DrawChessBoard();
            }
        }
        private void DrawChessBoard()
        {
            Bitmap bmp = new Bitmap(570, 570);
            Graphics graphics = Graphics.FromImage(bmp);
            for (int i = 0; i < 19; i++)
            {
                graphics.DrawLine(Pens.Black, i * 30 + 15, 15, i * 30 + 15, 30 * 18 + 15);
            }
            for (int i = 0; i < 19; i++)
            {
                graphics.DrawLine(Pens.Black, 15, i * 30 + 15, 30 * 18 + 15, i * 30 + 15);
            }
            bmp.Save(Server.MapPath("bg.png"));
        }

        protected void MainPanel_timer_Tick(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(Server_hiddenfield.Value))
            {
                if (!string.IsNullOrEmpty(TargetName_textbox.Text))
                {
                    Application[TargetName_textbox.Text] = Server_hiddenfield.Value;
                }
                Server_hiddenfield.Value = "";
            }
            if (!string.IsNullOrEmpty(UserName_textbox.Text))
            {
                if (Application[UserName_textbox.Text]!=null)
                {
                    Client_hiddenfield.Value = Application[UserName_textbox.Text].ToString();
                    Application[UserName_textbox.Text] = null;
                }
            }
        }

        protected void Replay_button_Click(object sender, EventArgs e)
        {
            Application[TargetName_textbox.Text] = "0";
            Client_hiddenfield.Value = "0";
        }
    }
}