using System.Runtime.Serialization;

namespace Service.ClientRiskManager.Domain.Models;

[DataContract]
public class AssetBalance
{
    [DataMember(Order = 1)] public string Asset { get; set; }
    [DataMember(Order = 2)] public decimal ClientBalance { get; set; }
    [DataMember(Order = 3)] public decimal ClientBalanceInUsd { get; set; }
    [DataMember(Order = 4)] public decimal CircleCardBalance { get; set; }
    [DataMember(Order = 5)] public decimal CircleCardBalanceInUsd { get; set; }
}