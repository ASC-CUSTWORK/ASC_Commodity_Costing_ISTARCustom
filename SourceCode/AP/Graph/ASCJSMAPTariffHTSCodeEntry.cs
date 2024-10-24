using ASCJSMCustom.AP.DAC;
using PX.Data;
using PX.Data.BQL.Fluent;


namespace ASCJSMCustom.Cost
{
    public class ASCJSMAPTariffHTSCodeEntry : PXGraph<ASCJSMAPTariffHTSCodeEntry, ASCJSMAPTariffHTSCode>
    {
        [PXFilterable]
        public SelectFrom<ASCJSMAPTariffHTSCode>.View TariffHTSCodeView;
    }
}
