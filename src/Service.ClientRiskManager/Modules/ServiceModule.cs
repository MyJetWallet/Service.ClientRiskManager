using System;
using Autofac;
using Autofac.Core;
using Autofac.Core.Registration;
using MyJetWallet.Sdk.ServiceBus;
using Service.Bitgo.DepositDetector.Client;
using Service.Bitgo.DepositDetector.Domain.Models;
using Service.ClientRiskManager.Domain;
using Service.ClientRiskManager.Subscribers;
using Service.Circle.Webhooks.Domain.Models;
using Service.ClientProfile.Client;
using Service.ClientRiskManager.Jobs;
using Service.Circle.Wallets.Client;

namespace Service.ClientRiskManager.Modules
{
    public class ServiceModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterCircleWalletsClientWithoutCache(Program.Settings.CircleWalletsGrpcServiceUrl);

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

            builder.RegisterBitgoDepositServiceClient(Program.Settings.BitgoDepositServiceGrpcUrl);
        }
    }
}