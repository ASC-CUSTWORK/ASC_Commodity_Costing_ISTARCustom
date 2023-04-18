using ASCISTARCustom.CustomerAllowance.Descriptor;
using ASCISTARCustom.DAC;
using PX.Data;

namespace ASCISTARCustom.CustomerAllowance
{
    public class ASCIStarCustomerAllowanceMaint : PXGraph<ASCIStarCustomerAllowanceMaint, ASCIStarCustomerAllowance>
    {
        [PXImport]
        public PXSelect<ASCIStarCustomerAllowance> CustomerAllowance;
       
        #region Events

        protected virtual void _(Events. FieldVerifying<ASCIStarCustomerAllowance, ASCIStarCustomerAllowance.allowancePct> e)
        {
            var newValue = (decimal?)e.NewValue;
            if (newValue<-100.0m || newValue>100.0m  )
                throw new PXSetPropertyException<ASCIStarCustomerAllowance.allowancePct>(ASCIStarARConstants.Errors.AllowancePctCheckValue);
        }

        #endregion
    }
}