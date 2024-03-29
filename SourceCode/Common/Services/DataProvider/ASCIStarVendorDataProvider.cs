﻿using ASCISTARCustom.Common.Descriptor;
using ASCISTARCustom.Common.Services.DataProvider.Interfaces;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Data.BQL;
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

        public APVendorPrice GetAPVendorPrice(int? bAccountID, int? inventoryID, string UOM, DateTime? effectiveDate, bool withException = false)
        {
            var result = SelectFrom<APVendorPrice>
            .Where<APVendorPrice.vendorID.IsEqual<P.AsInt>
                .And<APVendorPrice.inventoryID.IsEqual<P.AsInt>
                    .And<APVendorPrice.uOM.IsEqual<P.AsString>
                       .And<Brackets<APVendorPrice.effectiveDate.IsLessEqual<P.AsDateTime>.Or<APVendorPrice.effectiveDate.IsNull>>
                         .And<Brackets<APVendorPrice.expirationDate.IsGreaterEqual<P.AsDateTime>.Or<APVendorPrice.expirationDate.IsNull>>>>>>>
            .OrderBy<APVendorPrice.effectiveDate.Desc>
            .View.Select(_graph, bAccountID, inventoryID, UOM, effectiveDate, effectiveDate)?.TopFirst;

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
