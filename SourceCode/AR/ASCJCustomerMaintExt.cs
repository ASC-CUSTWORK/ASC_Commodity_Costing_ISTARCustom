using ASCJewelryLibrary.AR.Descriptor;
using ASCJewelryLibrary.AR.DAC;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.AR;

namespace ASCJewelryLibrary.AR
{
    public class ASCJCustomerMaintExt : PXGraphExtension<CustomerMaint>
    {
        public static bool IsActive() => true;

        public SelectFrom<ASCJARCustomerAllowance>.Where<ASCJARCustomerAllowance.customerID.IsEqual<Customer.bAccountID.FromCurrent>>.View ASCJCustomerAllowance;

        #region Events

        protected virtual void _(Events.RowInserted<ASCJARCustomerAllowance> e)
        {
            if (e.Row == null || this.Base.BAccount.Current?.BAccountID == null) return;

            e.Cache.SetValueExt<ASCJARCustomerAllowance.customerID>(e.Row, this.Base.BAccount.Current.BAccountID);
        }
        protected virtual void _(Events.FieldVerifying<ASCJARCustomerAllowance, ASCJARCustomerAllowance.allowancePct> e)
        {
            var newValue = (decimal?)e.NewValue;
            if (newValue < -100.0m || newValue > 100.0m)
                throw new PXSetPropertyException<ASCJARCustomerAllowance.allowancePct>(ASCJARConstants.ASCJErrors.AllowancePctCheckValue);
        }

        #endregion
    }
}
