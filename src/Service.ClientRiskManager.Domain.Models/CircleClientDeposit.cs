using System;
using System.Runtime.Serialization;

namespace Service.ClientRiskManager.Domain.Models;

[DataContract]
public class CircleClientDeposit
{
    [DataMember(Order = 1)] public DateTime Date { get; set; }
    [DataMember(Order = 2)] public decimal Balance { get; set; }
    [DataMember(Order = 3)] public decimal BalanceInUsd { get; set; }
    [DataMember(Order = 4)] public string AssetSymbol { get; set; }
}