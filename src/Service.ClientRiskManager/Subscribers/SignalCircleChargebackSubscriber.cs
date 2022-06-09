using DotNetCoreDecorators;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service;
using Service.Circle.Webhooks.Domain.Models;
using Service.ClientProfile.Grpc;
using System;
using System.Threading.Tasks;

namespace Service.ClientRiskManager.Subscribers
{

    public class SignalCircleChargebackSubscriber
    {
        private readonly ILogger<SignalCircleChargebackSubscriber> _logger;
        private readonly IClientProfileService _clientProfileService;

        public SignalCircleChargebackSubscriber(
            ILogger<SignalCircleChargebackSubscriber> logger,
            ISubscriber<SignalCircleChargeback> subscriber,
            IClientProfileService clientProfileService)
        {
            subscriber.Subscribe(HandleSignal);
            _logger = logger;
            _clientProfileService = clientProfileService;
        }

        private async ValueTask HandleSignal(SignalCircleChargeback signal)
        {
            using var activity = MyTelemetry.StartActivity($"Handle {nameof(SignalCircleChargeback)}");

            _logger.LogInformation("Processing SignalCircleChargeback: {context}", signal.ToJson());

            try
            {
                var reason = $"CircleChargeback: {signal.Chargeback.Id}";
                var expiryTime = DateTime.UtcNow + TimeSpan.FromDays(365);

                await _clientProfileService.AddBlockerToClient(new ClientProfile.Grpc.Models.Requests.AddBlockerToClientRequest
                {
                    BlockerReason = reason,
                    ClientId = signal.ClientId,
                    ExpiryTime = expiryTime,
                    Type = ClientProfile.Domain.Models.BlockingType.Withdrawal,
                });

                await _clientProfileService.AddBlockerToClient(new ClientProfile.Grpc.Models.Requests.AddBlockerToClientRequest
                {
                    BlockerReason = reason,
                    ClientId = signal.ClientId,
                    ExpiryTime = expiryTime,
                    Type = ClientProfile.Domain.Models.BlockingType.Transfer,
                });

                await _clientProfileService.AddBlockerToClient(new ClientProfile.Grpc.Models.Requests.AddBlockerToClientRequest
                {
                    BlockerReason = reason,
                    ClientId = signal.ClientId,
                    ExpiryTime = expiryTime,
                    Type = ClientProfile.Domain.Models.BlockingType.Trade,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing SignalCircleChargeback {@context}", signal.ToJson());
                ex.FailActivity();
                throw;
            }
        }
    }
}

