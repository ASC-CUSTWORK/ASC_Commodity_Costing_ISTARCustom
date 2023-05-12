using ASCISTARCustom.CustomerAllowance.Descriptor;
using ASCISTARCustom.DAC;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.AR;

namespace ASCISTARCustom.CustomerAllowance
{
    public class ASCIStarCustomerMaintExt : PXGraphExtension<CustomerMaint>
    {
        public static bool IsActive() => true;

        public SelectFrom<ASCIStarCustomerAllowance>.Where<ASCIStarCustomerAllowance.customerID.IsEqual<Customer.bAccountID.FromCurrent>>.View CustomerAllowance;

        #region Events

        protected virtual void _(Events.RowInserted<ASCIStarCustomerAllowance> e)
        {
            if (e.Row == null || this.Base.BAccount.Current?.BAccountID == null) return;

            e.Cache.SetValueExt<ASCIStarCustomerAllowance.customerID>(e.Row, this.Base.BAccount.Current.BAccountID);
        }
        protected virtual void _(Events.FieldVerifying<ASCIStarCustomerAllowance, ASCIStarCustomerAllowance.allowancePct> e)
        {
            var newValue = (decimal?)e.NewValue;
            if (newValue < -100.0m || newValue > 100.0m)
                throw new PXSetPropertyException<ASCIStarCustomerAllowance.allowancePct>(ASCIStarARConstants.Errors.AllowancePctCheckValue);
        }

        #endregion
    }
}
