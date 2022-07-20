using DotNetCoreDecorators;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service;
using Service.Circle.Webhooks.Domain.Models;
using Service.ClientProfile.Grpc;
using Service.Unlimint.Webhooks.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Service.ClientRiskManager.Subscribers
{

    public class SignalUnlimintTransferSubscriber
    {
        private readonly ILogger<SignalUnlimintTransferSubscriber> _logger;
        private readonly IClientProfileService _clientProfileService;
        private readonly SemaphoreSlim _locker = new SemaphoreSlim(1);

        public SignalUnlimintTransferSubscriber(
            ILogger<SignalUnlimintTransferSubscriber> logger,
            ISubscriber<SignalUnlimintTransfer> subscriber,
            IClientProfileService clientProfileService)
        {
            subscriber.Subscribe(HandleSignal);
            _logger = logger;
            _clientProfileService = clientProfileService;
        }

        private async ValueTask HandleSignal(SignalUnlimintTransfer signal)
        {
            using var activity = MyTelemetry.StartActivity($"Handle {nameof(SignalUnlimintTransfer)}");

            _logger.LogInformation("Processing SignalUnlimintTransfer: {context}", signal.ToJson());
            if (signal.PaymentInfo == null || signal.PaymentInfo.Status != MyJetWallet.Unlimint.Models.Payments.PaymentStatus.ChargedBack)
                return;
            try
            {
                await _locker.WaitAsync();
                var reason = $"UnlimintChargeback: {signal.PaymentInfo.Id}";
                var expiryTime = DateTime.UtcNow + TimeSpan.FromDays(365);

                var blockers = _clientProfileService.GetClientBlockers(new ClientProfile.Grpc.Models.Requests.Blockers.GetClientProfileBlockersRequest
                {
                    ClientId = signal.ClientId
                });

                var hashSetIds = new HashSet<int>();
                var hashSet = new HashSet<ClientProfile.Domain.Models.BlockingType>();

                await foreach (var item in blockers)
                {
                    if (hashSetIds.Contains(item.BlockerId))
                        break;
                    hashSet.Add(item.BlockedOperationType);
                    hashSetIds.Add(item.BlockerId);
                }

                if (!hashSet.Contains(ClientProfile.Domain.Models.BlockingType.Withdrawal))
                    await _clientProfileService.AddBlockerToClient(new ClientProfile.Grpc.Models.Requests.AddBlockerToClientRequest
                    {
                        BlockerReason = reason,
                        ClientId = signal.ClientId,
                        ExpiryTime = expiryTime,
                        Type = ClientProfile.Domain.Models.BlockingType.Withdrawal,
                    });

                if (!hashSet.Contains(ClientProfile.Domain.Models.BlockingType.Transfer))
                    await _clientProfileService.AddBlockerToClient(new ClientProfile.Grpc.Models.Requests.AddBlockerToClientRequest
                    {
                        BlockerReason = reason,
                        ClientId = signal.ClientId,
                        ExpiryTime = expiryTime,
                        Type = ClientProfile.Domain.Models.BlockingType.Transfer,
                    });

                if (!hashSet.Contains(ClientProfile.Domain.Models.BlockingType.Trade))
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
                _logger.LogError(ex, "Error processing SignalUnlimintTransfer {@context}", signal.ToJson());
                ex.FailActivity();
                throw;
            }
            finally
            {
                _locker.Release();
            }
        }
    }
}

