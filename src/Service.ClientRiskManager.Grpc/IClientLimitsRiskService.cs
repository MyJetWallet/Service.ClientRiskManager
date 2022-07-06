using System.ServiceModel;
using System.Threading.Tasks;
using Service.ClientRiskManager.Grpc.Models;

namespace Service.ClientRiskManager.Grpc
{
    [ServiceContract]
    public interface IClientLimitsRiskService
    {
        [OperationContract]
        Task<GetClientWithdrawalLimitsResponse> GetClientWithdrawalLimitsAsync(
            GetClientWithdrawalLimitsRequest request);
        
        [OperationContract]
        Task SetClientDepositLimitsAsync(
            SetClientDepositLimitsRequest request);
        

    }
}