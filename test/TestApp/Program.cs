using System;
using System.Threading.Tasks;
using ProtoBuf.Grpc.Client;
using Service.ClientRiskManager.Client;
using Service.ClientRiskManager.Grpc.Models;

namespace TestApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            GrpcClientFactory.AllowUnencryptedHttp2 = true;

            Console.Write("Press enter to start");
            Console.ReadLine();


            var factory = new ClientRiskManagerClientFactory("http://localhost:5001");
            var client = factory.GetClientLimitsRiskService();

            var resp = await  client.GetClientWithdrawalLimitsAsync(new GetClientWithdrawalLimitsRequest(){ClientId = "Alex"});
            Console.WriteLine(resp?.ErrorMessage);

            Console.WriteLine("End");
            Console.ReadLine();
        }
    }
}
