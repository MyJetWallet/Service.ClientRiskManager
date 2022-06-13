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
        
        // public static ClientRiskNoSqlEntity Create(string brokerId, string clientId, List<CircleClientDeposit> deposits)
        // {
        //     
        //     var (depositLast30DaysInUsd, depositLast14DaysInUsd, depositLast7DaysInUsd, depositLast1DaysInUsd) = CalcDepositLastDaysInUsd(deposits);
        //
        //     var entity = new ClientRiskNoSqlEntity
        //     {
        //         PartitionKey = GeneratePartitionKey(brokerId),
        //         RowKey = GenerateRowKey(clientId),
        //         //TimeStamp = null,
        //         //Expires = null,
        //         CardDeposits = deposits,
        //         DepositLast30DaysInUsd = depositLast30DaysInUsd,
        //         DepositLast14DaysInUsd = depositLast14DaysInUsd,
        //         DepositLast7DaysInUsd = depositLast7DaysInUsd,
        //         DepositLast1DaysInUsd = depositLast1DaysInUsd,
        //
        //     };
        //     return entity;
        // }
        //
        // private static (decimal, decimal, decimal, decimal) CalcDepositLastDaysInUsd(List<CircleClientDeposit> cardDeposits)
        // {
        //     var depositLast30DaysInUsd = 0m;
        //     var depositLast14DaysInUsd = 0m;
        //     var depositLast7DaysInUsd = 0m;
        //     var depositLast1DaysInUsd = 0m;
        //     var currDay = DateTime.UtcNow;
        //     foreach (var cardDeposit in cardDeposits)
        //     {
        //         if (cardDeposit.Date >= currDay.AddMonths(-1))
        //         {
        //             depositLast30DaysInUsd += cardDeposit.BalanceInUsd;
        //         }
        //         if (cardDeposit.Date >= currDay.AddDays(-14))
        //         {
        //             depositLast14DaysInUsd += cardDeposit.BalanceInUsd;
        //         }
        //         if (cardDeposit.Date >= currDay.AddDays(-7))
        //         {
        //             depositLast7DaysInUsd += cardDeposit.BalanceInUsd;
        //         }
        //         if (cardDeposit.Date >= currDay.AddDays(-1))
        //         {
        //             depositLast1DaysInUsd += cardDeposit.BalanceInUsd;
        //         }
        //     }
        //     return (depositLast30DaysInUsd, depositLast14DaysInUsd, depositLast7DaysInUsd, depositLast1DaysInUsd);
        // }

        public void CleanupDepositsLess30Days(DateTime currDay)
        {
            foreach (var cardDeposit in CardDeposits)
            {
                if (cardDeposit.Date < currDay.AddMonths(-1))
                {
                    CardDeposits.Remove(cardDeposit);
                }
            }
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