<%@ Page Language="C#" AutoEventWireup="false"  CodeFile="Default.aspx.cs" Inherits="_Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Untitled Page</title>
    <link href="default.css" rel="stylesheet" type="text/css" />
</head>
<body>
    <form id="form1" runat="server">
    <div>
    <h2>Website 1</h2>
    <asp:TextBox ID="txtInput" runat="server"></asp:TextBox>
    <asp:TextBox ID="txtOutput" runat="server"></asp:TextBox>
    <asp:Button ID="btnCopy" runat="server" Text="Copy" />
    </div>
    </form>
</body>
</html>
