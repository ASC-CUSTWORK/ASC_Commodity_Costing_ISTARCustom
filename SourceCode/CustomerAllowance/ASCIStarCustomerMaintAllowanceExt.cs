using ASCISTARCustom;
using PX.Data;


namespace PX.Objects.AR
{

    public class ASCIStarCustomerMaintAllowanceExt : PXGraphExtension<CustomerMaint>
    {
        public PXSelect<ASCIStarCustomerAllowance, Where<ASCIStarCustomerAllowance.customerID, Equal<Current<Customer.bAccountID>>>> CustomerAllowance;

        //[PXMergeAttributes(Method = MergeMethod.Append)]
        //[PXDBDefault(typeof(Customer.bAccountID))]
        ////[PXParent(typeof(Select<Customer, Where<Customer.bAccountID, Equal<Current<Customer.bAccountID>>>>))]
        //protected virtual void ASCIStarCustomerAllowance_CustomerID_CacheAttached(PXCache sender) { }

        public static bool IsActive()
        {
            return true;
            //return PX.Data.Update.PXInstanceHelper.CurrentCompany == 2;
        }
    }
}
