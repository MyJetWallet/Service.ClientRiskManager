using System.Runtime.Serialization;

namespace Service.ClientRiskManager.Domain.Models;

[DataContract]
public class CircleClientDepositSummary
{
    [DataMember(Order = 1)] public decimal DepositLast30DaysInUsd { get; set; }
    [DataMember(Order = 2)] public decimal DepositLast14DaysInUsd { get; set; }
    [DataMember(Order = 3)] public decimal DepositLast7DaysInUsd { get; set; }
    [DataMember(Order = 4)] public decimal DepositLast1DaysInUsd { get; set; }
}