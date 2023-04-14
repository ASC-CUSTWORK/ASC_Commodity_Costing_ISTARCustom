using PX.Data;

namespace ASCISTARCustom
{
    public class ASCIStarCustomerAllowanceMaint : PXGraph<ASCIStarCustomerAllowanceMaint, ASCIStarCustomerAllowance>
    {

        #region Actions
        //public PXSave<UsrAllowance> Save;
        //public PXCancel<UsrAllowance> Cancel;
        #endregion

        #region Data Members
        [PXImport]
        public PXSelect<ASCIStarCustomerAllowance> CustomerAllowance;
        //public PXFilter<UsrAllowance> MasterView;
        //public PXFilter<UsrAllowance> DetailsView;


        #endregion

        //public PXSave<UsrAllowance> Save;
        //public PXCancel<UsrAllowance> Cancel;

        //[Serializable]
        //public class MasterTable : IBqlTable
        //{

        //}

        //[Serializable]
        //public class DetailsTable : IBqlTable
        //{

        //}

        [PXMergeAttributes(Method = MergeMethod.Append)]
        [PXUIField(DisplayName = "Allowance ID")]
        protected void ASCIStarCustomerAllowance_InventoryID_CacheAttached(PXCache sender) { }



    }
}