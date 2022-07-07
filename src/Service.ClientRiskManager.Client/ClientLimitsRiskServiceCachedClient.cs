using System;
using System.Threading.Tasks;
using MyNoSqlServer.Abstractions;
using Service.ClientRiskManager.Domain.Models;
using Service.ClientRiskManager.Grpc;
using Service.ClientRiskManager.Grpc.Models;

namespace Service.ClientRiskManager.Client;

public class ClientLimitsRiskServiceCachedClient : IClientLimitsRiskService
{
    private readonly IMyNoSqlServerDataReader<ClientRiskNoSqlEntity> _clientLimitsRiskReader;
    private readonly IClientLimitsRiskService _limitsRiskService;

    public ClientLimitsRiskServiceCachedClient(IClientLimitsRiskService limitsRiskService,
        IMyNoSqlServerDataReader<ClientRiskNoSqlEntity> clientLimitsRiskReader)
    {
        _clientLimitsRiskReader = clientLimitsRiskReader;
        _limitsRiskService = limitsRiskService;
    }

    public async Task<GetClientWithdrawalLimitsResponse> GetClientWithdrawalLimitsAsync(GetClientWithdrawalLimitsRequest request)
    {
        var noSqlEntity = _clientLimitsRiskReader
            .Get(ClientRiskNoSqlEntity.GeneratePartitionKey(request.BrokerId), ClientRiskNoSqlEntity.GenerateRowKey(request.ClientId));
        
        if (noSqlEntity != null)
        {
            return new GetClientWithdrawalLimitsResponse
            {
                Success = true,
                ErrorMessage = String.Empty,
                CardDepositsSummary = new CircleClientDepositSummary
                {
                    DepositLast30DaysInUsd = noSqlEntity.CardDepositsSummary.DepositLast30DaysInUsd,
                    DepositLast14DaysInUsd = noSqlEntity.CardDepositsSummary.DepositLast14DaysInUsd,
                    DepositLast7DaysInUsd = noSqlEntity.CardDepositsSummary.DepositLast7DaysInUsd,
                    DepositLast1DaysInUsd = noSqlEntity.CardDepositsSummary.DepositLast1DaysInUsd,
                    Deposit30DaysLimit = noSqlEntity.CardDepositsSummary.Deposit30DaysLimit,
                    Deposit7DaysLimit = noSqlEntity.CardDepositsSummary.Deposit7DaysLimit,
                    Deposit1DaysLimit = noSqlEntity.CardDepositsSummary.Deposit1DaysLimit,
                    Deposit30DaysState = noSqlEntity.CardDepositsSummary.Deposit30DaysState,
                    Deposit7DaysState = noSqlEntity.CardDepositsSummary.Deposit7DaysState,
                    Deposit1DaysState = noSqlEntity.CardDepositsSummary.Deposit1DaysState,
                    BarInterval = noSqlEntity.CardDepositsSummary.BarInterval,
                    BarProgres = noSqlEntity.CardDepositsSummary.BarProgres,
                    LeftHours = noSqlEntity.CardDepositsSummary.LeftHours,
                    LastDeposit30DaysLeftHours = noSqlEntity.CardDepositsSummary.LastDeposit30DaysLeftHours,
                    LastDeposit7DaysLeftHours = noSqlEntity.CardDepositsSummary.LastDeposit7DaysLeftHours,
                    LastDeposit1DaysLeftHours = noSqlEntity.CardDepositsSummary.LastDeposit1DaysLeftHours,
                }
            };
        }

        return await _limitsRiskService.GetClientWithdrawalLimitsAsync(request);
    }

    public Task SetClientDepositLimitsAsync(SetClientDepositLimitsRequest request)
    {
        throw new NotImplementedException();
    }
}