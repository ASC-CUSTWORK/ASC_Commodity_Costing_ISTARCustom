using PX.Data;
using PX.Objects.PO;
using System;


namespace ASCJewelryLibrary.PO.DAC
{
    [Serializable]
    [PXCacheName("ASC POReceipt Extension")]
    public class ASCJPOReceiptExt : PXCacheExtension<POReceipt>
    {
        public static bool IsActive() => true;

        #region UsrAccrualLandedCost 
        [PXDBBool()]
        [PXUIField(DisplayName = "Accrual Landed Cost")]
        [PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual bool? UsrAccrualLandedCost { get; set; }
        public abstract class usrAccrualLandedCost : PX.Data.BQL.BqlBool.Field<usrAccrualLandedCost> { }
        #endregion
    }
}
