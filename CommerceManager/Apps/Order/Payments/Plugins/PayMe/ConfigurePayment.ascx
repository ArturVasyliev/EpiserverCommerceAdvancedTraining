<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ConfigurePayment.ascx.cs" Inherits="TrainingPaymentProvider.ConfigurePayment" %>

<table id="GenericTable" runat="server">
	<tr>
		<td class="FormLabelCell" colspan="2"><b><asp:Literal ID="Literal1" Text="Enter custom parameter value" runat="server"></asp:Literal></b></td>
	</tr>

    <tr>
		<td class="FormLabelCell" colspan="2"><b><asp:TextBox ID="txtBox1" Text="some default 1" runat="server"></asp:TextBox></b></td>
	</tr>
    
    <tr>
		<td class="FormLabelCell" colspan="2"><b><asp:TextBox ID="txtBox2" Text="some default 2" runat="server"></asp:TextBox></b></td>
	</tr>
</table>

