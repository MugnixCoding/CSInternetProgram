<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="WebForm1.aspx.cs" Inherits="ASP_InstantMessager.WebForm1" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
        </div>
        <!--ScriptManager須放在最頂端-->
        <asp:ScriptManager ID="ScriptManager1" runat="server">
        </asp:ScriptManager>
        
        <asp:UpdatePanel ID="UpdatePanel1" runat="server">
            <ContentTemplate>
                <asp:Timer ID="Timer1" runat="server" Interval="1000" OnTick="Timer1_Tick">
                </asp:Timer>
                <asp:TextBox ID="TextBox1" runat="server" TextMode="MultiLine" Rows="5"></asp:TextBox>
            </ContentTemplate>
        </asp:UpdatePanel>
        我是: 
        <asp:TextBox ID="TextBox2" runat="server"></asp:TextBox>
        要跟:  
        <asp:TextBox ID="TextBox3" runat="server"></asp:TextBox>
        <p>
            講說:  
            <asp:TextBox ID="TextBox4" runat="server"></asp:TextBox>
            <asp:Button ID="Button1" runat="server" Text="送出" OnClick="Button1_Click" />
        </p>
        
    </form>
</body>
</html>
