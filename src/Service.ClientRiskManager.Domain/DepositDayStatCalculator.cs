using System;
using System.Collections.Generic;
using System.Linq;
using Service.Circle.Wallets.Domain.Models;
using Service.ClientRiskManager.Domain.Models;

namespace Service.ClientRiskManager.Domain;

public static class DepositDayStatCalculator
{
    public static CircleClientDepositSummary PrepareDepositStat(CircleCardPaymentDetails paymentDetails, 
        List<CircleClientDeposit> deposits, DateTime currDay)
    {
        var dayStat = new CircleClientDepositSummary()
        {
            DepositLast30DaysInUsd = 0m,
            DepositLast14DaysInUsd = 0m,
            DepositLast7DaysInUsd = 0m,
            DepositLast1DaysInUsd = 0m,
        };
        
         RecalcDeposits(deposits, currDay, dayStat);
         RecalcDepositsLimitsProgress(paymentDetails, dayStat);
        return dayStat;
    }

    private static void RecalcDeposits(List<CircleClientDeposit> deposits, 
        DateTime currDay, CircleClientDepositSummary dayStat)
    {
            dayStat.DepositLast30DaysInUsd = 0m;
            dayStat.DepositLast14DaysInUsd = 0m;
            dayStat.DepositLast7DaysInUsd = 0m;
            dayStat.DepositLast1DaysInUsd = 0m;

        foreach (var cardDeposit in deposits)
        {
            if (cardDeposit.Date >= currDay.AddMonths(-1))
            {
                dayStat.DepositLast30DaysInUsd += cardDeposit.BalanceInUsd;
            }

            if (cardDeposit.Date >= currDay.AddDays(-14))
            {
                dayStat.DepositLast14DaysInUsd += cardDeposit.BalanceInUsd;
            }

            if (cardDeposit.Date >= currDay.AddDays(-7))
            {
                dayStat.DepositLast7DaysInUsd += cardDeposit.BalanceInUsd;
            }

            if (cardDeposit.Date >= currDay.AddDays(-1))
            {
                dayStat.DepositLast1DaysInUsd += cardDeposit.BalanceInUsd;
            }
        }
    }

    private static void RecalcDepositsLimitsProgress(CircleCardPaymentDetails paymentDetails, 
        CircleClientDepositSummary dayStat)
    {
        dayStat.Deposit1DaysLimit = paymentDetails.Day1Limit;
        dayStat.Deposit7DaysLimit = paymentDetails.Day7Limit;
        dayStat.Deposit30DaysLimit = paymentDetails.Day30Limit;

        var day1 = new DepositDayStat(dayStat.DepositLast1DaysInUsd,
            dayStat.Deposit1DaysLimit, BarState.Day1);
        var day7 = new DepositDayStat(dayStat.DepositLast7DaysInUsd,
            dayStat.Deposit7DaysLimit, BarState.Day7);
        var day30 = new DepositDayStat(dayStat.DepositLast30DaysInUsd,
            dayStat.Deposit30DaysLimit, BarState.Day30);

        DepositDayStat[] dayLimits = { day1, day7, day30 };
        // Find active DayXXXState
        var dayActive = dayLimits
            .Where(e => e.State != LimitState.Block)
            .OrderBy(e => e.AvailableAmount)
            .FirstOrDefault();
        
        if (dayActive == null)
        {
            dayStat.BarInterval = day30.Day;
            dayStat.BarProgres = 100;
        }
        else
        {
            dayActive.State = LimitState.Active;
            dayStat.BarInterval = dayActive.Day;
            dayStat.BarProgres = dayActive.CalcProgressBar();
        }
        
        dayStat.Deposit1DaysState = day1.State;
        dayStat.Deposit7DaysState = day7.State;
        dayStat.Deposit30DaysState = day30.State;
        dayStat.LeftHours = 0;
    }
}