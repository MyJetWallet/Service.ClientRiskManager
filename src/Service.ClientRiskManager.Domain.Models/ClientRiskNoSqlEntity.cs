using System;
using System.Collections.Generic;
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

        public void RecalcDepositsLimitsProgress(CircleCardPaymentDetails paymentDetails)
        {
            CardDepositsSummary.Deposit30DaysLimit = paymentDetails.Day30Limit;
            CardDepositsSummary.Deposit7DaysLimit = paymentDetails.Day7Limit;
            CardDepositsSummary.Deposit1DaysLimit = paymentDetails.Day1Limit;
            CardDepositsSummary.Deposit1DaysState = LimitState.None;
            CardDepositsSummary.Deposit7DaysState = LimitState.None;
            CardDepositsSummary.Deposit30DaysState = LimitState.None;

            //TODO: Add math engine
            // List<Periodic> freeAmounts = new List<Periodic>()
            // {
            //     new Periodic
            //     {
            //         Amount = 0,
            //         Period = PeriodType.Day1
            //     },
            //     new Periodic()
            //     {
            //         Amount = 0,
            //         Period = PeriodType.Day7
            //     },
            //     new Periodic()
            //     {
            //         Amount = 0,
            //         Period = PeriodType.Day30
            //     },
            // };
            // var freeAmount1Day = CardDepositsSummary.Deposit1DaysLimit - CardDepositsSummary.DepositLast1DaysInUsd;
            // if (freeAmount1Day <= 0)
            // {
            //     CardDepositsSummary.Deposit1DaysState = LimitState.Block;
            //     freeAmount1Day = 0m;
            // }
            // freeAmounts.Add(1,freeAmount1Day);
            // var freeAmount7Day = CardDepositsSummary.Deposit7DaysLimit - CardDepositsSummary.DepositLast7DaysInUsd;
            // if (freeAmount7Day <= 0)
            // {
            //     CardDepositsSummary.Deposit7DaysState = LimitState.Block;
            //     freeAmount7Day = 0m;
            // }
            // freeAmounts.Add(7, freeAmount7Day);
            // var freeAmount30Day = CardDepositsSummary.Deposit30DaysLimit - CardDepositsSummary.DepositLast30DaysInUsd;
            // if (freeAmount30Day <= 0)
            // {
            //     CardDepositsSummary.Deposit30DaysState = LimitState.Block;
            //     freeAmount30Day = 0m;
            // }
            //
            // freeAmounts.Add(30, freeAmount30Day);
            // var orderByDescending = freeAmounts.OrderBy(e => e.Value);
            // var minValue = orderByDescending.First();
            //

            //CardDepositsSummary.Deposit30DaysState 
            //CardDepositsSummary.Deposit7DaysState 
            // CardDepositsSummary.Deposit1DaysState 

            CardDepositsSummary.BarInterval = BarState.Day1;
            CardDepositsSummary.BarProgres = 0;
            CardDepositsSummary.LeftHours = 0;
        }
    }
}