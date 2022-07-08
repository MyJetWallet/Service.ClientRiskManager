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
         var dayStat = new CircleClientDepositSummary();
         RecalcDeposits(deposits, currDay, dayStat);
         RecalcDepositsLimitsProgress(paymentDetails, dayStat);
        return dayStat;
    }

    private static void RecalcDeposits(List<CircleClientDeposit> deposits, 
        DateTime currDay, CircleClientDepositSummary dayStat)
    {
        dayStat.DepositLast30DaysInUsd = dayStat.DepositLast14DaysInUsd = 
            dayStat.DepositLast7DaysInUsd = dayStat.DepositLast1DaysInUsd = 0m;
        
        dayStat.LastDeposit30DaysLeftHours = dayStat.LastDeposit7DaysLeftHours = 
            dayStat.LastDeposit1DaysLeftHours = Convert.ToInt32((currDay - currDay.AddMonths(-1)).TotalHours);
        
        foreach (var cardDeposit in deposits)
        {
            if (cardDeposit.Date >= currDay.AddMonths(-1))
            {
                dayStat.DepositLast30DaysInUsd += cardDeposit.BalanceInUsd;
                
                var leftHours = Convert.ToInt32((cardDeposit.Date - currDay.AddMonths(-1)).TotalHours);
                if (dayStat.LastDeposit30DaysLeftHours > leftHours)
                {
                    dayStat.LastDeposit30DaysLeftHours = leftHours;
                }
            }

            if (cardDeposit.Date >= currDay.AddDays(-14))
            {
                dayStat.DepositLast14DaysInUsd += cardDeposit.BalanceInUsd;
            }

            if (cardDeposit.Date >= currDay.AddDays(-7))
            {
                dayStat.DepositLast7DaysInUsd += cardDeposit.BalanceInUsd;
                
                var leftHours = Convert.ToInt32((cardDeposit.Date - currDay.AddDays(-7)).TotalHours);
                if (dayStat.LastDeposit7DaysLeftHours > leftHours)
                {
                    dayStat.LastDeposit7DaysLeftHours = leftHours;
                }
            }

            if (cardDeposit.Date >= currDay.AddDays(-1))
            {
                dayStat.DepositLast1DaysInUsd += cardDeposit.BalanceInUsd;
                
                var leftHours = Convert.ToInt32((cardDeposit.Date - currDay.AddDays(-1)).TotalHours);
                if (dayStat.LastDeposit1DaysLeftHours > leftHours)
                {
                    dayStat.LastDeposit1DaysLeftHours = leftHours;
                }
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
            dayStat.Deposit1DaysLimit, BarState.Day1, dayStat.LastDeposit1DaysLeftHours);
        var day7 = new DepositDayStat(dayStat.DepositLast7DaysInUsd,
            dayStat.Deposit7DaysLimit, BarState.Day7, dayStat.LastDeposit7DaysLeftHours);
        var day30 = new DepositDayStat(dayStat.DepositLast30DaysInUsd,
            dayStat.Deposit30DaysLimit, BarState.Day30, dayStat.LastDeposit30DaysLeftHours);

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
        
        // Find leftHours from Max blocked interval
        var leftHoursFromMaxBlockedInterval = dayLimits
            .Where(e => e.State == LimitState.Block)
            .OrderByDescending(e => e.LeftHours)
            .FirstOrDefault();

        var leftHours = leftHoursFromMaxBlockedInterval?.LeftHours ?? 0;
        dayStat.LeftHours = leftHours;
    }
}