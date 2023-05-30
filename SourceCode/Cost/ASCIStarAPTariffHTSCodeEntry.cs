using ASCISTARCustom.Cost.DAC;
using PX.Data;
using PX.Data.BQL.Fluent;


namespace ASCISTARCustom.Cost
{
    public class ASCIStarAPTariffHTSCodeEntry : PXGraph<ASCIStarAPTariffHTSCodeEntry>
    {
        [PXFilterable]
        public SelectFrom<ASCIStarAPTariffHTSCode>.View TariffHTSCodeView;

        public PXSave<ASCIStarAPTariffHTSCode> Save;
        public PXCancel<ASCIStarAPTariffHTSCode> Cancel;
    }
}
