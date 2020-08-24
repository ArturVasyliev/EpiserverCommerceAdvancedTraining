<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ConfigurePayment.ascx.cs" Inherits="AcmePaymentProvider.ConfigurePayment" %>
<table id="GenericTable" runat="server">
    <tr>
        <td class="FormLabelCell" colspan="2"><b><asp:Literal ID="litLabel" Text="Enter custom parameter value" runat="server"></asp:Literal></b></td>
    </tr>
    <tr>
		<td class="FormLabelCell" colspan="2"><b><asp:TextBox ID="txtSecretKey" Text="Enter some key example." runat="server"></asp:TextBox></b></td>
	</tr>
</table>