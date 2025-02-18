using ASCISTARCustom.PO.DAC;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.SM;

namespace ASCISTARCustom.PO
{
    public class ASCiStarDutyFreightMaint : PXGraph<ASCiStarDutyFreightMaint, ASCiStarDutyFreight>
    {
        [PXImport(typeof(ASCiStarDutyFreight))]
        public SelectFrom<ASCiStarDutyFreight>.OrderBy<ASCiStarDutyFreight.createdDateTime.Asc>.View ASCiStarDutyFreightView;


        //public void Initialize()
        //{
        //    var importAttr = this.ASCiStarDutyFreightView.GetAttribute<PXImportAttribute>();
        //    importAttr.MappingPropertiesInit += InitTaskMapppingProperties;
        //}

        //private void InitTaskMapppingProperties(object sender, PXImportAttribute.MappingPropertiesInitEventArgs args)
        //{
        //    args.Names.Add(nameof(ASCiStarDutyFreight.HSTariffCode));
        //    args.Names.Add(nameof(ASCiStarDutyFreight.CountryCode));
        //    args.Names.Add(nameof(ASCiStarDutyFreight.DutyPercent));
        //    args.Names.Add(nameof(ASCiStarDutyFreight.EffectiveDate));
        //    args.DisplayNames.Add("Tariff / HTS Code");
        //    args.DisplayNames.Add("Country Code");
        //    args.DisplayNames.Add("Duty, %");
        //    args.DisplayNames.Add("Effective Date");
        //}

        //public PXAction<ASCiStarDutyFreight> ImportFromExcel;
        //[PXButton]
        //[PXUIField(DisplayName = "Import from Exel")]
        //protected void importFromExel()
        //{
        //    PXLongOperation.StartOperation(this, () =>
        //    {
        //        var file = UploadFileHelper.UploadFile();
        //    });
        //}
    }
}
