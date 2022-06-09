using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Service.ClientRiskManager.Grpc;
using Service.ClientRiskManager.Grpc.Models;
using Service.ClientRiskManager.Settings;

namespace Service.ClientRiskManager.Services
{
    public class ClientLimitsRiskService: IClientLimitsRiskService
    {
        private readonly ILogger<ClientLimitsRiskService> _logger;

        public ClientLimitsRiskService(ILogger<ClientLimitsRiskService> logger)
        {
            _logger = logger;
        }

        public Task<GetClientWithdrawalLimitsResponse> GetClientWithdrawalLimitsAsync(GetClientWithdrawalLimitsRequest request)
        {
            _logger.LogInformation("Hello from {name}", request.ClientId);

            return Task.FromResult(new GetClientWithdrawalLimitsResponse
            {
                ErrorMessage = "Hello " + request.ClientId
            });
        }
    }
}
