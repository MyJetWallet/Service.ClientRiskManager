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
    public class SignalCircleCardSubscriber
    {
        private readonly ILogger<SignalCircleCardSubscriber> _logger;
        private readonly IMyNoSqlServerDataWriter<ClientFraudNoSql> _noSqlServerDataWriter;
        private readonly IClientProfileService _clientProfileService;
        private readonly IServiceBusPublisher<FraudDetectedMessage> _publisher;
        private readonly ICircleCardsService _circleCardsService;
        private readonly SemaphoreSlim _locker = new SemaphoreSlim(1);

        public SignalCircleCardSubscriber(
            ILogger<SignalCircleCardSubscriber> logger,
            ISubscriber<SignalCircleCard> subscriber,
            IMyNoSqlServerDataWriter<ClientFraudNoSql> noSqlServerDataWriter,
            IClientProfileService clientProfileService,
            IServiceBusPublisher<FraudDetectedMessage> publisher,
            ICircleCardsService circleCardsService)
        {
            subscriber.Subscribe(HandleSignal);
            _logger = logger;
            this._noSqlServerDataWriter = noSqlServerDataWriter;
            _clientProfileService = clientProfileService;
            this._publisher = publisher;
            this._circleCardsService = circleCardsService;
        }

        private async ValueTask HandleSignal(SignalCircleCard signal)
        {
            using var activity = MyTelemetry.StartActivity($"Handle {nameof(SignalCircleCard)}");

            _logger.LogInformation("Processing SignalCircleCard: {context}", signal.ToJson());

            try
            {
                if (!signal.ErrorCode.HasValue &&
                    signal.ErrorCode != MyJetWallet.Circle.Models.Cards.CardVerificationError.VerificationFraudDetected)
                    return;

                await _locker.WaitAsync();

                var card = await _circleCardsService.GetCardByCircleId(new Circle.Wallets.Grpc.Models.GetCardByCircleIdRequest
                {
                    CircleCardId = signal.CircleCardId,
                });

                if (!card.IsSuccess || card.Data == null)
                {
                    throw new Exception($"Card not found! Id: {signal.CircleCardId}");
                }

                var fraud = await _noSqlServerDataWriter.GetAsync(ClientFraudNoSql.GeneratePartitionKey(card.Data.ClientId),
                    ClientFraudNoSql.GenerateRowKey("CircleCard"));

                if (fraud == null || fraud.ClietFraud == null)
                {
                    fraud = ClientFraudNoSql.Create(new ClientFraud
                    {
                        Attempts3dsFailedCount = 0,
                        CardFraudDetected = false,
                        ClientId = card.Data.ClientId,
                        Type = "CircleCard",
                    });
                }

                fraud.ClietFraud.CardFraudDetected = true;

                await _publisher.PublishAsync(new FraudDetectedMessage
                {
                    ClientFraud = fraud.ClietFraud,
                });

                await _noSqlServerDataWriter.InsertOrReplaceAsync(fraud);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing SignalCircleCard {@context}", signal.ToJson());
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

