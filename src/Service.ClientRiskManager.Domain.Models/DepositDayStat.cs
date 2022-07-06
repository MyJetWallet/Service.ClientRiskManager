using System;

namespace Service.ClientRiskManager.Domain.Models;

public class DepositDayStat
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

    public DepositDayStat(decimal amount, decimal limit, BarState day)
    {
        Amount = amount;
        Limit = limit;
        Day = day;
        State = Limit <= Amount ? LimitState.Block : LimitState.None; 
    }
}