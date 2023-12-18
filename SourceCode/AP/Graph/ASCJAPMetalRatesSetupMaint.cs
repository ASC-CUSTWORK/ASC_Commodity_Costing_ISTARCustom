using ASCJewelryLibrary.Common.Descriptor;
using ASCJewelryLibrary.Common.Models;
using ASCJewelryLibrary.Common.Services.REST.Interfaces;
using ASCJewelryLibrary.AP.DAC;
using PX.Data;
using PX.Data.WorkflowAPI;
using System;
using System.Collections;
using System.Collections.Generic;

namespace ASCJewelryLibrary.AP
{
    public class ASCJAPMetalRatesSetupMaint : PXGraph<ASCJAPMetalRatesSetupMaint>
    {
        #region DataViews
        public PXSelect<ASCJAPMetalRatesSetup> Setup;
        #endregion

        #region Dependency Injection
        [InjectDependency]
        public IASCJRESTService _starRESTService { get; set; }
        #endregion

        #region Actions
        public PXSave<ASCJAPMetalRatesSetup> Save;
        public PXCancel<ASCJAPMetalRatesSetup> Cancel;

        public PXAction<ASCJAPMetalRatesSetup> TestConnection;
        [PXUIField(DisplayName = "Test Connection", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
        [PXButton(CommitChanges = true, Connotation = ActionConnotation.None, DisplayOnMainToolbar = true)]
        public virtual IEnumerable testConnection(PXAdapter adapter)
        {
            try
            {
                var parameters = new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>(ASCJQueryParams.Base, Setup.Current.BaseCurrency),
                    new KeyValuePair<string, string>(ASCJQueryParams.Symbols, Setup.Current.Symbols),
                };
                _starRESTService.Get<ASCJLatestRatesModel>(ASCJEndpoints.LatestRates, parameters);
            }
            catch (Exception ex)
            {
                PXTrace.WriteError(ex);
                Setup.Ask("Connection failed", ASCJMessages.ASCJConnection.TestConnectionFailed, MessageButtons.OK, MessageIcon.Error);
                return adapter.Get();
            }

            Setup.Ask("Success", ASCJMessages.ASCJConnection.TestConnectionSuccess, MessageButtons.OK, MessageIcon.Information);

            return adapter.Get();
        }
        #endregion
    }
}
