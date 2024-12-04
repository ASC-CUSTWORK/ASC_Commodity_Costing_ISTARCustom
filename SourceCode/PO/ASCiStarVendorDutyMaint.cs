using ASCISTARCustom.PO.DAC;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Data.Descriptor;

namespace ASCISTARCustom.PO
{
    public class ASCiStarVendorDutyMaint : PXGraph<ASCiStarVendorDutyMaint, ASCiStarVendorDuty>
    {
        public SelectFrom<ASCiStarVendorDuty>.OrderBy<ASCiStarVendorDuty.createdDateTime.Asc>.View ASCiStarVendorDutyView;
    }
}
