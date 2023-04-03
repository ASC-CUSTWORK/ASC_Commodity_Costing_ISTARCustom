using ASCISTARCustom.Common.Descriptor;
using ASCISTARCustom.Common.Models;
using ASCISTARCustom.Common.Services.Interfaces;
using ASCISTARCustom.Preferences.DAC;
using PX.Data;
using PX.Data.WorkflowAPI;
using System;
using System.Collections;
using System.Collections.Generic;

namespace ASCISTARCustom.Preferences
{
    public class ASCIStarSetupMaint : PXGraph<ASCIStarSetupMaint>
    {
        #region DataViews
        public PXSelect<ASCIStarSetup> Setup;
        #endregion

        #region Dependency Injection
        [InjectDependency]
        public IASCIStarRESTService _starRESTService { get; set; }
        #endregion

        #region Actions
        public PXSave<ASCIStarSetup> Save;
        public PXCancel<ASCIStarSetup> Cancel;

        public PXAction<ASCIStarSetup> TestConnection;
        [PXUIField(DisplayName = "Test Connection", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
        [PXButton(CommitChanges = true, Connotation = ActionConnotation.None, DisplayOnMainToolbar = true)]
        public virtual IEnumerable testConnection(PXAdapter adapter)
        {
            try
            {
                var parameters = new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>(ASCIStarQueryParams.Base, Setup.Current.BaseCurrency),
                    new KeyValuePair<string, string>(ASCIStarQueryParams.Symbols, Setup.Current.Symbols),
                };
                _starRESTService.Get<ASCIStarLatestRatesModel>(ASCIStarEndpoints.LatestRates, parameters);
            }
            catch (Exception ex)
            {
                PXTrace.WriteError(ex);
                Setup.Ask("Connection failed", ASCIStarMessages.Connection.TestConnectionFailed, MessageButtons.OK, MessageIcon.Error);
                return adapter.Get();
            }

            Setup.Ask("Success", ASCIStarMessages.Connection.TestConnectionSuccess, MessageButtons.OK, MessageIcon.Information);

            return adapter.Get();
        }
        #endregion
    }
}
