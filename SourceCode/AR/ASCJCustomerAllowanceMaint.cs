using ASCJewelryLibrary.AR.Descriptor;
using ASCJewelryLibrary.AR.DAC;
using PX.Data;
using PX.Data.BQL.Fluent;

namespace ASCJewelryLibrary.AR
{
    public class ASCJCustomerAllowanceMaint : PXGraph<ASCJCustomerAllowanceMaint, ASCJARCustomerAllowance>
    {
        [PXImport]
        public SelectFrom<ASCJARCustomerAllowance>.View CustomerAllowance;
       
        #region Events

        protected virtual void _(Events. FieldVerifying<ASCJARCustomerAllowance, ASCJARCustomerAllowance.allowancePct> e)
        {
            var newValue = (decimal?)e.NewValue;
            if (newValue<-100.0m || newValue>100.0m  )
                throw new PXSetPropertyException<ASCJARCustomerAllowance.allowancePct>(ASCJARConstants.ASCJErrors.AllowancePctCheckValue);
        }

        #endregion
    }
}