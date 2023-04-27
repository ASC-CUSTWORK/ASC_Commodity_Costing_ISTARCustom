
namespace ASCISTARCustom.CustomerAllowance.Descriptor
{
    public static class ASCIStarARConstants
    {
        public class AllowanceClass : PX.Data.BQL.BqlString.Constant<AllowanceClass>
        {
            public static readonly string value = "ALLOWANCES";
            public AllowanceClass() : base(value) { }
        }

        public class Errors
        {
            public const string AllowancePctCheckValue = "The Allowance Percetege can not be less then -100 and more then 100";
        }
    }
}
