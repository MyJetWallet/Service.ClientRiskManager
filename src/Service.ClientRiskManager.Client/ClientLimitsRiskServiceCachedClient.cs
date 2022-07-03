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
                    DepositLast1DaysInUsd = noSqlEntity.CardDepositsSummary.DepositLast1DaysInUsd
                }
            };
        }

        return await _limitsRiskService.GetClientWithdrawalLimitsAsync(request);
    }
}