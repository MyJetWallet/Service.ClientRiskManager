using MyJetWallet.Sdk.Service;
using MyYamlParser;

namespace Service.ClientRiskManager.Settings
{
    public class SettingsModel
    {
        [YamlProperty("ClientRiskManager.SeqServiceUrl")]
        public string SeqServiceUrl { get; set; }

        [YamlProperty("ClientRiskManager.ZipkinUrl")]
        public string ZipkinUrl { get; set; }

        [YamlProperty("ClientRiskManager.ElkLogs")]
        public LogElkSettings ElkLogs { get; set; }
    }
}
