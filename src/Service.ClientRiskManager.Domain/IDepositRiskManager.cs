using System.Collections.Generic;
using System.Threading.Tasks;
using Service.Bitgo.DepositDetector.Domain.Models;
using Service.ClientRiskManager.Domain.Models;

namespace Service.ClientRiskManager.Domain;

public interface IDepositRiskManager
{
    Task ApplyNewDepositAsync(Deposit message);
    Task RecalculateAllAsync();
    Task<ClientRiskNoSqlEntity> GetAndRecalculateClientLastMonthRawAsync(string clientId, string brokerId);
}