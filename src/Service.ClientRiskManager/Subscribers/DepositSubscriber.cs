using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using DotNetCoreDecorators;
using Microsoft.Extensions.Logging;
using Service.Bitgo.DepositDetector.Domain.Models;
using Service.ClientRiskManager.Domain;

namespace Service.ClientRiskManager.Subscribers
{
    public class DepositSubscriber : IStartable
    {
        private readonly ISubscriber<IReadOnlyList<Deposit>> _subscriber;
        private readonly ILogger<DepositSubscriber> _logger;
        private readonly IDepositRiskManager _manager;

        public DepositSubscriber(
            ISubscriber<IReadOnlyList<Deposit>> subscriber,
            IDepositRiskManager manager, 
            ILogger<DepositSubscriber> logger)
        {
            _subscriber = subscriber;
            _manager = manager;
            _logger = logger;
        }

        public void Start()
        {
            _subscriber.Subscribe(Handler);
        }

        private async ValueTask Handler(IReadOnlyList<Deposit> messages)
        {
            await _manager.ApplyNewDepositAsync(messages);
        }
    }
}