using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Service.Bitgo.DepositDetector.Domain.Models;

namespace Service.ClientRiskManager.Domain;

public class DepositRiskManager : IDepositRiskManager
{
    private const string CircleCard = "CircleCard";
    
    private readonly ILogger<DepositRiskManager> _logger;

    public DepositRiskManager(ILogger<DepositRiskManager> logger)
    {
        _logger = logger;
    }

    public async Task ApplyNewDepositAsync(IReadOnlyList<Deposit> messages)
    {
        foreach (var message in messages)
        {
            if (message.Status != DepositStatus.Processed)
            {
                continue;
            }
            
            if (message.Integration == CircleCard)
            {
                //TODO: Add cards
                continue;
            }
        }
        
    }
}