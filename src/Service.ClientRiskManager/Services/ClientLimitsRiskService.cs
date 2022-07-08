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
            using var activity = MyTelemetry.StartActivity($"Handle {nameof(GetClientWithdrawalLimitsAsync)}");

            _logger.LogInformation("Processing GetClientWithdrawalLimitsAsync: {context}", request.ToJson());

            try
            {
                var circleCardDeposit = await _depositRiskManager.GetAndRecalculateClientLastMonthRawAsync(
                    request.ClientId, request.BrokerId);

                var response = new GetClientWithdrawalLimitsResponse
                {
                    Success = true,
                    ErrorMessage = String.Empty,
                    CardDepositsSummary = new CircleClientDepositSummary
                    {
                        DepositLast30DaysInUsd = circleCardDeposit.CardDepositsSummary.DepositLast30DaysInUsd,
                        DepositLast14DaysInUsd = circleCardDeposit.CardDepositsSummary.DepositLast14DaysInUsd,
                        DepositLast7DaysInUsd = circleCardDeposit.CardDepositsSummary.DepositLast7DaysInUsd,
                        DepositLast1DaysInUsd = circleCardDeposit.CardDepositsSummary.DepositLast1DaysInUsd,
                        Deposit30DaysLimit = circleCardDeposit.CardDepositsSummary.Deposit30DaysLimit,
                        Deposit7DaysLimit = circleCardDeposit.CardDepositsSummary.Deposit7DaysLimit,
                        Deposit1DaysLimit = circleCardDeposit.CardDepositsSummary.Deposit1DaysLimit,
                        Deposit30DaysState = circleCardDeposit.CardDepositsSummary.Deposit30DaysState,
                        Deposit7DaysState = circleCardDeposit.CardDepositsSummary.Deposit7DaysState,
                        Deposit1DaysState = circleCardDeposit.CardDepositsSummary.Deposit1DaysState,
                        BarInterval = circleCardDeposit.CardDepositsSummary.BarInterval,
                        BarProgres = circleCardDeposit.CardDepositsSummary.BarProgres,
                        LeftHours = circleCardDeposit.CardDepositsSummary.LeftHours,
                        LastDeposit30DaysLeftHours = circleCardDeposit.CardDepositsSummary.LastDeposit30DaysLeftHours,
                        LastDeposit7DaysLeftHours = circleCardDeposit.CardDepositsSummary.LastDeposit7DaysLeftHours,
                        LastDeposit1DaysLeftHours = circleCardDeposit.CardDepositsSummary.LastDeposit1DaysLeftHours,
                    }
                };
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing GetClientWithdrawalLimitsAsync {@context}", request.ToJson());
                ex.FailActivity();
                throw;
            }
        }

        public async Task SetClientDepositLimitsAsync(SetClientDepositLimitsRequest request)
        {
            using var activity = MyTelemetry.StartActivity($"Handle {nameof(ClientLimitsRiskService)}");

            _logger.LogInformation("Processing SetClientDepositLimitsAsync: {context}", request.ToJson());
            try
            {
                var newLimits = request.PaymentDetails;
                await _depositRiskManager.RecalculateAllAsync();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing SetClientDepositLimitsAsync {@context}", request.ToJson());
                ex.FailActivity();
                throw;
            }
        }
    }
}
