using System.Collections.Generic;
using System.Runtime.Serialization;
using Service.ClientRiskManager.Domain.Models;

namespace Service.ClientRiskManager.Grpc.Models;

[DataContract]
public class GetClientWithdrawalLimitsResponse
{
    [DataMember(Order = 1)] public bool Success { get; set; }
    [DataMember(Order = 2)] public string ErrorMessage { get; set; }
    [DataMember(Order = 3)] public List<AssetBalance> Assets { get; set; }
}