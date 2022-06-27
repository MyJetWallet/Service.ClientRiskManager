using System;
using System.Collections.Generic;
using MyNoSqlServer.Abstractions;

namespace Service.ClientRiskManager.Domain.Models
{
    public class ClientRiskNoSqlEntity : MyNoSqlDbEntity
    {
        public const string TableName = "myjetwallet-clients-risk";

        public static string GeneratePartitionKey(string brokerId) => brokerId;
        public static string GenerateRowKey(string clientId) => clientId;

        public List<CircleClientDeposit> CardDeposits { get; set; }
        public CircleClientDepositSummary CardDepositsSummary { get; set; }
        public decimal MinDepositAmountInUsd { get; set; }
        public static ClientRiskNoSqlEntity Create(string brokerId, string clientId, CircleClientDeposit deposit, decimal minDepositAmount = 0m)
        {
            var entity = new ClientRiskNoSqlEntity
            {
                PartitionKey = GeneratePartitionKey(brokerId),
                RowKey = GenerateRowKey(clientId),
                CardDeposits = new List<CircleClientDeposit>() {deposit},
                CardDepositsSummary = new CircleClientDepositSummary()
                {
                    DepositLast30DaysInUsd = deposit.BalanceInUsd,
                    DepositLast14DaysInUsd = deposit.BalanceInUsd,
                    DepositLast7DaysInUsd = deposit.BalanceInUsd,
                    DepositLast1DaysInUsd = deposit.BalanceInUsd,
                },
                MinDepositAmountInUsd = minDepositAmount
            };
            return entity;
        }
        
        public void CleanupDepositsLess30Days(DateTime currDay)
        {
            var toRemove = new List<CircleClientDeposit>();
            foreach (var cardDeposit in CardDeposits)
            {
                if (cardDeposit.Date < currDay.AddMonths(-1))
                {
                    toRemove.Add(cardDeposit);
                }
            }
            CardDeposits.RemoveAll(toRemove.Contains);
        }

        public void RecalcDeposits(DateTime currDay)
        {
            CardDepositsSummary.DepositLast30DaysInUsd = 0m;
            CardDepositsSummary.DepositLast14DaysInUsd = 0m; 
            CardDepositsSummary.DepositLast7DaysInUsd = 0m;
            CardDepositsSummary.DepositLast1DaysInUsd = 0m;    
            
            foreach (var cardDeposit in CardDeposits)
            {
                if (cardDeposit.Date >= currDay.AddMonths(-1))
                {
                    CardDepositsSummary.DepositLast30DaysInUsd += cardDeposit.BalanceInUsd;
                }
                if (cardDeposit.Date >= currDay.AddDays(-14))
                {
                    CardDepositsSummary.DepositLast14DaysInUsd += cardDeposit.BalanceInUsd;
                }
                if (cardDeposit.Date >= currDay.AddDays(-7))
                {
                    CardDepositsSummary.DepositLast7DaysInUsd += cardDeposit.BalanceInUsd;
                }
                if (cardDeposit.Date >= currDay.AddDays(-1))
                {
                    CardDepositsSummary.DepositLast1DaysInUsd += cardDeposit.BalanceInUsd;
                }
            }
        }
    }
}