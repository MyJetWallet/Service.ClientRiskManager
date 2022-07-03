using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service;
using MyNoSqlServer.Abstractions;
using Service.Circle.Webhooks.Domain.Models;
using Service.ClientRiskManager.Domain;
using Service.ClientRiskManager.Domain.Models;
using Service.ClientRiskManager.Grpc;
using Service.ClientRiskManager.Grpc.Models;
using Service.ClientRiskManager.Settings;

namespace Service.ClientRiskManager.Services
{
    public class ClientLimitsRiskService: IClientLimitsRiskService
    {
        private readonly ILogger<ClientLimitsRiskService> _logger;
        private readonly IDepositRiskManager _depositRiskManager;


        public ClientLimitsRiskService(ILogger<ClientLimitsRiskService> logger, 
            IDepositRiskManager depositRiskManager)
        {
            _logger = logger;
            _depositRiskManager = depositRiskManager;
        }

        public async Task<GetClientWithdrawalLimitsResponse> GetClientWithdrawalLimitsAsync(GetClientWithdrawalLimitsRequest request)
        {
            using var activity = MyTelemetry.StartActivity($"Handle {nameof(SignalCircleChargeback)}");

            _logger.LogInformation("Processing GetClientWithdrawalLimitsAsync: {context}", request.ToJson());

            try
            {
                var circleCardDeposit = await _depositRiskManager.GetAndRecalculateClientLastMonthRawAsync(
                    request.ClientId, request.BrokerId);

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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing GetClientWithdrawalLimitsAsync {@context}", request.ToJson());
                ex.FailActivity();
                throw;
            }
        }
    }
}
