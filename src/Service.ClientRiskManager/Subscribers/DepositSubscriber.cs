using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using DotNetCoreDecorators;
using Microsoft.Extensions.Logging;
using Service.Bitgo.DepositDetector.Domain.Models;
using Service.ClientRiskManager.Domain;

namespace Service.ClientRiskManager.Subscribers
{
    public class DepositSubscriber
    {
        private readonly IDepositRiskManager _manager;

        public DepositSubscriber(
            ISubscriber<Deposit> subscriber,
            IDepositRiskManager manager)
        {
            _manager = manager;
            subscriber.Subscribe(Handler);
        }

        private async ValueTask Handler(Deposit message)
        {
            await _manager.ApplyNewDepositAsync(message);
        }
    }
}