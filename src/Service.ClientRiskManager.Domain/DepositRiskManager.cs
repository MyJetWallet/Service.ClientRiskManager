using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service;
using MyNoSqlServer.Abstractions;
using Service.Bitgo.DepositDetector.Domain.Models;
using Service.ClientRiskManager.Domain.Models;

namespace Service.ClientRiskManager.Domain;

public class DepositRiskManager : IDepositRiskManager
{
    private const string CircleCard = "CircleCard";

    private readonly ILogger<DepositRiskManager> _logger;
    private readonly IMyNoSqlServerDataWriter<ClientRiskNoSqlEntity> _writer;
    
    public DepositRiskManager(ILogger<DepositRiskManager> logger,
        IMyNoSqlServerDataWriter<ClientRiskNoSqlEntity> writer)
    {
        _logger = logger;
        _writer = writer;
    }

    public async Task ApplyNewDepositAsync(IReadOnlyList<Deposit> messages)
    {
        foreach (var message in messages)
        {
            if (message.Status == DepositStatus.Processed && message.Integration == CircleCard)
            {
                await UpsertDeposit(message);
                continue;
            }
        }
    }

    private async Task UpsertDeposit(Deposit deposit)
    {
        try
        {
            _logger.LogInformation("Processing CircleCard deposit due to {error}", deposit.ToJson());

            var cachedEntity = await _writer.GetAsync(
                ClientRiskNoSqlEntity.GeneratePartitionKey(deposit.BrokerId),
                ClientRiskNoSqlEntity.GenerateRowKey(deposit.ClientId));
            
            var newDepositInUsd = deposit.Amount * deposit.AssetIndexPrice;

            if (cachedEntity != null)
            {
                cachedEntity.CardDeposits.Add(new CircleClientDeposit
                {
                    Date = deposit.EventDate,
                    Balance = deposit.Amount,
                    AssetSymbol = deposit.AssetSymbol,
                    BalanceInUsd = newDepositInUsd
                });
                
                //var currDate = DateTime.UtcNow;
                cachedEntity.CleanupDepositsLess30Days(deposit.EventDate);
                cachedEntity.RecalcDeposits(deposit.EventDate);
                
                await _writer.InsertOrReplaceAsync(cachedEntity);
            }
            else
            {
                var entity = ClientRiskNoSqlEntity.Create(deposit.BrokerId, deposit.ClientId,
                    new CircleClientDeposit
                    {
                        Date = deposit.EventDate,
                        Balance = deposit.Amount,
                        AssetSymbol = deposit.AssetSymbol,
                        BalanceInUsd = newDepositInUsd
                    });
                await _writer.InsertOrReplaceAsync(entity);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to process CircleCard deposit due to {error}", ex.Message);
        }
    }
}