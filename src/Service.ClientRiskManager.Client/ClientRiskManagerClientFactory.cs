using JetBrains.Annotations;
using MyJetWallet.Sdk.Grpc;
using Service.ClientRiskManager.Grpc;

namespace Service.ClientRiskManager.Client
{
    [UsedImplicitly]
    public class ClientRiskManagerClientFactory: MyGrpcClientFactory
    {
        public ClientRiskManagerClientFactory(string grpcServiceUrl) : base(grpcServiceUrl)
        {
        }

        public IClientLimitsRiskService GetClientLimitsRiskService() => CreateGrpcService<IClientLimitsRiskService>();
    }
}
