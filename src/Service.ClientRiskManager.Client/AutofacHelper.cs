using Autofac;
using Service.ClientRiskManager.Grpc;

// ReSharper disable UnusedMember.Global

namespace Service.ClientRiskManager.Client
{
    public static class AutofacHelper
    {
        public static void RegisterClientRiskManagerClient(this ContainerBuilder builder, string grpcServiceUrl)
        {
            var factory = new ClientRiskManagerClientFactory(grpcServiceUrl);

            builder.RegisterInstance(factory.GetHelloService()).As<IHelloService>().SingleInstance();
        }
    }
}
