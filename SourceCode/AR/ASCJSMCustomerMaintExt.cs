using ASCJSMCustom.AR.Descriptor;
using ASCJSMCustom.AR.DAC;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.AR;

namespace ASCJSMCustom.AR
{
    public class ASCJSMCustomerMaintExt : PXGraphExtension<CustomerMaint>
    {
        public static bool IsActive() => true;

        public SelectFrom<ASCJSMCustomerAllowance>.Where<ASCJSMCustomerAllowance.customerID.IsEqual<Customer.bAccountID.FromCurrent>>.View CustomerAllowance;

        #region Events

        protected virtual void _(Events.RowSelected<PX.Objects.CR.Standalone.Location> e)
        {
            if (e.Row == null) return;

            PXUIFieldAttribute.SetRequired<PX.Objects.CR.Standalone.Location.cBranchID>(e.Cache, true);
            PXDefaultAttribute.SetPersistingCheck<PX.Objects.CR.Standalone.Location.cBranchID>(e.Cache, e.Row, PXPersistingCheck.NullOrBlank);
        }

        protected virtual void _(Events.RowInserted<ASCJSMCustomerAllowance> e)
        {
            if (e.Row == null || this.Base.BAccount.Current?.BAccountID == null) return;

            e.Cache.SetValueExt<ASCJSMCustomerAllowance.customerID>(e.Row, this.Base.BAccount.Current.BAccountID);
        }
        protected virtual void _(Events.FieldVerifying<ASCJSMCustomerAllowance, ASCJSMCustomerAllowance.allowancePct> e)
        {
            var newValue = (decimal?)e.NewValue;
            if (newValue < -100.0m || newValue > 100.0m)
                throw new PXSetPropertyException<ASCJSMCustomerAllowance.allowancePct>(ASCJSMARConstants.Errors.AllowancePctCheckValue);
        }

        #endregion
    }
}
