using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service.Tools;
using Service.Bitgo.DepositDetector.Grpc;
using Service.ClientRiskManager.Domain;

namespace Service.ClientRiskManager.Jobs
{
    public class RecalculateRiskBackgroundJob : IStartable
    {
        private const int TimerSpan5Min = 5*60;
        //private const int TimerSpan5Min = 30;
        private readonly ILogger<RecalculateRiskBackgroundJob> _logger;
        private readonly MyTaskTimer _operationsTimer;
        private readonly IDepositRiskManager _manager;

        public RecalculateRiskBackgroundJob(
            ILogger<RecalculateRiskBackgroundJob> logger, 
            IDepositRiskManager manager)
        {
            _logger = logger;
            _manager = manager;
            _operationsTimer = new MyTaskTimer(nameof(RecalculateRiskBackgroundJob),
                TimeSpan.FromSeconds(TimerSpan5Min), logger, Process);
        }

        public void Start()
        {
            _operationsTimer.Start();
        }
        
        public void Stop()
        {
            _operationsTimer.Stop();
        }

        private async Task Process()
        {
            try
            {
                _logger.LogInformation("Recalculating CircleCard all clients deposits");
                await _manager.RecalculateAllAsync();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to process CircleCard deposit due to {error}", ex.Message);
            }

        }
    }
}