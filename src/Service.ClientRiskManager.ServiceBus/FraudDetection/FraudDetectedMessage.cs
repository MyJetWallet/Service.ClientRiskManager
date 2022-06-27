using Service.ClientRiskManager.Domain.Models.FraudDetection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Service.ClientRiskManager.ServiceBus.FraudDetection
{
    [DataContract]
    public class FraudDetectedMessage
    {

        public const string TopicName = "service-client-risk-management-fraud-detected";

        [DataMember(Order =1)]
        public ClientFraud? ClientFraud { get; set; }
    }
}
