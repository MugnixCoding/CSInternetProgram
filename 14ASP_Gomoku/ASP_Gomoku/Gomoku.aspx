<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Gomoku.aspx.cs" Inherits="ASP_Gomoku.Gomoku" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title>網頁五子棋</title>
    <script type="text/javascript">
        var graphObject;
        var chessBoardArray;
        var listener;
        var turn;
        function init() {
            graphObject = ChessBoard_canvas.getContext("2d");
            reset();
        }
        function reset() {
            graphObject.clearRect(0, 0, 570, 570);
            chessBoardArray = new Array(19);
            for (var i = 0; i < 19; i++) {
                chessBoardArray[i] = new Array(19);
                for (var j = 0; j < 19; j++) {
                    chessBoardArray[i][j] = 0;
                }
            }
            BD.style.backgroundColor = "white";
            BD.style.cursor = "default";
            Msg.innerHTML = "下棋囉!";
            turn = true;
            listener = setInterval("timer()",250);
        }
        function placeChess(i,j,color) {
            var x = i * 30 + 15;
            var y = j * 30 + 15;
            graphObject.beginPath();
            graphObject.arc(x, y, 13, 0, Math.PI * 2, true);
            graphObject.closePath();
            graphObject.fillStyle = color;
            graphObject.fill();
            graphObject.stroke();
        }
        function CheckFive(i,j,color) {
            // check horizontal
            var n = 0;
            var m = 0;
            for (m = i - 4; m <= i + 4; m++) {
                n = IsNextSameColor(m, j, color, n);
                if (n==5) {
                    return true;
                }
            }
            //check vertical 
            n = 0;
            for (var m = j - 4; m <= j + 4; m++) {
                n = IsNextSameColor(i, m, color, n);
                if (n == 5) {
                    return true;
                }
            }
            //check left up to right down
            n = 0;
            for (var m = -4; m <= 4; m++) {
                n = IsNextSameColor(i + m, j + m, color, n);
                if (n == 5) {
                    return true;
                }
            }
            //check right up to left down
            n = 0;
            for (var m = -4; m <= 4; m++) {
                n = IsNextSameColor(i - m, j + m, color, n);
                if (n == 5) {
                    return true;
                }
            }
            return false;
        }
        function IsNextSameColor(i,j,color,count) {
            if (i < 0 || i > 18 || j < 0 || j > 18) {
                return count;
            }
            if (chessBoardArray[i][j] == color) {
                return count + 1;
            }
            else {
                return count;
            }
        }
        function OnMouseDown() {
            if (turn == false) return;
            var x = Math.round((event.offsetX - 15) / 30);
            var y = Math.round((event.offsetY - 15) / 30);
            if (chessBoardArray[x][y] != 0) {
                return;
            }
            document.getElementById("Server_hiddenfield").value = "-1," + x + "," + y;
            document.getElementById("Client_hiddenfield").value = "1," + x + "," + y;
            listener = setInterval("timer()", 250);
        }
        function timer() {
            if (document.getElementById("Client_hiddenfield").value == "") {
                return;
            }
            var receiveMsg = document.getElementById("Client_hiddenfield").value;
            document.getElementById("Client_hiddenfield").value = "";
            var message = receiveMsg.split(",");
            var cmd = parseInt(message[0]);
            if (cmd == 0) {
                reset();
            }
            else {
                var x = parseInt(message[1]);
                var y = parseInt(message[2]);
                chessBoardArray[x][y] = cmd; // 1:user ,-1:opponent
                switch (cmd) {
                    case 1:
                        placeChess(x, y, "black");
                        BD.style.backgroundColor = "lightyellow";
                        BD.style.cursor = "no-drop";
                        turn = false;
                        if (CheckFive(x, y, 1)) {
                            Msg.innerHTML = "你贏了!";
                        } else {
                            Msg.innerHTML = "換對手下...";
                        }
                        break;
                    case -1:
                        placeChess(x, y, "white");
                        if (CheckFive(x, y, -1)) {
                            Msg.innerHTML = "你輸了!";
                        } else {
                            BD.style.backgroundColor = "white";
                            BD.style.cursor = "default";
                            turn = true;
                            Msg.innerHTML = "到你了...";
                            clearInterval(listener);
                        }
                        break;
                }
            }
        }
    </script>
</head>
<body onload="init()">
    <form id="form1" runat="server">
        <div>
            <asp:ScriptManager ID="ScriptManager" runat="server" />
            <asp:UpdatePanel ID="MainPanel_updatepanel" runat="server">
                <ContentTemplate>
                    <asp:Timer ID="MainPanel_timer" runat="server" Interval="250" OnTick="MainPanel_timer_Tick" />
                    <asp:HiddenField ID="Server_hiddenfield" runat="server" />
                    <asp:HiddenField ID="Client_hiddenfield" runat="server" />
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
        <div id ="Msg"></div>
        <div id ="BD" style ="background-image:url(&quot;bg.png&quot;); width:570px;height:570px">
            <canvas id ="ChessBoard_canvas" width="570" height="570" onmousedown="OnMouseDown()"></canvas>
        </div>
        <p>
            我是:<asp:TextBox ID="UserName_textbox" runat="server" />
            在跟:<asp:TextBox ID="TargetName_textbox" runat="server" />
            <asp:Button ID="Replay_button" runat="server" Text="重玩" Width="40px" OnClick="Replay_button_Click" />
        </p>
    </form>
</body>
</html>
