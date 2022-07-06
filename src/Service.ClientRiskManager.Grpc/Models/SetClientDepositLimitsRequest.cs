using System.Runtime.Serialization;
using Service.Circle.Wallets.Domain.Models;

namespace Service.ClientRiskManager.Grpc.Models
{
    [DataContract]
    public class SetClientDepositLimitsRequest
    {
        [DataMember(Order = 1)] public string BrokerId { get; set; }
        [DataMember(Order = 2)] public CircleCardPaymentDetails PaymentDetails { get; set; }
    }
}