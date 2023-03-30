﻿using PX.Data;
using PX.Objects.PO;
using System;

namespace ASCISTARCustom
{
    public sealed class ASCIStarPOLineExt : PXCacheExtension<PX.Objects.PO.POLine>
    {
        public static bool IsActive() => true;
        
        #region UsrPurity
        [PXDBString(20)]
        [PXUIField(DisplayName = "Purity",Enabled =false)]
        public string UsrPurity { get; set; }
        public abstract class usrPurity : PX.Data.BQL.BqlString.Field<usrPurity> { }
        #endregion

        #region UsrWeight
        [PXDBDecimal]
        [PXUIField(DisplayName = "Weight/gram", Enabled = false)]
        public Decimal? UsrWeight { get; set; }
        public abstract class usrWeight : PX.Data.BQL.BqlDecimal.Field<usrWeight> { }
        #endregion

        #region UsrTotalWeight
        [PXDecimal]
        [PXUIField(DisplayName = "Total Weight (g)", Enabled = false)]
        public Decimal? UsrTotWeight { get; set; }
        public abstract class usrTotWeight : PX.Data.BQL.BqlDecimal.Field<usrTotWeight> { }
        #endregion

        #region UsrActualGoldWgt
        [PXDecimal]
        [PXUIField(DisplayName = "Actual Gold (g)", Enabled = false)]
        public Decimal? UsrActualGoldWgt { get; set; }
        public abstract class usrActualGoldWgt : PX.Data.BQL.BqlDecimal.Field<usrActualGoldWgt> { }
        #endregion

        #region UsrIntrinsicGold
        [PXDecimal]
        [PXUIField(DisplayName = "Intrinsic Gold (g)", Enabled = false)]
        public Decimal? UsrIntrinsicGoldWgt { get; set; }
        public abstract class usrIntrinsicGoldWgt : PX.Data.BQL.BqlDecimal.Field<usrIntrinsicGoldWgt> { }
        #endregion

        #region UsrIntrinsicGoldWgt
        [PXDecimal]
        [PXUIField(DisplayName = "Actual Silver (g)", Enabled = false)]
        public Decimal? UsrActualSilverWgt { get; set; }
        public abstract class usrActualSilverWgt : PX.Data.BQL.BqlDecimal.Field<usrActualSilverWgt> { }
        #endregion

        #region UsrIntrinsicSilver
        [PXDecimal]
        [PXUIField(DisplayName = "Intrinsic Silver (g)", Enabled = false)]
        public Decimal? UsrIntrinsicSilverWgt { get; set; }
        public abstract class usrIntrinsicSilverWgt : PX.Data.BQL.BqlDecimal.Field<usrIntrinsicSilverWgt> { }
        #endregion

        #region UsrRatePerGram
        [PXDBDecimal]
        [PXUIField(DisplayName = "Rate/gram", Enabled = false)]
        public Decimal? UsrRatePerGram { get; set; }
        public abstract class usrRatePerGram : PX.Data.BQL.BqlDecimal.Field<usrRatePerGram> { }
        #endregion

        #region UsrMaterialCost
        [PXDBDecimal]
        [PXUIField(DisplayName = "Material Cost", Enabled = false)]
        public Decimal? UsrMaterialCost { get; set; }
        public abstract class usrMaterialCost : PX.Data.BQL.BqlDecimal.Field<usrMaterialCost> { }
        #endregion

        #region UsrCostingType
        [PXDBString(1, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Costing Type")]
        [CostingType.List]
        [PXFormula(typeof(Selector<POLine.inventoryID, ASCIStarINInventoryItemExt.usrCostingType>))]
        public string UsrCostingType { get; set; }
        public abstract class usrCostingType : PX.Data.BQL.BqlString.Field<usrCostingType> { }
        #endregion
    }
}