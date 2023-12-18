
using PX.Common;

namespace ASCJewelryLibrary.AR.Descriptor
{
    public class ASCJARConstants
    {
        public class AllowanceClass : PX.Data.BQL.BqlString.Constant<AllowanceClass>
        {
            public const string value = "ALLOWANCES";
            public AllowanceClass() : base(value) { }
        }

        [PXLocalizable]
        public class ASCJErrors
        {
            public const string AllowancePctCheckValue = "The Allowance Percetege can not be less then -100 and more then 100";
        }
    }
}
