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
            CardDepositsSummary.Deposit1DaysLimit = paymentDetails.Day1Limit;
            CardDepositsSummary.Deposit7DaysLimit = paymentDetails.Day7Limit;
            CardDepositsSummary.Deposit30DaysLimit = paymentDetails.Day30Limit;

            var day1 = new DayLimit(CardDepositsSummary.DepositLast1DaysInUsd,
                CardDepositsSummary.Deposit1DaysLimit, BarState.Day1);
            var day7 = new DayLimit(CardDepositsSummary.DepositLast7DaysInUsd,
                CardDepositsSummary.Deposit7DaysLimit, BarState.Day7);
            var day30 = new DayLimit(CardDepositsSummary.DepositLast30DaysInUsd,
                CardDepositsSummary.Deposit30DaysLimit, BarState.Day30);

            DayLimit[] dayLimits = { day1, day7, day30 };
            // Find active DayXXXState
            var dayActive = dayLimits
                .Where(e => e.State != LimitState.Block)
                .OrderBy(e => e.AvailableAmount)
                .First();
            
            dayActive.State = LimitState.Active;
            
            CardDepositsSummary.BarInterval = dayActive.Day;
            CardDepositsSummary.BarProgres = dayActive.CalcProgressBar();
            CardDepositsSummary.LeftHours = 0;
        }
    }

    public class DayLimit
    {
        public readonly decimal Amount;
        public readonly decimal Limit;
        public readonly BarState Day;
        public LimitState State { get; set; }
        public decimal AvailableAmount => Limit <= Amount ? 0m : Limit - Amount;

        public int CalcProgressBar()
        {
            if (Limit == 0 || AvailableAmount == 0)
                return 0; 
            
            return Convert.ToInt32(Amount * 100 / Limit);
        }

        public DayLimit(decimal amount, decimal limit, BarState day)
        {
            Amount = amount;
            Limit = limit;
            Day = day;
            State = Limit <= Amount ? LimitState.Block : LimitState.None; 
        }
    }

}