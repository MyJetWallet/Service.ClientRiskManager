using System.Runtime.Serialization;

namespace Service.ClientRiskManager.Grpc.Models
{
    [DataContract]
    public class GetClientWithdrawalLimitsRequest
    {
        [DataMember(Order = 1)] public string ClientId { get; set; }
    }
}