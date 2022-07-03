using Autofac;
using MyJetWallet.Sdk.NoSql;
using MyNoSqlServer.Abstractions;
using MyNoSqlServer.DataReader;
using Service.ClientRiskManager.Domain.Models;
using Service.ClientRiskManager.Grpc;

// ReSharper disable UnusedMember.Global

namespace Service.ClientRiskManager.Client
{
    public static class AutofacHelper
    {
        public static void RegisterClientRiskManagerClient(this ContainerBuilder builder, string grpcServiceUrl)
        {
            var factory = new ClientRiskManagerClientFactory(grpcServiceUrl);

            builder.RegisterInstance(factory.GetClientLimitsRiskService())
                .As<IClientLimitsRiskService>()
                .SingleInstance();
        }
        
        public static void RegisterClientRiskManagerCachedClient(this ContainerBuilder builder, string grpcServiceUrl, IMyNoSqlSubscriber noSqlSubscriber)
        {
            var factory = new ClientRiskManagerClientFactory(grpcServiceUrl);

            builder.RegisterInstance(factory.GetClientLimitsRiskService())
                .As<IClientLimitsRiskService>()
                .SingleInstance();
            
            IMyNoSqlServerDataReader<ClientRiskNoSqlEntity> earnOfferDtoReader = builder
                .RegisterMyNoSqlReader<ClientRiskNoSqlEntity>(noSqlSubscriber, ClientRiskNoSqlEntity.TableName);

            var client = new ClientLimitsRiskServiceCachedClient(factory.GetClientLimitsRiskService(), earnOfferDtoReader);

            builder.RegisterInstance(client)
                .As<IClientLimitsRiskService>()
                .SingleInstance();
        }
    }
}
