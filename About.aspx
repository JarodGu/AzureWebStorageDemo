<%@ Page Title="Program 4 - Web Storage" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="About.aspx.cs" Inherits="Program4WebStorage.About" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2><%: Title %>.</h2>
    <h3>People Web Storage Application</h3>
    <p>Load, Clear, and Query in Azure Storage</p>
    <p>
        <asp:Button ID="Button1" runat="server" OnClick="Button1_Click" Text="Load Data" />
    &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
        <asp:Label ID="Label1" runat="server" Text=""></asp:Label>
    </p>
    <p>&nbsp;</p>
    <p>
        <asp:Button ID="Button2" runat="server" Text="Clear Data" OnClick="Button2_Click" />
    &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
        <asp:Label ID="Label2" runat="server" Text=""></asp:Label>
    </p>
    <p>&nbsp;</p>
    <p>
        <asp:TextBox ID="TextBox1" runat="server" placeholder="Last name"></asp:TextBox>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
        <asp:TextBox ID="TextBox2" runat="server" placeholder="First name"></asp:TextBox>
    </p>
    <p>
        <asp:Button ID="Button3" runat="server" Text="Query" OnClick="Button3_Click" />
    &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
        <asp:Label ID="Label3" runat="server" Text=""></asp:Label>
    </p>
    <p>
        <asp:Table ID="Table1" runat="server"
                    GridLines="Both"
                    CellPadding="15" 
                    CellSpacing="0" >
        </asp:Table>
    </p>
</asp:Content>
