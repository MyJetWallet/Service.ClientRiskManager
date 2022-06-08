using System.ServiceModel;
using System.Threading.Tasks;
using Service.ClientRiskManager.Grpc.Models;

namespace Service.ClientRiskManager.Grpc
{
    [ServiceContract]
    public interface IHelloService
    {
        [OperationContract]
        Task<HelloMessage> SayHelloAsync(HelloRequest request);
    }
}