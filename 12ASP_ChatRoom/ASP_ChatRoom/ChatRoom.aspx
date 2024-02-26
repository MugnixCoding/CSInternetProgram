<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ChatRoom.aspx.cs" Inherits="ASP_ChatRoom.ChatRoom" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
        <asp:ScriptManager ID="ScriptManager1" runat="server">
        </asp:ScriptManager>
        <asp:UpdatePanel ID="UpdatePanel1" runat="server">
            <ContentTemplate>
                <asp:Timer ID="Timer1" runat="server" Interval="2000" OnTick="Timer1_Tick"/>
                線上名單<br/>
                <asp:ListBox ID="OnlineUser_listbox" runat="server"></asp:ListBox>
                <br/>聊天看板<br/>
                <asp:TextBox ID="ChatRoom_textbox" runat="server" TextMode="MultiLine" Rows="7" />

            </ContentTemplate>
            
        </asp:UpdatePanel>
        我是:
        <asp:TextBox ID="UserName_textbox" runat="server"/>
        <asp:Button ID="Online_button" Text ="上線" runat="server" OnClick="Online_button_Click" />
        <br />
        想說:
        <asp:TextBox ID="Message_textbox" runat="server"/>
        <asp:Button ID="Send_button" Text ="送出" runat="server" OnClick="Send_button_Click" />
        </div>
    </form>
</body>
</html>
