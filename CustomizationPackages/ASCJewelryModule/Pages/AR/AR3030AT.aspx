<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true"  ValidateRequest="false" CodeFile="AR3030AT.aspx.cs" Inherits="Page_AR3030AT"  Title="Untitled Page" %>

<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource ID="ds" Width="100%" runat="server" PrimaryView="CustomerAllowance" TypeName="ASCISTARCustom.CustomerAllowance.ASCIStarCustomerAllowanceMaint" Visible="True">
		<CallbackCommands>
			<px:PXDSCallbackCommand Name="Save" CommitChanges="True" ></px:PXDSCallbackCommand>
			<px:PXDSCallbackCommand Name="Delete" Visible="False" ></px:PXDSCallbackCommand>
			<px:PXDSCallbackCommand Name="First" Visible="False" ></px:PXDSCallbackCommand>
			<px:PXDSCallbackCommand Name="Previous" Visible="False" ></px:PXDSCallbackCommand>
			<px:PXDSCallbackCommand Name="Next" Visible="False" ></px:PXDSCallbackCommand>
			<px:PXDSCallbackCommand Name="Last" Visible="False" ></px:PXDSCallbackCommand>
			<px:PXDSCallbackCommand Name="Clipboard" Visible="False" ></px:PXDSCallbackCommand>
			<px:PXDSCallbackCommand Visible="False" Name="CopyPaste" ></px:PXDSCallbackCommand>
			<px:PXDSCallbackCommand Visible="False" Name="Insert" ></px:PXDSCallbackCommand></CallbackCommands>
	</px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phL" runat="Server">
    <px:PXGrid SyncPosition="True" ID="grid" runat="server" Height="400px" Width="100%" Style="z-index: 100" AllowPaging="True" AllowSearch="True" AdjustPageSize="Auto" DataSourceID="ds" 
		SkinID="Primary" FastFilterFields="CustomerID,InventoryID, Commodity" TabIndex="800">
		<Levels>
			<px:PXGridLevel DataMember="CustomerAllowance">
				<RowTemplate>
					<px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" ></px:PXLayoutRule>
					<px:PXSelector AutoRefresh="True" AllowEdit="True" ID="edCustomerID" runat="server" DataField="CustomerID"  AutoCallBack="True" RenderEditorText="True" ></px:PXSelector>
					<px:PXSelector ID="edOrderType" runat="server" DataField="OrderType" DisplayMode="Hint" AutoCallBack="True" RenderEditorText="True" ></px:PXSelector>
					<px:PXSelector AllowEdit="True" AutoRefresh="True" ID="edInventoryID" runat="server" DataField="InventoryID" DisplayMode="Hint" DisplayFormat="&gt;AAAAAAAAAA" AutoCallBack="True" RenderEditorText="True" ></px:PXSelector>
					<px:PXDropDown ID="edCommodity" runat="server" AllowNull="False" DataField="Commodity" Enabled="True"></px:PXDropDown>
					<px:PXDateTimeEdit runat="server" CommitChanges="true" ID="edEffectiveAsOfDate" DataField="EffectiveAsOfDate" ></px:PXDateTimeEdit>
					<px:PXNumberEdit ID="edAllowancePct" runat="server" DataField="AllowancePct" ></px:PXNumberEdit>
					<px:PXCheckBox CommitChanges="True" ID="chkActive" runat="server" Checked="True" DataField="Active" ></px:PXCheckBox>
				</RowTemplate>
				<Columns>
					<px:PXGridColumn DataField="CustomerID" ></px:PXGridColumn>
					<px:PXGridColumn DataField="CustomerID_description" Width="280" ></px:PXGridColumn>
					<px:PXGridColumn DataField="OrderType" ></px:PXGridColumn>
					<px:PXGridColumn CommitChanges="True" DataField="InventoryID" ></px:PXGridColumn>
					<px:PXGridColumn DisplayFormat="" DataField="AllowancePct" ></px:PXGridColumn>
                    <px:PXGridColumn DataField="Commodity" RenderEditorText="True" ></px:PXGridColumn>
					<px:PXGridColumn DataField="EffectiveDate" AutoCallBack="true" CommitChanges="true"></px:PXGridColumn>
					<px:PXGridColumn AllowNull="False" DataField="Active" TextAlign="Center" Type="CheckBox" AutoCallBack="True" ></px:PXGridColumn></Columns>
				<Styles>
					<RowForm Height="250px">
					</RowForm>
				</Styles>
			</px:PXGridLevel>
		</Levels>
		<Layout FormViewHeight="250px" ></Layout>
		<AutoSize Container="Window" Enabled="True" MinHeight="200" ></AutoSize>
		<Mode AllowFormEdit="True" AllowUpload="True" ></Mode>
		<CallbackCommands>
			<Save PostData="Content" ></Save>
		</CallbackCommands>
	</px:PXGrid>

</asp:Content>
