using Autofac;
using MyJetWallet.Sdk.ServiceBus;
using MyServiceBus.Abstractions;
using Service.Bitgo.DepositDetector.Domain.Models;
using Service.Circle.Webhooks.Domain.Models;
using Service.ClientRiskManager.ServiceBus.FraudDetection;
using Service.ClientRiskManager.Subscribers;
using Service.Unlimint.Webhooks.Client;

namespace Service.ClientRiskManager.Modules
{
    public class ServiceBusModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var queueName = "client-risk-manager";

            var serviceBusClient = builder.RegisterMyServiceBusTcpClient(
                Program.ReloadedSettings(e => e.SpotServiceBusHostPort),
                Program.LogFactory);

            builder.RegisterSignalUnlimintTransferSubscriber(serviceBusClient, queueName,
                TopicQueueType.Permanent);

            builder.RegisterMyServiceBusPublisher<FraudDetectedMessage>(serviceBusClient, FraudDetectedMessage.TopicName, false);

            builder.RegisterMyServiceBusSubscriberSingle<FraudDetectedMessage>(serviceBusClient,
                FraudDetectedMessage.TopicName,
                queueName,
                MyServiceBus.Abstractions.TopicQueueType.Permanent);

            builder
                .RegisterMyServiceBusSubscriberSingle<SignalCircleChargeback>(
                    serviceBusClient,
                    SignalCircleChargeback.ServiceBusTopicName,
                    queueName,
                    MyServiceBus.Abstractions.TopicQueueType.Permanent);

            builder
                .RegisterMyServiceBusSubscriberSingle<SignalCircleCard>(
                    serviceBusClient,
                    SignalCircleCard.ServiceBusTopicName,
                    queueName,
                    MyServiceBus.Abstractions.TopicQueueType.Permanent);

            builder
                .RegisterMyServiceBusSubscriberSingle<SignalCircleTransfer>(
                    serviceBusClient,
                    SignalCircleTransfer.ServiceBusTopicName,
                    queueName,
                    MyServiceBus.Abstractions.TopicQueueType.Permanent);

            builder
                .RegisterMyServiceBusSubscriberSingle<Deposit>(
                    serviceBusClient,
                    Deposit.TopicName,
                    queueName,
                    MyServiceBus.Abstractions.TopicQueueType.Permanent);

            builder.RegisterType<DepositSubscriber>().AutoActivate().SingleInstance();
            builder.RegisterType<SignalCircleChargebackSubscriber>().AutoActivate().SingleInstance();
            builder.RegisterType<SignalCircleCardSubscriber>().AutoActivate().SingleInstance();
            builder.RegisterType<SignalCircleTransferSubscriber>().AutoActivate().SingleInstance();
            builder.RegisterType<FraudDetectedMessageSubscriber>().AutoActivate().SingleInstance();
            builder.RegisterType<Service.ClientRiskManager.Subscribers.SignalUnlimintTransferSubscriber>()
                    .AutoActivate()
                    .SingleInstance();
        }
    }
}