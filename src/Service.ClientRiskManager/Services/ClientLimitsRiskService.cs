using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service;
using MyNoSqlServer.Abstractions;
using Service.Circle.Webhooks.Domain.Models;
using Service.ClientRiskManager.Domain.Models;
using Service.ClientRiskManager.Grpc;
using Service.ClientRiskManager.Grpc.Models;
using Service.ClientRiskManager.Settings;

namespace Service.ClientRiskManager.Services
{
    public class ClientLimitsRiskService: IClientLimitsRiskService
    {
        private readonly ILogger<ClientLimitsRiskService> _logger;
        private readonly IMyNoSqlServerDataWriter<ClientRiskNoSqlEntity> _writer;


        public ClientLimitsRiskService(ILogger<ClientLimitsRiskService> logger, 
            IMyNoSqlServerDataWriter<ClientRiskNoSqlEntity> writer)
        {
            _logger = logger;
            _writer = writer;
        }

        public async Task<GetClientWithdrawalLimitsResponse> GetClientWithdrawalLimitsAsync(GetClientWithdrawalLimitsRequest request)
        {
            using var activity = MyTelemetry.StartActivity($"Handle {nameof(SignalCircleChargeback)}");

            _logger.LogInformation("Processing GetClientWithdrawalLimitsAsync: {context}", request.ToJson());

            try
            {
                var circleCardDeposit = await _writer.GetAsync(request.BrokerId, request.ClientId);
                if (circleCardDeposit != null)
                {
                    return new GetClientWithdrawalLimitsResponse
                    {
                        Success = true,
                        ErrorMessage = String.Empty,
                        CardDepositsSummary = new CircleClientDepositSummary
                        {
                            DepositLast30DaysInUsd = circleCardDeposit.CardDepositsSummary.DepositLast30DaysInUsd,
                            DepositLast14DaysInUsd = circleCardDeposit.CardDepositsSummary.DepositLast14DaysInUsd,
                            DepositLast7DaysInUsd = circleCardDeposit.CardDepositsSummary.DepositLast7DaysInUsd,
                            DepositLast1DaysInUsd = circleCardDeposit.CardDepositsSummary.DepositLast1DaysInUsd
                        }
                    };
                }
                
                return new GetClientWithdrawalLimitsResponse
                {
                    Success = true,
                    ErrorMessage = String.Empty,
                    CardDepositsSummary = new CircleClientDepositSummary
                    {
                        DepositLast30DaysInUsd = 0m,
                        DepositLast14DaysInUsd = 0m,
                        DepositLast7DaysInUsd = 0m,
                        DepositLast1DaysInUsd = 0m
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing GetClientWithdrawalLimitsAsync {@context}", request.ToJson());
                ex.FailActivity();
                throw;
            }
        }
        
        // private async Task CalculateCardReserveForWalletBalances(List<WalletBalanceWithAvgPrice> balances)
        // {
        //     try
        //     {
        //         var clientWithdrawalLimits = await _clientLimitsRiskService.GetClientWithdrawalLimitsAsync(
        //             new GetClientWithdrawalLimitsRequest
        //             {
        //                 BrokerId = ClientId.BrokerId,
        //                 ClientId = ClientId.ClientId
        //             });
        //         
        //         if (clientWithdrawalLimits?.CardDepositsSummary == null)
        //             return;
        //         
        //         var clientLimitsSummary = clientWithdrawalLimits.CardDepositsSummary;
        //         
        //         var clientEarn = _highYieldEngineClientOfferService
        //             .GetClientOfferDtoListAsync(new GetClientOfferDtoListGrpcRequest
        //         {
        //             ClientId = ClientId.ClientId,
        //             BaseAsset = "USD",
        //             Lang = Language, //"En",
        //             Brand = ClientId.BrandId,
        //             BrokerId = ClientId.BrokerId
        //         });
        //
        //         var earns = clientEarn.Result?.ClientOffers ?? new List<EarnOfferDto>();
        //         var earnBalances = earns
        //             .GroupBy(e => e.Asset)
        //             .ToDictionary(e => e.Key, e => e.Sum(i => i.Amount));
        //
        //         var usdAccountEquity = 0m;
        //
        //         foreach (var assetId in earnBalances.Keys.Where(e => balances.All(b => b.AssetId != e)))
        //         {
        //             balances.Add(new WalletBalanceWithAvgPrice(assetId, 0, DateTime.UtcNow, 0));
        //         }
        //         
        //         foreach (var balance in balances)
        //         {
        //             var amount = balance.Balance;
        //             if (earnBalances.TryGetValue(balance.AssetId, out var earnAmount))
        //             {
        //                 amount += earnAmount;
        //             }
        //             
        //             var (_, usdAmount) = _indexPricesClient.GetIndexPriceByAssetVolumeAsync(balance.AssetId, amount);
        //             usdAccountEquity += usdAmount;
        //         }
        //         
        //         var usdReserved = clientLimitsSummary.DepositLast14DaysInUsd; //todo sub aveilable reserve to withdrawal
        //
        //         var availableAmountUsd = usdAccountEquity - usdReserved;
        //         
        //         foreach (var balance in balances)
        //         {
        //             var balanceAmount = balance.Balance;
        //             var balanceAsset = balance.AssetId;
        //             
        //             if (earnBalances.TryGetValue(balanceAsset, out var earnAmount))
        //             {
        //                 balanceAmount += earnAmount;
        //             }
        //             else
        //             {
        //                 earnAmount = 0;
        //             }
        //             
        //             var asset = _assetService.GetAssetBySymbol(new AssetIdentity()
        //             {
        //                 Symbol = balanceAsset,
        //                 BrokerId = ClientId.BrokerId
        //             });
        //             
        //             if (asset == null)
        //             {
        //                 _logger.LogError(
        //                     "Cannot find {asset} for wallet '{walletId}' [{brokerId}|{brandId}|{clientId}] via connection {connectionId}",
        //                     balanceAsset, WalletId.WalletId, WalletId.BrokerId, WalletId.BrandId,
        //                     WalletId.ClientId, ConnectionId);
        //                 continue;
        //             }
        //             
        //             if (balanceAmount > 0)
        //             {
        //                 var (_, balanceUsd) = _indexPricesClient.GetIndexPriceByAssetVolumeAsync(balanceAsset, balanceAmount);
        //
        //                 var reserve = 0m;
        //                 if (availableAmountUsd < balanceUsd)
        //                 {
        //                     var coef = (balanceUsd - availableAmountUsd) / balanceUsd;
        //                     reserve = Math.Round(balanceAmount * coef, asset.Accuracy, MidpointRounding.ToPositiveInfinity);
        //                 }
        //
        //                 reserve -= earnAmount;
        //                 if (reserve < 0)
        //                     reserve = 0;
        //                 
        //                 if (reserve > balanceAmount)
        //                     reserve = balanceAmount;
        //                 
        //                 balance.CardReserve = reserve;
        //             }
        //             else
        //             {
        //                 balance.CardReserve = 0m;
        //             }
        //         }
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(
        //             "Error {message}",
        //             ex.Message);
        //     }
        // }
    }
}
