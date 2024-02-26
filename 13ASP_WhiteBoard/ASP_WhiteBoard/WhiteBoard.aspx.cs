using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ASP_WhiteBoard
{
    public partial class WhiteBoard : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void Timer1_Tick(object sender, EventArgs e)
        {
            if (HiddenField1.Value.Length>0)
            {
                if (TargetName_textbox.Text!="")
                {
                    Application[TargetName_textbox.Text] = HiddenField1.Value;
                    HiddenField1.Value = "";
                }
            }
            if (!string.IsNullOrEmpty(UserName_textbox.Text))
            {
                if (Application[UserName_textbox.Text]!=null)
                {
                    HiddenField2.Value = Application[UserName_textbox.Text].ToString();
                    Application[UserName_textbox.Text] = null;
                }
            }
        }
    }
}