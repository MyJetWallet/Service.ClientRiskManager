using Autofac;
using MyJetWallet.Sdk.NoSql;
using Service.ClientRiskManager.Domain.Models;

namespace Service.ClientRiskManager.Modules
{
    public class MyNoSqlModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterMyNoSqlWriter<ClientRiskNoSqlEntity>(
                Program.ReloadedSettings(e => e.MyNoSqlWriterUrl),
                ClientRiskNoSqlEntity.TableName);
        }
    }
}