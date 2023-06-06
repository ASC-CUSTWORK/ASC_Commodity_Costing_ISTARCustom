using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CS;
using PX.Objects.IN;
using ASCISTARCustom.IN.Helpers;
using ASCISTARCustom.IN.DAC;

namespace ASCISTARCustom.IN.GraphExt
{
    public class InventoryItemMaintExt : PXGraphExtension<InventoryItemMaint>
    {
        public static bool IsActive() => true;

        [PXFilterable]
        public SelectFrom<INCompliance>.Where<INCompliance.inventoryID.IsEqual<InventoryItem.inventoryID.FromCurrent>>.View ComplianceView;

        public SelectFrom<INJewelryItemData>.Where<INJewelryItemData.inventoryID.IsEqual<InventoryItem.inventoryID.FromCurrent>>.View JewelryItemView;


        #region Events

        public virtual void _(Events.FieldSelecting<INCompliance, INCompliance.customerAlphaCode> e)
        {
            SetupStringList<INCompliance.customerAlphaCode>(e.Cache, INConstants.INAttributesID.CustomerCode);
        }

        public virtual void _(Events.FieldSelecting<INCompliance, INCompliance.division> e)
        {
            SetupStringList<INCompliance.division>(e.Cache, INConstants.INAttributesID.InventoryCategory);
        }

        public virtual void _(Events.FieldSelecting<INCompliance, INCompliance.testingLab> e)
        {
            SetupStringList<INCompliance.testingLab>(e.Cache, INConstants.INAttributesID.CPTESTTYPE);
        }

        public virtual void _(Events.FieldSelecting<INCompliance, INCompliance.protocolTestedTo> e)
        {
            SetupStringList<INCompliance.protocolTestedTo>(e.Cache, INConstants.INAttributesID.CPPROTOCOL);
        }

        public virtual void _(Events.FieldSelecting<INCompliance, INCompliance.waiverReasonCode> e)
        {
            SetupStringList<INCompliance.waiverReasonCode>(e.Cache, INConstants.INAttributesID.REASONCODE);
        }

        #endregion Events


        #region Methods

        private void SetupStringList<Field>(PXCache cache, string attributeID) where Field : IBqlField
        {
            List<string> values = new List<string>();
            List<string> labels = new List<string>();
            SelectAttributeDetails(attributeID).ForEach(x =>
            {
                values.Add(x.ValueID);
                labels.Add(x.Description);
            });
            PXStringListAttribute.SetList<Field>(cache, null, values.ToArray(), labels.ToArray());
        }

        private List<CSAttributeDetail> SelectAttributeDetails(string attributeID)
        {
            return SelectFrom<CSAttributeDetail>.Where<CSAttributeDetail.attributeID.IsEqual<@P.AsString>>.View.Select(this.Base, attributeID)?.FirstTableItems.ToList();
        }

        #endregion Methods
    }
}
