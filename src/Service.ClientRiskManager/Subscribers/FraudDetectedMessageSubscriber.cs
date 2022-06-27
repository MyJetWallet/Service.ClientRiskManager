using DotNetCoreDecorators;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service;
using MyJetWallet.Sdk.ServiceBus;
using MyNoSqlServer.Abstractions;
using Service.Circle.Wallets.Grpc;
using Service.Circle.Webhooks.Domain.Models;
using Service.ClientProfile.Grpc;
using Service.ClientRiskManager.Domain.Models.FraudDetection;
using Service.ClientRiskManager.MyNoSql.FraudDetection;
using Service.ClientRiskManager.ServiceBus.FraudDetection;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Service.ClientRiskManager.Subscribers
{
    public class FraudDetectedMessageSubscriber
    {
        private readonly ILogger<FraudDetectedMessageSubscriber> _logger;
        private readonly IMyNoSqlServerDataWriter<ClientFraudNoSql> _noSqlServerDataWriter;
        private readonly IClientProfileService _clientProfileService;
        private readonly SemaphoreSlim _locker = new SemaphoreSlim(1);

        public FraudDetectedMessageSubscriber(
            ILogger<FraudDetectedMessageSubscriber> logger,
            ISubscriber<FraudDetectedMessage> subscriber,
            IMyNoSqlServerDataWriter<ClientFraudNoSql> noSqlServerDataWriter,
            IClientProfileService clientProfileService)
        {
            subscriber.Subscribe(HandleSignal);
            _logger = logger;
            this._noSqlServerDataWriter = noSqlServerDataWriter;
            _clientProfileService = clientProfileService;
        }

        private async ValueTask HandleSignal(FraudDetectedMessage signal)
        {
            using var activity = MyTelemetry.StartActivity($"Handle {nameof(FraudDetectedMessage)}");

            _logger.LogInformation("Processing FraudDetectedMessage: {context}", signal.ToJson());

            try
            {
                await _locker.WaitAsync();
                //TODO: Create Business logic for fraud block.
                var reason = $"FraudDetected: {signal.ClientFraud.Type}";
                var expiryTime = DateTime.UtcNow + TimeSpan.FromDays(365);

                var blockers = _clientProfileService.GetClientBlockers(new ClientProfile.Grpc.Models.Requests.Blockers.GetClientProfileBlockersRequest
                {
                    ClientId = signal.ClientFraud.ClientId
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
                        ClientId = signal.ClientFraud.ClientId,
                        ExpiryTime = expiryTime,
                        Type = ClientProfile.Domain.Models.BlockingType.Withdrawal,
                    });

                if (!hashSet.Contains(ClientProfile.Domain.Models.BlockingType.Transfer))
                    await _clientProfileService.AddBlockerToClient(new ClientProfile.Grpc.Models.Requests.AddBlockerToClientRequest
                    {
                        BlockerReason = reason,
                        ClientId = signal.ClientFraud.ClientId,
                        ExpiryTime = expiryTime,
                        Type = ClientProfile.Domain.Models.BlockingType.Transfer,
                    });

                if (!hashSet.Contains(ClientProfile.Domain.Models.BlockingType.Trade))
                    await _clientProfileService.AddBlockerToClient(new ClientProfile.Grpc.Models.Requests.AddBlockerToClientRequest
                    {
                        BlockerReason = reason,
                        ClientId = signal.ClientFraud.ClientId,
                        ExpiryTime = expiryTime,
                        Type = ClientProfile.Domain.Models.BlockingType.Trade,
                    });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing FraudDetectedMessage {@context}", signal.ToJson());
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

