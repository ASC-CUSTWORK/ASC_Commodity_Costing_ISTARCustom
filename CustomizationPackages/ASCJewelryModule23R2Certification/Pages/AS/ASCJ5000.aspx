<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="ASCJ5000.aspx.cs" Inherits="Page_ASCJ5000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%"
        TypeName="ASCJewelryLibrary.AP.ASCJMetalRatesSyncProcessing"
        PrimaryView="VandorBasis"
        >
		<CallbackCommands>

		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
	<px:PXFormView ID="form" runat="server" DataSourceID="ds" DataMember="Filter" Width="100%" Height="100px" AllowAutoHide="false">
		<Template>
			<px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartRow="True"></px:PXLayoutRule>
			<px:PXLayoutRule runat="server" ID="CstPXLayoutRule1" StartColumn="True" ></px:PXLayoutRule>
			<px:PXSegmentMask AllowEdit="True" CommitChanges="True" runat="server" ID="ASCIStarVendorIDSegmentMask" DataField="VendorID" ></px:PXSegmentMask>
			<px:PXSegmentMask CommitChanges="True" runat="server" ID="ASCIStarItemClassCDSegmentMask" DataField="ItemClassCD" ></px:PXSegmentMask>
			<px:PXLayoutRule runat="server" ID="CstPXLayoutRule2" StartColumn="True" ></px:PXLayoutRule>
			<px:PXSegmentMask CommitChanges="True" AllowEdit="True" runat="server" ID="ASCIStarInventoryIDSegmentMask5" DataField="InventoryID" ></px:PXSegmentMask></Template>
	</px:PXFormView>
</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
	<px:PXGrid AllowPaging="True" KeepPosition="True" SyncPosition="True" ID="grid" runat="server" DataSourceID="ds" Width="100%" Height="150px" SkinID="Inquire" AllowAutoHide="false">
		<Levels>
			<px:PXGridLevel DataMember="VandorBasis">
			    <Columns>
				<px:PXGridColumn Type="CheckBox" TextAlign="Center" AllowCheckAll="True" DataField="Selected" Width="60" ></px:PXGridColumn>
				<px:PXGridColumn DataField="VendorID" Width="140" ></px:PXGridColumn>
				<px:PXGridColumn DataField="VendorID_BAccountR_acctName" Width="280" ></px:PXGridColumn>
				<px:PXGridColumn DataField="Market" Width="70" ></px:PXGridColumn>
				<px:PXGridColumn DataField="InventoryID" Width="70" ></px:PXGridColumn>
				<px:PXGridColumn DataField="InventoryID_description" Width="280" ></px:PXGridColumn>
				<px:PXGridColumn DataField="ItemClassCD" Width="140" />
				<px:PXGridColumn DataField="UOM" Width="72" ></px:PXGridColumn>
				<px:PXGridColumn DataField="CuryID" Width="70" ></px:PXGridColumn>
				<px:PXGridColumn DataField="Commodity" Width="70" ></px:PXGridColumn></Columns>
			
				<RowTemplate>
					<px:PXSegmentMask AllowEdit="True" runat="server" ID="CstPXSegmentMask7" DataField="InventoryID" ></px:PXSegmentMask>
					<px:PXSegmentMask AllowEdit="True" runat="server" ID="CstPXSegmentMask8" DataField="VendorID" ></px:PXSegmentMask></RowTemplate></px:PXGridLevel>
		</Levels>
		<AutoSize Container="Window" Enabled="True" MinHeight="150" ></AutoSize>
		<ActionBar >
		</ActionBar>
	</px:PXGrid>
</asp:Content>