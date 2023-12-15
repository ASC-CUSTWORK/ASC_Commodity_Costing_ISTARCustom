<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="ASCJ1010.aspx.cs" Inherits="Page_ASCJ1010" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%"
        TypeName="ASCJewelryLibrary.AP.ASCJAPTariffHTSCodeEntry"
        PrimaryView="TariffHTSCodeView"
        >
		<CallbackCommands>
			<px:PXDSCallbackCommand Visible="False" Name="First" ></px:PXDSCallbackCommand>
			<px:PXDSCallbackCommand Visible="False" Name="Previous" ></px:PXDSCallbackCommand>
			<px:PXDSCallbackCommand Visible="False" Name="Next" ></px:PXDSCallbackCommand>
			<px:PXDSCallbackCommand Visible="False" Name="Last" ></px:PXDSCallbackCommand>
			<px:PXDSCallbackCommand Visible="False" Name="CopyPaste" ></px:PXDSCallbackCommand>
			<px:PXDSCallbackCommand Visible="True" Name="Save" ></px:PXDSCallbackCommand>
			<px:PXDSCallbackCommand Visible="True" Name="Cancel" ></px:PXDSCallbackCommand>
			<px:PXDSCallbackCommand Visible="False" Name="Insert" ></px:PXDSCallbackCommand>
			<px:PXDSCallbackCommand Visible="False" Name="Delete" ></px:PXDSCallbackCommand></CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
	<px:PXGrid KeepPosition="True" runat="server" Height="150px" SkinID="Primary" Width="100%" ID="grid" AllowAutoHide="false" DataSourceID="ds" SyncPosition="True">
		<AutoSize Enabled="True" Container="Window" MinHeight="600" ></AutoSize>
		<Levels>
			<px:PXGridLevel DataMember="TariffHTSCodeView">
				<Columns>
					<px:PXGridColumn CommitChanges="True" DataField="HSTariffCode" Width="140" ></px:PXGridColumn>
					<px:PXGridColumn CommitChanges="True" DataField="HSTariffCodeDescr" Width="280" ></px:PXGridColumn></Columns></px:PXGridLevel></Levels></px:PXGrid></asp:Content>