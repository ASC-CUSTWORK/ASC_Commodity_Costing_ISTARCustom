using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CS;
using PX.Objects.IN;
using ASCISTARCustom.Inventory.DAC;
using ASCISTARCustom.Inventory.Descriptor.Constants;

namespace ASCISTARCustom.Inventory.GraphExt
{
    //public class InventoryItemMaintExt : PXGraphExtension<InventoryItemMaint>
    //{
    //    public static bool IsActive() => true;

    //    [PXFilterable]
    //    [PXCopyPasteHiddenView(IsHidden = true)]
    //    public SelectFrom<ASCIStarINCompliance>.Where<ASCIStarINCompliance.inventoryID.IsEqual<InventoryItem.inventoryID.FromCurrent>>.View ComplianceView;

    //    public SelectFrom<ASCIStarINJewelryItem>.Where<ASCIStarINJewelryItem.inventoryID.IsEqual<InventoryItem.inventoryID.FromCurrent>>.View JewelryItemView;


    //    #region Events

    //    public virtual void _(Events.FieldSelecting<ASCIStarINCompliance, ASCIStarINCompliance.customerAlphaCode> e)
    //    {
    //        SetupStringList<ASCIStarINCompliance.customerAlphaCode>(e.Cache, INConstants.INAttributesID.CustomerCode);
    //    }

    //    public virtual void _(Events.FieldSelecting<ASCIStarINCompliance, ASCIStarINCompliance.division> e)
    //    {
    //        SetupStringList<ASCIStarINCompliance.division>(e.Cache, INConstants.INAttributesID.InventoryCategory);
    //    }

    //    public virtual void _(Events.FieldSelecting<ASCIStarINCompliance, ASCIStarINCompliance.testingLab> e)
    //    {
    //        SetupStringList<ASCIStarINCompliance.testingLab>(e.Cache, INConstants.INAttributesID.CPTESTTYPE);
    //    }

    //    public virtual void _(Events.FieldSelecting<ASCIStarINCompliance, ASCIStarINCompliance.protocolTestedTo> e)
    //    {
    //        SetupStringList<ASCIStarINCompliance.protocolTestedTo>(e.Cache, INConstants.INAttributesID.CPPROTOCOL);
    //    }

    //    public virtual void _(Events.FieldSelecting<ASCIStarINCompliance, ASCIStarINCompliance.waiverReasonCode> e)
    //    {
    //        SetupStringList<ASCIStarINCompliance.waiverReasonCode>(e.Cache, INConstants.INAttributesID.REASONCODE);
    //    }

    //    #endregion Events


    //    #region Methods

    //    private void SetupStringList<Field>(PXCache cache, string attributeID) where Field : IBqlField
    //    {
    //        List<string> values = new List<string>();
    //        List<string> labels = new List<string>();
    //        SelectAttributeDetails(attributeID).ForEach(x =>
    //        {
    //            values.Add(x.ValueID);
    //            labels.Add(x.Description);
    //        });
    //        PXStringListAttribute.SetList<Field>(cache, null, values.ToArray(), labels.ToArray());
    //    }

    //    private List<CSAttributeDetail> SelectAttributeDetails(string attributeID)
    //    {
    //        return SelectFrom<CSAttributeDetail>.Where<CSAttributeDetail.attributeID.IsEqual<@P.AsString>>.View.Select(this.Base, attributeID)?.FirstTableItems.ToList();
    //    }

    //    #endregion Methods
    //}
}
