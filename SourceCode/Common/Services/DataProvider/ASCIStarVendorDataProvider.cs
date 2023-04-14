using ASCISTARCustom.Common.Descriptor;
using ASCISTARCustom.Common.Services.DataProvider.Interfaces;
using PX.Data;
using PX.Objects.AP;
using System;
namespace ASCISTARCustom.Common.Services.DataProvider
{
    public class ASCIStarVendorDataProvider : IASCIStarVendorDataProvider
    {
        private readonly PXGraph _graph;

        public ASCIStarVendorDataProvider(PXGraph graph)
        {
            _graph = graph;
        }

        public APVendorPrice GetAPVendorPrice(int? bAccountID, int? inventoryID, string UOM, DateTime effectiveDate, bool withException = false)
        {
            var result = PXSelectBase<
                APVendorPrice, PXSelect<
                APVendorPrice,
                Where<APVendorPrice.inventoryID, Equal<Required<APVendorPrice.inventoryID>>,
                    And<APVendorPrice.uOM, Equal<Required<APVendorPrice.uOM>>,
                    And<APVendorPrice.effectiveDate, LessEqual<Required<APVendorPrice.effectiveDate>>,
                    And<APVendorPrice.vendorID, Equal<Required<APVendorPrice.vendorID>>>>>>>.Config>
                .Select(_graph, inventoryID, UOM, effectiveDate, bAccountID);

            if (result == null && withException == true)
            {
                throw new PXException(ASCIStarMessages.Error.VendorPriceNotFound);
            }

            return result;
        }

        public Vendor GetVendor(int? bAccountID, bool withException = false)
        {
            var result = PXSelectBase<
                Vendor, PXSelect<
                Vendor,
                Where<Vendor.bAccountID, Equal<Required<Vendor.bAccountID>>>>.Config>
                .Select(_graph, bAccountID);
            if (result == null)
            {
                throw new PXException(ASCIStarMessages.Error.VendorRecordNotFound);
            }

            return result;
        }
    }
}
