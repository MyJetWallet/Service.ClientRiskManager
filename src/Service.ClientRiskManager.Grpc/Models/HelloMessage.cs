using System.Runtime.Serialization;
using Service.ClientRiskManager.Domain.Models;

namespace Service.ClientRiskManager.Grpc.Models
{
    [DataContract]
    public class HelloMessage : IHelloMessage
    {
        [DataMember(Order = 1)]
        public string Message { get; set; }
    }
}