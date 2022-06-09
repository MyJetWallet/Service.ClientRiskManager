using Autofac;
using Autofac.Core;
using Autofac.Core.Registration;
using MyJetWallet.Sdk.ServiceBus;
using Service.ClientRiskManager.Domain;
using Service.ClientRiskManager.Subscribers;
using Service.Circle.Webhooks.Domain.Models;
using Service.ClientProfile.Client;

namespace Service.ClientRiskManager.Modules
{
    public class ServiceModule: Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            RegisterServiceBus(builder);
            RegisterSubscribers(builder);
            builder.RegisterType<DepositRiskManager>().SingleInstance().As<IDepositRiskManager>().AutoActivate().AsSelf();

            builder
               .RegisterMyServiceBusSubscriberSingle<SignalCircleChargeback>(
                   serviceBusClient,
                   SignalCircleChargeback.ServiceBusTopicName,
                   "client-risk-manager",
                   MyServiceBus.Abstractions.TopicQueueType.Permanent);

            builder.RegisterClientProfileClientWithoutCache(Program.Settings.ClientProfileGrpcServiceUrl);
        }

        private static void RegisterServiceBus(ContainerBuilder builder)
        {
            //Subscribers
            var serviceBusClient = builder.RegisterMyServiceBusTcpClient(
                Program.ReloadedSettings(e => e.SpotServiceBusHostPort),
                Program.LogFactory);
        }

        private static void RegisterSubscribers(ContainerBuilder builder)
        {
            builder.RegisterType<DepositSubscriber>().As<IStartable>().SingleInstance().AutoActivate();
        }
    }
}