using DotNetCoreDecorators;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service;
using MyJetWallet.Sdk.ServiceBus;
using MyNoSqlServer.Abstractions;
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
    public class SignalCircleTransferSubscriber
    {
        private readonly ILogger<SignalCircleTransferSubscriber> _logger;
        private readonly IMyNoSqlServerDataWriter<ClientFraudNoSql> _noSqlServerDataWriter;
        private readonly IClientProfileService _clientProfileService;
        private readonly IServiceBusPublisher<FraudDetectedMessage> _publisher;
        private readonly SemaphoreSlim _locker = new SemaphoreSlim(1);

        public SignalCircleTransferSubscriber(
            ILogger<SignalCircleTransferSubscriber> logger,
            ISubscriber<SignalCircleTransfer> subscriber,
            IMyNoSqlServerDataWriter<ClientFraudNoSql> noSqlServerDataWriter,
            IClientProfileService clientProfileService,
            IServiceBusPublisher<FraudDetectedMessage> publisher)
        {
            subscriber.Subscribe(HandleSignal);
            _logger = logger;
            this._noSqlServerDataWriter = noSqlServerDataWriter;
            _clientProfileService = clientProfileService;
            this._publisher = publisher;
        }

        private async ValueTask HandleSignal(SignalCircleTransfer signal)
        {
            using var activity = MyTelemetry.StartActivity($"Handle {nameof(SignalCircleTransfer)}");

            _logger.LogInformation("Processing SignalCircleTransfer: {context}", signal.ToJson());

            try
            {
                if (!signal.PaymentInfo.ErrorCode.HasValue &&
                    signal.PaymentInfo.ErrorCode != MyJetWallet.Circle.Models.Payments.PaymentErrorCode.ThreeDSecureFailure &&
                    signal.PaymentInfo.ErrorCode != MyJetWallet.Circle.Models.Payments.PaymentErrorCode.ThreeDSecureActionExpired)
                    return;

                await _locker.WaitAsync();

                var fraud = await _noSqlServerDataWriter.GetAsync(ClientFraudNoSql.GeneratePartitionKey(signal.ClientId),
                    ClientFraudNoSql.GenerateRowKey("CircleCard"));

                if (fraud == null || fraud.ClietFraud == null)
                {
                    fraud = ClientFraudNoSql.Create(new ClientFraud
                    {
                        Attempts3dsFailedCount = 0,
                        CardFraudDetected = false,
                        ClientId = signal.ClientId,
                        Type = "CircleCard",
                    }) ;
                }

                fraud.ClietFraud.Attempts3dsFailedCount++;

                if (fraud.ClietFraud.Attempts3dsFailedCount >= 3)
                {
                    await _publisher.PublishAsync(new FraudDetectedMessage
                    {
                        ClientFraud = fraud.ClietFraud,
                    });
                }

                await _noSqlServerDataWriter.InsertOrReplaceAsync(fraud);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing SignalCircleTransfer {@context}", signal.ToJson());
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

