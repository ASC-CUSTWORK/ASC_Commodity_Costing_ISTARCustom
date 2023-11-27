using ASCISTARCustom.AP.CacheExtension;
using PX.Data;
using PX.Data.BQL;
using PX.Objects.AP;

namespace ASCISTARCustom.AP.GraphExt
{
    public class ASCIStarAPVendorPriceMaintExt : PXGraphExtension<APVendorPriceMaint>
    {
        public static bool IsActive() => true;

        public override void Initialize()
        {
            base.Initialize();
            Base.Records.WhereAnd<Where
                <Brackets<ASCIStarAPVendorPriceFilterExt.usrOnlyMarkets.FromCurrent.IsEqual<True>
                .And<Vendor.vendorClassID.IsEqual<Common.Descriptor.ASCIStarConstants.MarketClass>>
                .Or<ASCIStarAPVendorPriceFilterExt.usrOnlyMarkets.FromCurrent.IsEqual<False>>>>>();
        }
    }
}
