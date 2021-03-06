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
        
        [YamlProperty("ClientRiskManager.MyNoSqlReaderHostPort")]
        public string MyNoSqlReaderHostPort { get; set; }

        [YamlProperty("ClientRiskManager.MyNoSqlWriterUrl")]
        public string MyNoSqlWriterUrl { get; set; }

        [YamlProperty("ClientRiskManager.SpotServiceBusHostPort")]
        public string SpotServiceBusHostPort { get; set; }

        [YamlProperty("ClientRiskManager.ClientProfileGrpcServiceUrl")]
        public string ClientProfileGrpcServiceUrl { get; set; }

        [YamlProperty("ClientRiskManager.CircleWalletsGrpcServiceUrl")]
        public string CircleWalletsGrpcServiceUrl { get; set; }

        [YamlProperty("ClientRiskManager.DisableCircleFraudDetection")]
        public bool DisableCircleFraudDetection { get; set; } = false;

        [YamlProperty("ClientRiskManager.CardErrorCodesAsFraud")]
        public string CardErrorCodesAsFraud { get; set; }

        [YamlProperty("ClientRiskManager.PaymetErrorCodesAsFraud")]
        public string PaymetErrorCodesAsFraud { get; set; }

        [YamlProperty("ClientRiskManager.BitgoDepositServiceGrpcUrl")]
        public string BitgoDepositServiceGrpcUrl { get; set; }
        
    }
}
