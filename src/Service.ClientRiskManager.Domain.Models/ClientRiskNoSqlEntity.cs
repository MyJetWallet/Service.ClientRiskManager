using System.Collections.Generic;
using MyNoSqlServer.Abstractions;

namespace Service.ClientRiskManager.Domain.Models
{
    public class ClientRiskNoSqlEntity : MyNoSqlDbEntity
    {
        public const string TableName = "myjetwallet-clients-risk";

        public static string GeneratePartitionKey(string brokerId) => brokerId;
        public static string GenerateRowKey(string clientId) => clientId;

        public List<AssetBalance> Cards { get; set; }

        public static ClientRiskNoSqlEntity Create(string brokerId, string clientId, List<AssetBalance> cards)
        {
            return new ClientRiskNoSqlEntity()
            {
                PartitionKey = GeneratePartitionKey(brokerId),
                RowKey = GenerateRowKey(clientId),
                Cards = cards
            };
        }
    }
}