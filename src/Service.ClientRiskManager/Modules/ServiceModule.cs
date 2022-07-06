using Autofac;
using Service.Bitgo.DepositDetector.Client;
using Service.ClientRiskManager.Domain;
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