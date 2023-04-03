using ASCISTARCustom.Common.Descriptor;
using ASCISTARCustom.Preferences;
using ASCISTARCustom.Preferences.DAC;
using Customization;
using PX.Data;
using System;
using System.Linq;

namespace ASCISTARCustom.Common.Plugins
{
    public class ASCIStarPlugin : CustomizationPlugin
    {
        #region Constants
        private const string _baseUrl = "https://metals-api.com/api/";
        private const string _apiToken = "<api_token>";
        private const string _currency = "USD";
        private const string _symbols = "LBXAUAM,LBXAUPM,XAU,XAG";
        #endregion

        public override void OnPublished() { }

        public override void UpdateDatabase()
        {
            WriteLog(ASCIStarMessages.Plugin.PluginStart);

            // Metal API
            MetalAPIConnectionPrefDefaulting();

            WriteLog(ASCIStarMessages.Plugin.PluginEnd);
        }

        /// <summary>
        /// Sets default values for connection preferences in the ASCIStar plugin, if the preferences have not already been set.
        /// Queries the ASCIStarSetup table for existing preferences. If no preferences exist, creates new preferences with default values, saves them to the table, and logs the success.
        /// If an exception occurs during the process, logs the error message.
        /// </summary>
        private void MetalAPIConnectionPrefDefaulting()
        {
            try
            {
                var setupMaint = PXGraph.CreateInstance<ASCIStarSetupMaint>();
                var setup = PXSelect<ASCIStarSetup>.Select(setupMaint).RowCast<ASCIStarSetup>();
                if (!setup.Any())
                {
                    WriteLog(ASCIStarMessages.Plugin.PluginCreateConnectionPref);
                    setupMaint.Setup.Insert(new ASCIStarSetup()
                    {
                        BaseURL = _baseUrl,
                        BaseCurrency = _currency,
                        AccessKey = _apiToken,
                        Symbols = _symbols,
                    });
                    setupMaint.Save.PressButton();
                    WriteLog(ASCIStarMessages.Plugin.PluginCreateConnectionPrefSuccess);
                }
            }
            catch (Exception ex)
            {
                WriteLog(string.Format(ASCIStarMessages.Plugin.PluginCreateConnectionPrefError, ex.Message));
            }
        }
    }
}
