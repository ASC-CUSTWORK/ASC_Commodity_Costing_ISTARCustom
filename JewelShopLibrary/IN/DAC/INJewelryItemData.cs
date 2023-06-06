using System;
using ASCISTARCustom.CommonHelpers;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CS;
using PX.Objects.IN;
using static ASCISTARCustom.IN.Helpers.INConstants;

namespace ASCISTARCustom.IN.DAC
{
    [Serializable]
    [PXCacheName("Jewelry Item Data DAC")]
    public class INJewelryItemData : AuditSystemFieldsDAC, IBqlTable
    {
        public static bool IsActive() => true;

        #region InventoryID
        [PXDBInt(IsKey = true)]
        [PXParent(typeof(SelectFrom<InventoryItem>.Where<InventoryItem.inventoryID.IsEqual<inventoryID.FromCurrent>>))]
        [PXDBDefault(typeof(InventoryItem.inventoryID))]
        [PXUIField(DisplayName = "Inventory ID", Visible = false)]
        public virtual int? InventoryID { get; set; }
        public abstract class inventoryID : BqlInt.Field<inventoryID> { }
        #endregion

        #region ShortDesc
        [PXDBString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "Short Desc")]
        public virtual string ShortDesc { get; set; }
        public abstract class shortDesc : BqlString.Field<shortDesc> { }
        #endregion

        #region LongDesc
        [PXDBString(500, IsUnicode = true)]
        [PXUIField(DisplayName = "Long Desc")]
        public virtual string LongDesc { get; set; }
        public abstract class longDesc : BqlString.Field<longDesc> { }
        #endregion

        #region StyleStatus
        [PXDBString(10, IsUnicode = true)]
        [PXUIField(DisplayName = "Style Status")]
        [PXSelector(typeof(Search<CSAttributeDetail.valueID, Where<CSAttributeDetail.attributeID, Equal<INAttributesID.styleStatus>>, OrderBy<Asc<CSAttributeDetail.sortOrder>>>),
            new Type[] { typeof(CSAttributeDetail.valueID), typeof(CSAttributeDetail.description) },
            DescriptionField = typeof(CSAttributeDetail.description))]
        public virtual string StyleStatus { get; set; }
        public abstract class styleStatus : BqlString.Field<styleStatus> { }
        #endregion

        #region CustomerCode
        [PXDBString(10, IsUnicode = true)]
        [PXUIField(DisplayName = "Customer Code")]
        [PXSelector(typeof(Search<CSAttributeDetail.valueID, Where<CSAttributeDetail.attributeID, Equal<INAttributesID.customerCode>>, OrderBy<Asc<CSAttributeDetail.sortOrder>>>),
            new Type[] { typeof(CSAttributeDetail.valueID), typeof(CSAttributeDetail.description) },
            DescriptionField = typeof(CSAttributeDetail.description))]
        public virtual string CustomerCode { get; set; }
        public abstract class customerCode : BqlString.Field<customerCode> { }
        #endregion

        #region InvCategory
        [PXDBString(10, IsUnicode = true)]
        [PXUIField(DisplayName = "Inventory Category")]
        [PXSelector(typeof(Search<CSAttributeDetail.valueID, Where<CSAttributeDetail.attributeID, Equal<INAttributesID.inventoryCategory>>, OrderBy<Asc<CSAttributeDetail.sortOrder>>>),
            new Type[] { typeof(CSAttributeDetail.valueID), typeof(CSAttributeDetail.description) },
            DescriptionField = typeof(CSAttributeDetail.description))]
        public virtual string InvCategory { get; set; }
        public abstract class invCategory : BqlString.Field<invCategory> { }
        #endregion

        #region ItemType
        [PXDBString(10, IsUnicode = true)]
        [PXUIField(DisplayName = "Item Type")]
        [PXSelector(typeof(Search<CSAttributeDetail.valueID, Where<CSAttributeDetail.attributeID, Equal<INAttributesID.itemType>>, OrderBy<Asc<CSAttributeDetail.sortOrder>>>),
            new Type[] { typeof(CSAttributeDetail.valueID), typeof(CSAttributeDetail.description) },
            DescriptionField = typeof(CSAttributeDetail.description))]
        public virtual string ItemType { get; set; }
        public abstract class itemType : BqlString.Field<itemType> { }
        #endregion

        #region ItemSubType
        [PXDBString(10, IsUnicode = true)]
        [PXUIField(DisplayName = "Item Sub-Type")]
        [PXSelector(typeof(Search<CSAttributeDetail.valueID, Where<CSAttributeDetail.attributeID, Equal<INAttributesID.itemSubType>>, OrderBy<Asc<CSAttributeDetail.sortOrder>>>),
            new Type[] { typeof(CSAttributeDetail.valueID), typeof(CSAttributeDetail.description) },
            DescriptionField = typeof(CSAttributeDetail.description))]
        public virtual string ItemSubType { get; set; }
        public abstract class itemSubType : BqlString.Field<itemSubType> { }
        #endregion

        #region Collection
        [PXDBString(10, IsUnicode = true)]
        [PXUIField(DisplayName = "Collection")]
        [PXSelector(typeof(Search<CSAttributeDetail.valueID, Where<CSAttributeDetail.attributeID, Equal<INAttributesID.collections>>, OrderBy<Asc<CSAttributeDetail.sortOrder>>>),
            new Type[] { typeof(CSAttributeDetail.valueID), typeof(CSAttributeDetail.description) },
            DescriptionField = typeof(CSAttributeDetail.description))]
        public virtual string Collection { get; set; }
        public abstract class collection : BqlString.Field<collection> { }
        #endregion

        #region MetalType
        [PXDBString(10, IsUnicode = true)]
        [PXUIField(DisplayName = "Metal Type")]
        [PXSelector(typeof(Search<CSAttributeDetail.valueID, Where<CSAttributeDetail.attributeID, Equal<INAttributesID.metalType>>, OrderBy<Asc<CSAttributeDetail.sortOrder>>>),
            new Type[] { typeof(CSAttributeDetail.valueID), typeof(CSAttributeDetail.description) },
            DescriptionField = typeof(CSAttributeDetail.description))]
        public virtual string MetalType { get; set; }
        public abstract class metalType : BqlString.Field<metalType> { }
        #endregion

        #region MetalNote
        [PXDBString(100, IsUnicode = true)]
        [PXUIField(DisplayName = "Metal Note")]
        public virtual string MetalNote { get; set; }
        public abstract class metalNote : BqlString.Field<metalNote> { }
        #endregion

        #region MetalColor
        [PXDBString(10, IsUnicode = true)]
        [PXUIField(DisplayName = "Metal Color")]
        [PXSelector(typeof(Search<CSAttributeDetail.valueID, Where<CSAttributeDetail.attributeID, Equal<INAttributesID.metalColor>>, OrderBy<Asc<CSAttributeDetail.sortOrder>>>),
            new Type[] { typeof(CSAttributeDetail.valueID), typeof(CSAttributeDetail.description) },
            DescriptionField = typeof(CSAttributeDetail.description))]
        public virtual string MetalColor { get; set; }
        public abstract class metalColor : BqlString.Field<metalColor> { }
        #endregion

        #region Plating
        [PXDBString(10, IsUnicode = true)]
        [PXUIField(DisplayName = "Plating")]
        [PXSelector(typeof(Search<CSAttributeDetail.valueID, Where<CSAttributeDetail.attributeID, Equal<INAttributesID.plating>>, OrderBy<Asc<CSAttributeDetail.sortOrder>>>),
            new Type[] { typeof(CSAttributeDetail.valueID), typeof(CSAttributeDetail.description) },
            DescriptionField = typeof(CSAttributeDetail.description))]
        public virtual string Plating { get; set; }
        public abstract class plating : BqlString.Field<plating> { }
        #endregion

        #region Finishes
        [PXDBString(10, IsUnicode = true)]
        [PXUIField(DisplayName = "Finishes")]
        [PXSelector(typeof(Search<CSAttributeDetail.valueID, Where<CSAttributeDetail.attributeID, Equal<INAttributesID.finishes>>, OrderBy<Asc<CSAttributeDetail.sortOrder>>>),
            new Type[] { typeof(CSAttributeDetail.valueID), typeof(CSAttributeDetail.description) },
            DescriptionField = typeof(CSAttributeDetail.description))]
        public virtual string Finishes { get; set; }
        public abstract class finishes : BqlString.Field<finishes> { }
        #endregion

        #region VendorMaker
        [PXDBString(10, IsUnicode = true)]
        [PXUIField(DisplayName = "Vendor")]
        [PXSelector(typeof(Search<CSAttributeDetail.valueID, Where<CSAttributeDetail.attributeID, Equal<INAttributesID.vendorMaker>>, OrderBy<Asc<CSAttributeDetail.sortOrder>>>),
            new Type[] { typeof(CSAttributeDetail.valueID), typeof(CSAttributeDetail.description) },
            DescriptionField = typeof(CSAttributeDetail.description))]
        public virtual string VendorMaker { get; set; }
        public abstract class vendorMaker : BqlString.Field<vendorMaker> { }
        #endregion

        #region OrgCountry
        [PXDBString(10, IsUnicode = true)]
        [PXUIField(DisplayName = "Country of Origin")]
        [PXSelector(typeof(Search<CSAttributeDetail.valueID, Where<CSAttributeDetail.attributeID, Equal<INAttributesID.countryOfOrigion>>, OrderBy<Asc<CSAttributeDetail.sortOrder>>>),
            new Type[] { typeof(CSAttributeDetail.valueID), typeof(CSAttributeDetail.description) },
            DescriptionField = typeof(CSAttributeDetail.description))]
        public virtual string OrgCountry { get; set; }
        public abstract class orgCountry : BqlString.Field<orgCountry> { }
        #endregion

        #region StoneType
        [PXDBString(10, IsUnicode = true)]
        [PXUIField(DisplayName = "Stone Type")]
        [PXSelector(typeof(Search<CSAttributeDetail.valueID, Where<CSAttributeDetail.attributeID, Equal<INAttributesID.stoneType>>, OrderBy<Asc<CSAttributeDetail.sortOrder>>>),
            new Type[] { typeof(CSAttributeDetail.valueID), typeof(CSAttributeDetail.description) },
            DescriptionField = typeof(CSAttributeDetail.description))]
        public virtual string StoneType { get; set; }
        public abstract class stoneType : BqlString.Field<stoneType> { }
        #endregion

        #region WebNotesComment
        [PXDBString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "Notes")]
        public virtual string WebNotesComment { get; set; }
        public abstract class webNotesComment : BqlString.Field<webNotesComment> { }
        #endregion

        #region StoneComment
        [PXDBString(255, IsUnicode = true)]
        [PXUIField(DisplayName = "Stone Comment")]
        public virtual string StoneComment { get; set; }
        public abstract class stoneComment : BqlString.Field<stoneComment> { }
        #endregion

        #region StoneColor
        [PXDBString(10, IsUnicode = true)]
        [PXUIField(DisplayName = "Stone Color")]
        [PXSelector(typeof(Search<CSAttributeDetail.valueID, Where<CSAttributeDetail.attributeID, Equal<INAttributesID.stoneColor>>, OrderBy<Asc<CSAttributeDetail.sortOrder>>>),
            new Type[] { typeof(CSAttributeDetail.valueID), typeof(CSAttributeDetail.description) },
            DescriptionField = typeof(CSAttributeDetail.description))]
        public virtual string StoneColor { get; set; }
        public abstract class stoneColor : BqlString.Field<stoneColor> { }
        #endregion

        #region StoneShape
        [PXDBString(10, IsUnicode = true)]
        [PXUIField(DisplayName = "Stone Shape")]
        [PXSelector(typeof(Search<CSAttributeDetail.valueID, Where<CSAttributeDetail.attributeID, Equal<INAttributesID.stoneShapes>>, OrderBy<Asc<CSAttributeDetail.sortOrder>>>),
            new Type[] { typeof(CSAttributeDetail.valueID), typeof(CSAttributeDetail.description) },
            DescriptionField = typeof(CSAttributeDetail.description))]
        public virtual string StoneShape { get; set; }
        public abstract class stoneShape : BqlString.Field<stoneShape> { }
        #endregion

        #region StoneCreation
        [PXDBString(10, IsUnicode = true)]
        [PXUIField(DisplayName = "Stone Creation")]
        [PXSelector(typeof(Search<CSAttributeDetail.valueID, Where<CSAttributeDetail.attributeID, Equal<INAttributesID.stoneCreations>>, OrderBy<Asc<CSAttributeDetail.sortOrder>>>),
            new Type[] { typeof(CSAttributeDetail.valueID), typeof(CSAttributeDetail.description) },
            DescriptionField = typeof(CSAttributeDetail.description))]
        public virtual string StoneCreation { get; set; }
        public abstract class stoneCreation : BqlString.Field<stoneCreation> { }
        #endregion

        #region GemstoneTreatment
        [PXDBString(10, IsUnicode = true)]
        [PXUIField(DisplayName = "Gemstone Treatment")]
        [PXSelector(typeof(Search<CSAttributeDetail.valueID, Where<CSAttributeDetail.attributeID, Equal<INAttributesID.gemstoneTreatment>>, OrderBy<Asc<CSAttributeDetail.sortOrder>>>),
            new Type[] { typeof(CSAttributeDetail.valueID), typeof(CSAttributeDetail.description) },
            DescriptionField = typeof(CSAttributeDetail.description))]
        public virtual string GemstoneTreatment { get; set; }
        public abstract class gemstoneTreatment : BqlString.Field<gemstoneTreatment> { }
        #endregion

        #region SettingType
        [PXDBString(10, IsUnicode = true)]
        [PXUIField(DisplayName = "Setting Type")]
        [PXSelector(typeof(Search<CSAttributeDetail.valueID, Where<CSAttributeDetail.attributeID, Equal<INAttributesID.settingType>>, OrderBy<Asc<CSAttributeDetail.sortOrder>>>),
            new Type[] { typeof(CSAttributeDetail.valueID), typeof(CSAttributeDetail.description) },
            DescriptionField = typeof(CSAttributeDetail.description))]
        public virtual string SettingType { get; set; }
        public abstract class settingType : BqlString.Field<settingType> { }
        #endregion

        #region SettingType
        [PXDBString(10, IsUnicode = true)]
        [PXUIField(DisplayName = "Findings")]
        [PXSelector(typeof(Search<CSAttributeDetail.valueID, Where<CSAttributeDetail.attributeID, Equal<INAttributesID.findings>>, OrderBy<Asc<CSAttributeDetail.sortOrder>>>),
            new Type[] { typeof(CSAttributeDetail.valueID), typeof(CSAttributeDetail.description) },
            DescriptionField = typeof(CSAttributeDetail.description))]
        public virtual string Findings { get; set; }
        public abstract class findings : BqlString.Field<findings> { }
        #endregion

        #region FindingsSubType
        [PXDBString(10, IsUnicode = true)]
        [PXUIField(DisplayName = "Finding Sub Type")]
        [PXSelector(typeof(Search<CSAttributeDetail.valueID, Where<CSAttributeDetail.attributeID, Equal<INAttributesID.findingsSubType>>, OrderBy<Asc<CSAttributeDetail.sortOrder>>>),
            new Type[] { typeof(CSAttributeDetail.valueID), typeof(CSAttributeDetail.description) },
            DescriptionField = typeof(CSAttributeDetail.description))]
        public virtual string FindingsSubType { get; set; }
        public abstract class findingsSubType : BqlString.Field<findingsSubType> { }
        #endregion

        #region ChainType
        [PXDBString(10, IsUnicode = true)]
        [PXUIField(DisplayName = "Chain Type")]
        [PXSelector(typeof(Search<CSAttributeDetail.valueID, Where<CSAttributeDetail.attributeID, Equal<INAttributesID.chainType>>, OrderBy<Asc<CSAttributeDetail.sortOrder>>>),
            new Type[] { typeof(CSAttributeDetail.valueID), typeof(CSAttributeDetail.description) },
               DescriptionField = typeof(CSAttributeDetail.description))]
        public virtual string ChainType { get; set; }
        public abstract class chainType : BqlString.Field<chainType> { }
        #endregion

        #region RingLength
        [PXDBString(10, IsUnicode = true)]
        [PXUIField(DisplayName = "Ring Length")]
        [PXSelector(typeof(Search<CSAttributeDetail.valueID, Where<CSAttributeDetail.attributeID, Equal<INAttributesID.ringLength>>, OrderBy<Asc<CSAttributeDetail.sortOrder>>>),
            new Type[] { typeof(CSAttributeDetail.valueID), typeof(CSAttributeDetail.description) },
            DescriptionField = typeof(CSAttributeDetail.description))]
        public virtual string RingLength { get; set; }
        public abstract class ringLength : BqlString.Field<ringLength> { }
        #endregion

        #region RingSize
        [PXDBString(10, IsUnicode = true)]
        [PXUIField(DisplayName = "Ring Size")]
        [PXSelector(typeof(Search<CSAttributeDetail.valueID, Where<CSAttributeDetail.attributeID, Equal<INAttributesID.ringSize>>, OrderBy<Asc<CSAttributeDetail.sortOrder>>>),
            new Type[] { typeof(CSAttributeDetail.valueID), typeof(CSAttributeDetail.description) },
            DescriptionField = typeof(CSAttributeDetail.description))]
        public virtual string RingSize { get; set; }
        public abstract class ringSize : BqlString.Field<ringSize> { }
        #endregion

        #region OD
        [PXDBString(10, IsUnicode = true)]
        [PXUIField(DisplayName = "OD")]
        [PXSelector(typeof(Search<CSAttributeDetail.valueID, Where<CSAttributeDetail.attributeID, Equal<INAttributesID.od>>, OrderBy<Asc<CSAttributeDetail.sortOrder>>>),
            new Type[] { typeof(CSAttributeDetail.valueID), typeof(CSAttributeDetail.description) },
            DescriptionField = typeof(CSAttributeDetail.description))]
        public virtual string OD { get; set; }
        public abstract class od : BqlString.Field<od> { }
        #endregion
    }
}