using System.Runtime.Serialization;

namespace Service.ClientRiskManager.Domain.Models;

[DataContract]
public class CircleClientDepositSummary
{
    [DataMember(Order = 1)] public decimal DepositLast30DaysInUsd { get; set; }
    [DataMember(Order = 2)] public decimal DepositLast14DaysInUsd { get; set; }
    [DataMember(Order = 3)] public decimal DepositLast7DaysInUsd { get; set; }
    [DataMember(Order = 4)] public decimal DepositLast1DaysInUsd { get; set; }
    
    [DataMember(Order = 5)] public decimal Deposit30DaysLimit { get; set; }
    [DataMember(Order = 6)] public decimal Deposit7DaysLimit { get; set; }
    [DataMember(Order = 7)] public decimal Deposit1DaysLimit { get; set; }
    
    [DataMember(Order = 8)] public LimitState Deposit30DaysState { get; set; }
    [DataMember(Order = 9)] public LimitState Deposit7DaysState { get; set; }
    [DataMember(Order = 10)] public LimitState Deposit1DaysState { get; set; }

    [DataMember(Order = 11)] public BarState BarInterval { get; set; }
    [DataMember(Order = 12)] public int BarProgres { get; set; }
    [DataMember(Order = 13)] public int LeftHours { get; set; }
}

public enum LimitState
{
    None, // - black label
    Active, // - blue label
    Block // - red label
}
    
public enum BarState
{
    Day1,
    Day7,
    Day30,
}