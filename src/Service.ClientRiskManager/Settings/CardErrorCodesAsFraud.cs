using System;
using System.Collections.Generic;
using System.Linq;

namespace Service.ClientRiskManager.Settings
{
    public static class CardErrorCodesAsFraud
    {
        private static HashSet<MyJetWallet.Circle.Models.Cards.CardVerificationError> _errorCodes = null;
        public static HashSet<MyJetWallet.Circle.Models.Cards.CardVerificationError> ErrorCodes
        {
            get
            {
                if (_errorCodes == null)
                {
                    var codes = Program.Settings.CardErrorCodesAsFraud.Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);

                    _errorCodes = codes.Select(x =>
                    {
                        return (Enum.TryParse<MyJetWallet.Circle.Models.Cards.CardVerificationError>(x, out var res), res);
                    }).Where(x => x.Item1).Select(x => x.res).ToHashSet();
                }

                return _errorCodes;
            }
        }
    }
}
