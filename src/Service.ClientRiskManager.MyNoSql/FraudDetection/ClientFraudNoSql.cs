using MyJetWallet.Fireblocks.Domain.Models.VaultAssets;
using MyNoSqlServer.Abstractions;
using Service.ClientRiskManager.Domain.Models.FraudDetection;

namespace Service.ClientRiskManager.MyNoSql.FraudDetection
{
    public class ClientFraudNoSql : MyNoSqlDbEntity
    {
        public const string TableName = "myjetwallet-client-risk-management-fraud-detection";
        public static string GeneratePartitionKey(string clientId) => $"{clientId}";
        public static string GenerateRowKey(string type) =>
            $"{type}";

        public ClientFraud ClietFraud { get; set; }

        public static ClientFraudNoSql Create(
            ClientFraud clientFraud)
        {
            return new ClientFraudNoSql()
            {
                PartitionKey = GeneratePartitionKey(clientFraud.ClientId),
                RowKey = GenerateRowKey(clientFraud.Type),
                ClietFraud = clientFraud,
            };
        }

    }
}
