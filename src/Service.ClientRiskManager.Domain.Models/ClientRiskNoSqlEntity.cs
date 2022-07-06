using System;
using System.Collections.Generic;
using System.Linq;
using MyNoSqlServer.Abstractions;
using Service.Circle.Wallets.Domain.Models;

namespace Service.ClientRiskManager.Domain.Models
{
    public class ClientRiskNoSqlEntity : MyNoSqlDbEntity
    {
        public const string TableName = "myjetwallet-clients-risk";

        public static string GeneratePartitionKey(string brokerId) => brokerId;
        public static string GenerateRowKey(string clientId) => clientId;

        public List<CircleClientDeposit> CardDeposits { get; set; }
        public CircleClientDepositSummary CardDepositsSummary { get; set; }

        public static ClientRiskNoSqlEntity Create(string brokerId, string clientId, 
            CircleClientDeposit deposit, CircleCardPaymentDetails  paymentDetails)
        {
            var entity = new ClientRiskNoSqlEntity
            {
                PartitionKey = GeneratePartitionKey(brokerId),
                RowKey = GenerateRowKey(clientId),
                CardDeposits = new List<CircleClientDeposit>() {deposit},
                CardDepositsSummary = new CircleClientDepositSummary
                {
                    DepositLast30DaysInUsd = deposit.BalanceInUsd,
                    DepositLast14DaysInUsd = deposit.BalanceInUsd,
                    DepositLast7DaysInUsd = deposit.BalanceInUsd,
                    DepositLast1DaysInUsd = deposit.BalanceInUsd,
                    Deposit30DaysLimit = paymentDetails.Day30Limit,
                    Deposit7DaysLimit = paymentDetails.Day7Limit,
                    Deposit1DaysLimit = paymentDetails.Day1Limit,
                    Deposit30DaysState = LimitState.None,
                    Deposit7DaysState = LimitState.None,
                    Deposit1DaysState = LimitState.None,
                    BarInterval = BarState.Day1,
                    BarProgres = 0,
                    LeftHours = 0,
                },
            };
            return entity;
        }
    }
}