using System;
using System.Collections.Generic;
using System.Text;

namespace trading
{
    public class TradingBotInvestor : TradingBotBase
    {
        public override string GetBotName()
        {
            return "TradingBot Investor";
        }

        public TradingBotInvestor(decimal initMoney, decimal brokerFee) : base (initMoney, brokerFee)
        {

        }

        protected override bool BuyDecision()
        {
            return true;
        }

        protected override bool SellDecision()
        {
            return false;
        }
    }
}
