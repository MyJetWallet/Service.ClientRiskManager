using System.Collections.Generic;
using System.Threading.Tasks;
using Service.Bitgo.DepositDetector.Domain.Models;

namespace Service.ClientRiskManager.Domain;

public interface IDepositRiskManager
{
    Task ApplyNewDepositAsync(Deposit message);
}