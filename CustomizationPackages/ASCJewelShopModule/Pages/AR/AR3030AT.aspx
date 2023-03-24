<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true"  ValidateRequest="false" CodeFile="AR3030AT.aspx.cs" Inherits="Page_AR3030AT"  Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource ID="ds" Width="100%" runat="server" PrimaryView="CustomerAllowance" TypeName="ASCISTARCustom.ASCIStarCustomerAllowanceMaint" Visible="True">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Save" CommitChanges="True" />
			<px:PXDSCallbackCommand Name="Delete" Visible="False" />
			<px:PXDSCallbackCommand Name="First" Visible="False" />
			<px:PXDSCallbackCommand Name="Previous" Visible="False" />
			<px:PXDSCallbackCommand Name="Next" Visible="False" />
			<px:PXDSCallbackCommand Name="Last" Visible="False" />
			<px:PXDSCallbackCommand Name="Clipboard" Visible="False" />
		</CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
    <px:PXGrid ID="grid" runat="server" Height="400px" Width="100%" Style="z-index: 100" AllowPaging="True" AllowSearch="True" AdjustPageSize="Auto" DataSourceID="ds" 
		SkinID="Primary" FastFilterFields="CustomerID,InventoryID, Commodity" TabIndex="800">
		<Levels>
			<px:PXGridLevel DataMember="CustomerAllowance">
				<RowTemplate>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
					<px:PXSelector ID="edCustomerID" runat="server" DataField="CustomerID"  AutoCallBack="True" RenderEditorText="True" />
					<px:PXSelector ID="edOrderType" runat="server" DataField="OrderType" DisplayMode="Hint" AutoCallBack="True" RenderEditorText="True" />
					<px:PXSelector ID="edInventoryID" runat="server" DataField="InventoryID" DisplayMode="Hint" DisplayFormat="&gt;AAAAAAAAAA" AutoCallBack="True" RenderEditorText="True" />
					<px:PXDropDown ID="edCommodity" runat="server" AllowNull="False" DataField="Commodity" Enabled="True"/>
					<px:PXDateTimeEdit runat="server" CommitChanges="true" ID="edEffectiveAsOfDate" DataField="EffectiveAsOfDate" />
					<px:PXNumberEdit ID="edAllowancePct" runat="server" DataField="AllowancePct" />
					<px:PXCheckBox CommitChanges="True" ID="chkActive" runat="server" Checked="True" DataField="Active" />
				</RowTemplate>
				<Columns>
					<px:PXGridColumn DataField="CustomerID" />
					<px:PXGridColumn DataField="OrderType" />
					<px:PXGridColumn DataField="InventoryID" />
					<px:PXGridColumn DataField="AllowancePct" />
                    <px:PXGridColumn DataField="Commodity" RenderEditorText="True" />
					<px:PXGridColumn DataField="EffectiveDate" AutoCallBack="true" CommitChanges="true"/>
					<px:PXGridColumn AllowNull="False" DataField="Active" TextAlign="Center" Type="CheckBox" AutoCallBack="True" />
				</Columns>
				<Styles>
					<RowForm Height="250px">
					</RowForm>
				</Styles>
			</px:PXGridLevel>
		</Levels>
		<Layout FormViewHeight="250px" />
		<AutoSize Container="Window" Enabled="True" MinHeight="200" />
		<Mode AllowFormEdit="True" AllowUpload="True" />
		<CallbackCommands>
			<Save PostData="Content" />
		</CallbackCommands>
	</px:PXGrid>

</asp:Content>
