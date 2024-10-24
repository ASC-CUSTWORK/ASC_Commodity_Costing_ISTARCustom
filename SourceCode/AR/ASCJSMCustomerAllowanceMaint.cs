using ASCJSMCustom.AR.Descriptor;
using ASCJSMCustom.AR.DAC;
using PX.Data;

namespace ASCJSMCustom.AR
{
    public class ASCJSMCustomerAllowanceMaint : PXGraph<ASCJSMCustomerAllowanceMaint, ASCJSMCustomerAllowance>
    {
        [PXImport]
        public PXSelect<ASCJSMCustomerAllowance> CustomerAllowance;
       
        #region Events

        protected virtual void _(Events. FieldVerifying<ASCJSMCustomerAllowance, ASCJSMCustomerAllowance.allowancePct> e)
        {
            var newValue = (decimal?)e.NewValue;
            if (newValue<-100.0m || newValue>100.0m  )
                throw new PXSetPropertyException<ASCJSMCustomerAllowance.allowancePct>(ASCJSMARConstants.Errors.AllowancePctCheckValue);
        }

        #endregion
    }
}