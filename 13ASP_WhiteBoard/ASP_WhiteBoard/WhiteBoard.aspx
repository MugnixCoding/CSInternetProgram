<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="WhiteBoard.aspx.cs" Inherits="ASP_WhiteBoard.WhiteBoard" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
    <script type="text/javascript">
        var draw = false;//繪圖狀態
        var c;//繪圖物件
        var p;//繪圖記錄字串
        function init() {
            c = WhiteBoard_canvas.getContext("2d");
            checkin();
        }
        function Canvas_mouse_down() {
            c.moveTo(event.offsetX, event.offsetY);
            draw = true;
            p = event.offsetX + "," + event.offsetY + "/";
        }
        function Canvas_mouse_move() {
            if (draw) {
                c.lineTo(event.offsetX, event.offsetY);
                c.stroke();//繪圖
                p += event.offsetX + "," + event.offsetY + "/";
            }
        }
        function Canvas_mouse_up() {
            document.getElementById("HiddenField1").value = p;//拷貝繪圖字串到H1 伺服器將回收
            draw = false;
        }
        function checkin() {
            if (document.getElementById("HiddenField2").value != "") {
                var z = document.getElementById("HiddenField2").value.split("/");
                document.getElementById("HiddenField2").value = "";
                var p = z[0].split(",");
                for (var i = 1; i < z.length; i++) {
                    var q = z[i].split(",");
                    c.lineTo(q[0], q[1]);
                }
                c.stroke();
            }
            setTimeout("checkin()", 200);//0.2秒後再執行一次
        }
    </script>
</head>
<body onload="init()">
    <form id="form1" runat="server">
        <div>
        </div>
        <asp:ScriptManager ID="ScriptManager1" runat="server">
        </asp:ScriptManager>
        <asp:UpdatePanel ID="UpdatePanel1" runat="server">
            <ContentTemplate>
                <asp:Timer ID="Timer1" runat="server" Interval="500" OnTick="Timer1_Tick"></asp:Timer>
                <asp:HiddenField ID="HiddenField1" runat="server" />
                <asp:HiddenField ID="HiddenField2" runat="server" />
            </ContentTemplate>
        </asp:UpdatePanel>
        
        我是: 
        <asp:TextBox ID="UserName_textbox" runat="server"></asp:TextBox>
        畫給:  
        <asp:TextBox ID="TargetName_textbox" runat="server"></asp:TextBox>看
    </form>
    <canvas id ="WhiteBoard_canvas" width ="400" height="300"
        onmousedown="Canvas_mouse_down()" onmousemove="Canvas_mouse_move()"
        onmouseup ="Canvas_mouse_up()" style="border: thin solid #000000">
    </canvas>
</body>
</html>
