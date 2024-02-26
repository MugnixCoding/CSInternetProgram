using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;

namespace ASP_ChatRoom
{
    public class Global : System.Web.HttpApplication
    {
        Thread outdateChecker;
        protected void Application_Start(object sender, EventArgs e)
        {
            Hashtable onlineUserLiet = new Hashtable();
            Application["onlineUserList"] = onlineUserLiet;
            Queue chatRoomMessage = new Queue();
            Application["chatRoomMessage"] = chatRoomMessage;
            outdateChecker = new Thread(IsUserOutDate);
            outdateChecker.IsBackground = true;
            outdateChecker.Start();

        }
        private void IsUserOutDate()
        {
            while (true)
            {
                Hashtable onlineUserLiet = (Hashtable)Application["onlineUserList"];
                if (onlineUserLiet.Count>0)
                {
                    Application.Lock();
                    Hashtable stillOnlineList = new Hashtable();
                    foreach (var user in onlineUserLiet.Keys)
                    {
                        if (user != "")
                        {
                            DateTime lastOnlineTime = (DateTime)onlineUserLiet[user];
                            double s = new TimeSpan(DateTime.Now.Ticks - lastOnlineTime.Ticks).TotalSeconds;
                            if ((int)s <= 2)
                            {
                                stillOnlineList.Add(user, lastOnlineTime);
                            }
                        }
                    }
                    Application["onlineUserLiet"] = stillOnlineList;
                    Application.UnLock();
                }
                Thread.Sleep(1000);
            }
        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {
            outdateChecker.Abort();
        }
    }
}