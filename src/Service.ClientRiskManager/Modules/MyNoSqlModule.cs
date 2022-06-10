using Autofac;
using MyJetWallet.Sdk.NoSql;
using Service.ClientRiskManager.Domain.Models;
using Service.IndexPrices.Client;


namespace Service.ClientRiskManager.Modules
{
    public class MyNoSqlModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterMyNoSqlWriter<ClientRiskNoSqlEntity>(
                Program.ReloadedSettings(e => e.MyNoSqlWriterUrl),
                ClientRiskNoSqlEntity.TableName);
            
            var myNoSqlClient = builder.CreateNoSqlClient(Program.Settings.MyNoSqlReaderHostPort, Program.LogFactory);
            builder.RegisterIndexPricesClient(myNoSqlClient);
        }
    }
}