using PX.Data;
using PX.Objects.PO;
using System;


namespace ASCISTARCustom.PO.DAC
{
    [Serializable]
    [PXCacheName("ASC POReceipt Extension")]
    public class ASCIStarPOReceiptExt : PXCacheExtension<POReceipt>
    {
        public static bool IsActive() => true;

        #region UsrAccrualLandedCost 
        [PXDBBool()]
        [PXUIField(DisplayName = "Accrual Landed Cost", IsReadOnly = false)]
        [PXDefault(true, PersistingCheck = PXPersistingCheck.Nothing)]
        public virtual bool? UsrAccrualLandedCost { get; set; }
        public abstract class usrAccrualLandedCost : PX.Data.BQL.BqlBool.Field<usrAccrualLandedCost> { }
        #endregion
    }
}
