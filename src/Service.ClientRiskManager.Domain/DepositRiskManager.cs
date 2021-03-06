using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service;
using MyNoSqlServer.Abstractions;
using Service.Bitgo.DepositDetector.Domain.Models;
using Service.Bitgo.DepositDetector.Grpc;
using Service.Bitgo.DepositDetector.Grpc.Models;
using Service.Circle.Wallets.Domain.Models;
using Service.Circle.Wallets.Grpc;
using Service.ClientRiskManager.Domain.Models;

namespace Service.ClientRiskManager.Domain;

public class DepositRiskManager : IDepositRiskManager
{
    private const string CircleCard = "CircleCard";

    private readonly ILogger<DepositRiskManager> _logger;
    private readonly IMyNoSqlServerDataWriter<ClientRiskNoSqlEntity> _writer;
    private readonly IBitgoDepositService _bitgoDepositService;
    private readonly ICircleCardsService _circleCardsService;


    public DepositRiskManager(ILogger<DepositRiskManager> logger,
        IMyNoSqlServerDataWriter<ClientRiskNoSqlEntity> writer,
        IBitgoDepositService bitgoDepositService,
        ICircleCardsService circleCardsService)
    {
        _logger = logger;
        _writer = writer;
        _bitgoDepositService = bitgoDepositService;
        _circleCardsService = circleCardsService;
    }

    public async Task ApplyNewDepositAsync(Deposit message)
    {
        if (message.Status == DepositStatus.Processed && message.Integration == CircleCard)
        {
            var paymentDetails = (await _circleCardsService.GetCardPaymentDetails()).Data;
            await UpsertDeposit(message, paymentDetails);
        }
    }

    private async Task UpsertDeposit(Deposit deposit, CircleCardPaymentDetails paymentDetails)
    {
        try
        {
            _logger.LogInformation("Processing CircleCard deposit due to {error}", deposit.ToJson());

            var cachedEntity = await _writer.GetAsync(
                ClientRiskNoSqlEntity.GeneratePartitionKey(deposit.BrokerId),
                ClientRiskNoSqlEntity.GenerateRowKey(deposit.ClientId));

            decimal balance = deposit.Amount + deposit.FeeAmount;
            var newDepositInUsd = balance * deposit.AssetIndexPrice;

            if (cachedEntity != null)
            {
                cachedEntity.CardDeposits.Add(new CircleClientDeposit
                {
                    Date = deposit.EventDate,
                    Balance = balance,
                    AssetSymbol = deposit.AssetSymbol,
                    BalanceInUsd = newDepositInUsd
                });

                // Cleanup old deposits
                var currDate = deposit.EventDate; //DateTime.UtcNow;
                cachedEntity.CardDeposits = cachedEntity.CardDeposits
                    .Where(e => e.Date < currDate.AddMonths(-1))
                    .ToList();

                cachedEntity.CardDepositsSummary = DepositDayStatCalculator.PrepareDepositStat(
                    paymentDetails, cachedEntity.CardDeposits, deposit.EventDate);

                await _writer.InsertOrReplaceAsync(cachedEntity);
            }
            else
            {
                var entity = ClientRiskNoSqlEntity.Create(deposit.BrokerId, deposit.ClientId,
                    new CircleClientDeposit
                    {
                        Date = deposit.EventDate,
                        Balance = balance,
                        AssetSymbol = deposit.AssetSymbol,
                        BalanceInUsd = newDepositInUsd
                    },
                    paymentDetails);

                await _writer.InsertOrReplaceAsync(entity);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to process CircleCard deposit due to {error}", ex.Message);
        }
    }

    public async Task RecalculateAllAsync()
    {
        try
        {
            _logger.LogInformation("Recalculating CircleCard all clients deposits");

            var currDate = DateTime.UtcNow;
            var lastId = 0L;
            var batchSize = 200;
            var deposits = new List<Deposit>();
            while (true)
            {
                var depositsResponse = await _bitgoDepositService
                    .GetDepositsByPeriod(new GetDepositsByPeriodRequest
                    {
                        LastId = lastId,
                        BatchSize = batchSize,
                        ClientId = null,
                        WalletId = null,
                        Integration = CircleCard,
                        OnlySuccessfully = true,
                        FromDate = currDate.AddMonths(-1),
                        ToDate = currDate
                    });

                lastId = depositsResponse.IdForNextQuery;
                if (!depositsResponse.Success || depositsResponse.DepositCollection == null ||
                    depositsResponse.DepositCollection.Count == 0)
                {
                    break;
                }

                var depositsFromDb = depositsResponse?.DepositCollection ?? new List<Deposit>();
                deposits.AddRange(depositsFromDb);
            }
            var paymentDetails = (await _circleCardsService.GetCardPaymentDetails()).Data;

            var depositsFromDbByClient = deposits
                .GroupBy(e => new { e.BrokerId, e.ClientId },
                    (k, c) => new ClientRiskNoSqlEntity
                    {
                        PartitionKey = ClientRiskNoSqlEntity.GeneratePartitionKey(k.BrokerId),
                        RowKey = ClientRiskNoSqlEntity.GenerateRowKey(k.ClientId),
                        // TimeStamp = null,
                        // Expires = null,
                        CardDeposits = c.Select(cs => new CircleClientDeposit
                        {
                            Date = cs.EventDate,
                            Balance = cs.Amount + cs.FeeAmount,
                            BalanceInUsd = (cs.Amount + cs.FeeAmount) * cs.AssetIndexPrice,
                            AssetSymbol = cs.AssetSymbol
                        }).ToList(),
                        CardDepositsSummary = new CircleClientDepositSummary()
                    })
                .ToList();

            foreach (var deposit in depositsFromDbByClient)
            {
                deposit.CardDepositsSummary = DepositDayStatCalculator.PrepareDepositStat(
                    paymentDetails, deposit.CardDeposits, currDate);
            }

            await _writer.CleanAndBulkInsertAsync(depositsFromDbByClient);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to recalculate CircleCard client deposit due to {error}", ex.Message);
        }
    }

    public async Task<ClientRiskNoSqlEntity> GetAndRecalculateClientLastMonthRawAsync(string clientId,
        string brokerId)
    {
        try
        {
            _logger.LogInformation("Recalculating CircleCard client deposit {clientId}", clientId);

            var currDate = DateTime.UtcNow;
            var lastId = 0L;
            var batchSize = 200;
            var deposits = new List<Deposit>();
            while (true)
            {
                var depositsResponse = await _bitgoDepositService
                    .GetDepositsByPeriod(new GetDepositsByPeriodRequest
                    {
                        LastId = lastId,
                        BatchSize = batchSize,
                        ClientId = clientId,
                        WalletId = null,
                        Integration = CircleCard,
                        OnlySuccessfully = true,
                        FromDate = currDate.AddMonths(-1),
                        ToDate = currDate,
                    });

                lastId = depositsResponse.IdForNextQuery;
                if (!depositsResponse.Success || depositsResponse.DepositCollection == null ||
                    depositsResponse.DepositCollection.Count == 0)
                {
                    break;
                }

                var depositsFromDb = depositsResponse?.DepositCollection ?? new List<Deposit>();
                deposits.AddRange(depositsFromDb);
            }

            var depositsFromDbByClient = deposits
                .Where(e => e.BrokerId == brokerId)
                .GroupBy(e => e.ClientId,
                    (k, c) => new ClientRiskNoSqlEntity
                    {
                        PartitionKey = ClientRiskNoSqlEntity.GeneratePartitionKey(brokerId),
                        RowKey = ClientRiskNoSqlEntity.GenerateRowKey(k),
                        // TimeStamp = null,
                        // Expires = null,
                        CardDeposits = c.Select(cs => new CircleClientDeposit
                        {
                            Date = cs.EventDate,
                            Balance = cs.Amount + cs.FeeAmount,
                            BalanceInUsd = (cs.Amount + cs.FeeAmount) * cs.AssetIndexPrice,
                            AssetSymbol = cs.AssetSymbol
                        }).ToList(),
                        CardDepositsSummary = new CircleClientDepositSummary()
                    })
                .FirstOrDefault();

            var depositsToCalc = depositsFromDbByClient ?? new ClientRiskNoSqlEntity
            {
                PartitionKey = ClientRiskNoSqlEntity.GeneratePartitionKey(brokerId),
                RowKey = ClientRiskNoSqlEntity.GenerateRowKey(clientId),
                //TimeStamp = null,
                //Expires = null,
                CardDeposits = new List<CircleClientDeposit>(),
                CardDepositsSummary = new CircleClientDepositSummary()
            };
            var paymentDetails = (await _circleCardsService.GetCardPaymentDetails()).Data;

            depositsToCalc.CardDepositsSummary = DepositDayStatCalculator.PrepareDepositStat(
                paymentDetails, depositsToCalc.CardDeposits, currDate);

            await _writer.InsertOrReplaceAsync(depositsToCalc);
            return depositsToCalc;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to recalculate CircleCard client {clientId} deposit due to {error}",
                clientId, ex.Message);
            throw;
        }
    }
}