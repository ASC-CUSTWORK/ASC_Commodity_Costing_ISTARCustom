using ASCJSMCustom.Common.Descriptor;
using ASCJSMCustom.Cost;
using ASCJSMCustom.Cost.DAC;
using Customization;
using PX.Data;
using System;
using System.Linq;

namespace ASCJSMCustom.Common.Plugins
{
    public class ASCJSMPlugin : CustomizationPlugin
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
            WriteLog(ASCJSMMessages.Plugin.PluginStart);

            // Metal API
            MetalAPIConnectionPrefDefaulting();

            WriteLog(ASCJSMMessages.Plugin.PluginEnd);
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
                var setupMaint = PXGraph.CreateInstance<ASCJSMSetupMaint>();
                var setup = PXSelect<ASCJSMSetup>.Select(setupMaint).RowCast<ASCJSMSetup>();
                if (!setup.Any())
                {
                    WriteLog(ASCJSMMessages.Plugin.PluginCreateConnectionPref);
                    setupMaint.Setup.Insert(new ASCJSMSetup()
                    {
                        BaseURL = _baseUrl,
                        BaseCurrency = _currency,
                        AccessKey = _apiToken,
                        Symbols = _symbols,
                    });
                    setupMaint.Save.PressButton();
                    WriteLog(ASCJSMMessages.Plugin.PluginCreateConnectionPrefSuccess);
                }
            }
            catch (Exception ex)
            {
                WriteLog(string.Format(ASCJSMMessages.Plugin.PluginCreateConnectionPrefError, ex.Message));
            }
        }
    }
}
