using ASCJSMCustom.Common.Descriptor;
using ASCJSMCustom.Common.Models;
using ASCJSMCustom.Common.Services.REST.Interfaces;
using ASCJSMCustom.Cost.DAC;
using PX.Data;
using PX.Data.WorkflowAPI;
using System;
using System.Collections;
using System.Collections.Generic;

namespace ASCJSMCustom.Cost
{
    public class ASCJSMSetupMaint : PXGraph<ASCJSMSetupMaint>
    {
        #region DataViews
        public PXSelect<ASCJSMSetup> Setup;
        #endregion

        #region Dependency Injection
        [InjectDependency]
        public IASCJSMRESTService _starRESTService { get; set; }
        #endregion

        #region Actions
        public PXSave<ASCJSMSetup> Save;
        public PXCancel<ASCJSMSetup> Cancel;

        public PXAction<ASCJSMSetup> TestConnection;
        [PXUIField(DisplayName = "Test Connection", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
        [PXButton(CommitChanges = true, Connotation = ActionConnotation.None, DisplayOnMainToolbar = true)]
        public virtual IEnumerable testConnection(PXAdapter adapter)
        {
            try
            {
                var parameters = new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>(ASCJSMQueryParams.Base, Setup.Current.BaseCurrency),
                    new KeyValuePair<string, string>(ASCJSMQueryParams.Symbols, Setup.Current.Symbols),
                };
                _starRESTService.Get<ASCJSMLatestRatesModel>(ASCJSMEndpoints.LatestRates, parameters);
            }
            catch (Exception ex)
            {
                PXTrace.WriteError(ex);
                Setup.Ask("Connection failed", ASCJSMMessages.Connection.TestConnectionFailed, MessageButtons.OK, MessageIcon.Error);
                return adapter.Get();
            }

            Setup.Ask("Success", ASCJSMMessages.Connection.TestConnectionSuccess, MessageButtons.OK, MessageIcon.Information);

            return adapter.Get();
        }
        #endregion
    }
}
