using System;
using System.Collections.Generic;
using System.Linq;

namespace Service.ClientRiskManager.Settings
{
    public static class PaymentErrorCodesAsFraud
    {
        private static HashSet<MyJetWallet.Circle.Models.Payments.PaymentErrorCode> _errorCodes = null;
        public static HashSet<MyJetWallet.Circle.Models.Payments.PaymentErrorCode> ErrorCodes
        {
            get
            {
                if (_errorCodes == null)
                {
                    var codes = Program.Settings.PaymetErrorCodesAsFraud.Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);

                    _errorCodes = codes.Select(x =>
                    {
                        return (Enum.TryParse<MyJetWallet.Circle.Models.Payments.PaymentErrorCode>(x, out var res), res);
                    }).Where(x => x.Item1).Select(x => x.res).ToHashSet();
                }

                return _errorCodes;
            }
        }
    }
}
