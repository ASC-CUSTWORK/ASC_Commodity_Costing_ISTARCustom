using ASCJewelryLibrary.AP.DAC;
using PX.Data;
using PX.Data.BQL.Fluent;


namespace ASCJewelryLibrary.AP
{
    public class ASCJAPTariffHTSCodeEntry : PXGraph<ASCJAPTariffHTSCodeEntry, ASCJAPTariffHTSCode>
    {
        [PXFilterable]
        public SelectFrom<ASCJAPTariffHTSCode>.View TariffHTSCodeView;
    }
}
