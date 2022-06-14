using Autofac;
using Autofac.Core;
using Autofac.Core.Registration;
using MyJetWallet.Sdk.ServiceBus;
using Service.Bitgo.DepositDetector.Domain.Models;
using Service.ClientRiskManager.Domain;
using Service.ClientRiskManager.Subscribers;
using Service.Circle.Webhooks.Domain.Models;
using Service.ClientProfile.Client;
using Service.ClientRiskManager.Jobs;

namespace Service.ClientRiskManager.Modules
{
    public class ServiceModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            RegisterServiceBus(builder);
            RegisterSubscribers(builder);
            
            builder.RegisterType<DepositRiskManager>()
                .SingleInstance()
                .As<IDepositRiskManager>()
                .AutoActivate().AsSelf();

            builder.RegisterClientProfileClientWithoutCache(Program.Settings.ClientProfileGrpcServiceUrl);
            
            builder
                .RegisterType<RecalculateRiskBackgroundJob>()
                .As<IStartable>()
                .AutoActivate()
                .SingleInstance();
        }

        private static void RegisterServiceBus(ContainerBuilder builder)
        {
            //Subscribers
            var serviceBusClient = builder.RegisterMyServiceBusTcpClient(
                Program.ReloadedSettings(e => e.SpotServiceBusHostPort),
                Program.LogFactory);
            var queueName = "client-risk-manager";

            builder
                .RegisterMyServiceBusSubscriberSingle<SignalCircleChargeback>(
                    serviceBusClient,
                    SignalCircleChargeback.ServiceBusTopicName,
                    queueName,
                    MyServiceBus.Abstractions.TopicQueueType.Permanent);

            builder
                .RegisterMyServiceBusSubscriberSingle<Deposit>(
                    serviceBusClient,
                    Deposit.TopicName,
                    queueName,
                    MyServiceBus.Abstractions.TopicQueueType.Permanent);
        }

        private static void RegisterSubscribers(ContainerBuilder builder)
        {
            builder.RegisterType<DepositSubscriber>().AutoActivate();
            builder.RegisterType<SignalCircleChargebackSubscriber>().AutoActivate();
        }
    }
}