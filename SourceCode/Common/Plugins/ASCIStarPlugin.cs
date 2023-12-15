using ASCJewelryLibrary.Common.Descriptor;
using ASCJewelryLibrary.AP;
using ASCJewelryLibrary.AP.DAC;
using Customization;
using PX.Data;
using System;
using System.Linq;

namespace ASCJewelryLibrary.Common.Plugins
{
    public class ASCJPlugin : CustomizationPlugin
    {
        #region Constants
        private const string _baseUrl = "https://metals-api.com/api/";
        private const string _apiToken = "<api_token>";
        private const string _currency = "USD";
        private const string _symbols = "LBXAG,LBXAUAM,LBXAUPM,XAU,XAG";
        #endregion

        public override void OnPublished() { }

        public override void UpdateDatabase()
        {
            WriteLog(ASCJMessages.Plugin.PluginStart);

            // Metal API
            MetalAPIConnectionPrefDefaulting();

            WriteLog(ASCJMessages.Plugin.PluginEnd);
        }

        /// <summary>
        /// Sets default values for connection preferences in the ASCJ plugin, if the preferences have not already been set.
        /// Queries the ASCJSetup table for existing preferences. If no preferences exist, creates new preferences with default values, saves them to the table, and logs the success.
        /// If an exception occurs during the process, logs the error message.
        /// </summary>
        private void MetalAPIConnectionPrefDefaulting()
        {
            try
            {
                var setupMaint = PXGraph.CreateInstance<ASCJAPMetalRatesSetupMaint>();
                var setup = PXSelect<ASCJAPMetalRatesSetup>.Select(setupMaint).RowCast<ASCJAPMetalRatesSetup>();
                if (!setup.Any())
                {
                    WriteLog(ASCJMessages.Plugin.PluginCreateConnectionPref);
                    setupMaint.Setup.Insert(new ASCJAPMetalRatesSetup()
                    {
                        BaseURL = _baseUrl,
                        BaseCurrency = _currency,
                        AccessKey = _apiToken,
                        Symbols = _symbols,
                    });
                    setupMaint.Save.PressButton();
                    WriteLog(ASCJMessages.Plugin.PluginCreateConnectionPrefSuccess);
                }
            }
            catch (Exception ex)
            {
                WriteLog(string.Format(ASCJMessages.Plugin.PluginCreateConnectionPrefError, ex.Message));
            }
        }
    }
}
