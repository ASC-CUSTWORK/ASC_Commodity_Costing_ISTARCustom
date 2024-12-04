using ASCISTARCustom.PO.DAC;
using PX.Data;
using PX.Data.BQL.Fluent;

namespace ASCISTARCustom.PO
{
    public class ASCiStarDutyFreightMaint : PXGraph<ASCiStarDutyFreightMaint, ASCiStarDutyFreight>
    {
        public SelectFrom<ASCiStarDutyFreight>.OrderBy<ASCiStarDutyFreight.createdDateTime.Asc>.View ASCiStarDutyFreightView;
    }
}
