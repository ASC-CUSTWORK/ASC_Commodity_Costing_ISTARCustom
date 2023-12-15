<%@ Page Language="C#" MasterPageFile="~/MasterPages/TabView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="ISTR1000.aspx.cs" Inherits="Page_ISTR1000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/TabView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%"
        TypeName="ASCJewelryLibrary.AP.ASCJAPMetalRatesSetupMaint"
        PrimaryView="Setup"
        >
		<CallbackCommands>
			<px:PXDSCallbackCommand Visible="False" Name="testConnection" ></px:PXDSCallbackCommand></CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
	<px:PXTab ID="tab" runat="server" DataSourceID="ds" Height="150px" Style="z-index: 100" Width="100%" AllowAutoHide="false">
		<Items>
			<px:PXTabItem Text="General Settings">
			
				<Template>
					<px:PXFormView DataMember="Setup" DataSourceID="ds" Width="100%" SkinID="Transparent" runat="server" ID="CstFormView1" >
						<Template>
							<px:PXLayoutRule runat="server" ID="CstPXLayoutRule2" StartRow="True" ></px:PXLayoutRule>
							<px:PXLayoutRule ControlSize="XL" LabelsWidth="XL" runat="server" ID="CstPXLayoutRule3" StartColumn="True" ></px:PXLayoutRule>
							<px:PXLayoutRule GroupCaption="Metals-API Settings" runat="server" ID="CstPXLayoutRule4" StartGroup="True" ></px:PXLayoutRule>
							<px:PXTextEdit CommitChanges="True" runat="server" ID="BaseURLTextEdit" DataField="BaseURL" ></px:PXTextEdit>
							<px:PXSelector CommitChanges="True" runat="server" ID="BaseCurrencySelector" DataField="BaseCurrency" ></px:PXSelector>
							<px:PXTextEdit runat="server" ID="AccessKeyTextEdit" DataField="AccessKey" ></px:PXTextEdit>
							<px:PXDropDown CommitChanges="True" runat="server" ID="ASCIStarSymbolsDropDown" DataField="Symbols" ></px:PXDropDown>
							<px:PXButton CommandSourceID="ds" Width="100%" Text="Test Connection" CommandName="testConnection" runat="server" ID="ASCIStarTestConnectionButton" ></px:PXButton></Template></px:PXFormView></Template></px:PXTabItem>
		</Items>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
	</px:PXTab>
</asp:Content>