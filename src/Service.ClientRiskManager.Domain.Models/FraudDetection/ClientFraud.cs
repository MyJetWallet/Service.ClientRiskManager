using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Service.ClientRiskManager.Domain.Models.FraudDetection
{
    [DataContract]
    public class ClientFraud
    {
        [DataMember(Order =1)]
        public string ClientId { get; set; }

        [DataMember(Order = 2)]
        public string Type { get; set; }

        [DataMember(Order = 3)]
        public bool CardFraudDetected { get; set; }

        [DataMember(Order = 4)]
        public int Attempts3dsFailedCount { get; set; }
    }
}
