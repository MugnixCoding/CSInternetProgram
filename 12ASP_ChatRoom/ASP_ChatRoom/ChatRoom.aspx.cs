using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ASP_ChatRoom
{
    public partial class ChatRoom : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void Online_button_Click(object sender, EventArgs e)
        {
            if (Online_button.Text=="上線")
            {
                if (!string.IsNullOrEmpty(UserName_textbox.Text))
                {
                    Session[UserName_textbox.Text] = UserName_textbox.Text;
                    Application.Lock();// 鎖定網站的公用變數
                    Hashtable onlineUserList = (Hashtable)Application["onlineUserList"];
                    onlineUserList.Add(Session[UserName_textbox.Text],DateTime.Now);
                    Application.UnLock();
                    Online_button.Text = "離線";
                }
            }
            else
            {
                Application.Lock();
                Hashtable onlineUserList = (Hashtable)Application["onlineUserList"];
                onlineUserList.Remove(Session[UserName_textbox.Text]);
                Application.UnLock();
                Session[UserName_textbox.Text] = null;
                Online_button.Text = "上線";
            }
        }

        protected void Send_button_Click(object sender, EventArgs e)
        {
            if (Session[UserName_textbox.Text]!=null)
            {
                Application.Lock();
                Queue chatRoomMessage = (Queue)Application["chatRoomMessage"];
                chatRoomMessage.Enqueue(UserName_textbox.Text + ":" + Message_textbox.Text);
                while (chatRoomMessage.Count>5)
                {
                    chatRoomMessage.Dequeue();
                }
                Application.UnLock();
                Message_textbox.Text = "";
            }
        }

        protected void Timer1_Tick(object sender, EventArgs e)
        {
            Queue chatRoomMessage = (Queue)Application["chatRoomMessage"];
            ChatRoom_textbox.Text = "";
            foreach (var message in chatRoomMessage)
            {
                ChatRoom_textbox.Text += message + "\r\n";
            }

            Application.Lock();
            Hashtable onlineUserList = (Hashtable)Application["onlineUserList"];
            if (Session[UserName_textbox.Text]!=null)
            {
                if (onlineUserList[Session[UserName_textbox.Text]]==null)
                {
                    onlineUserList.Add(Session[UserName_textbox.Text], DateTime.Now);
                }
                else
                {
                    onlineUserList[Session[UserName_textbox.Text]] = DateTime.Now;
                }
            }
            Application.UnLock();
            OnlineUser_listbox.Items.Clear();
            foreach (var name in onlineUserList.Keys)
            {
                OnlineUser_listbox.Items.Add(name.ToString());
            }
        }
    }
}