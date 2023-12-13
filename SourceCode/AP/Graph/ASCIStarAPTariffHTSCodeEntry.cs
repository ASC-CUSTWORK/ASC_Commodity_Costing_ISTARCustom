using ASCISTARCustom.AP.DAC;
using PX.Data;
using PX.Data.BQL.Fluent;


namespace ASCISTARCustom.Cost
{
    public class ASCIStarAPTariffHTSCodeEntry : PXGraph<ASCIStarAPTariffHTSCodeEntry, ASCIStarAPTariffHTSCode>
    {
        [PXFilterable]
        public SelectFrom<ASCIStarAPTariffHTSCode>.View TariffHTSCodeView;
    }
}
