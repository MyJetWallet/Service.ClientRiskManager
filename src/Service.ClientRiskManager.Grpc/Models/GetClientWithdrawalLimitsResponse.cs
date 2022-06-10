using System.Collections.Generic;
using System.Runtime.Serialization;
using Service.ClientRiskManager.Domain.Models;

namespace Service.ClientRiskManager.Grpc.Models;

[DataContract]
public class GetClientWithdrawalLimitsResponse
{
    // public GetClientWithdrawalLimitsResponse()
    // {
    // }
    //
    // public GetClientWithdrawalLimitsResponse(string errorMessage)
    // {
    //     ErrorMessage = errorMessage;
    // }

    [DataMember(Order = 1)] public bool Success { get; set; }
    [DataMember(Order = 2)] public string ErrorMessage { get; set; }
    [DataMember(Order = 3)] public CircleClientDepositSummary CardDepositsSummary { get; set; }
}
