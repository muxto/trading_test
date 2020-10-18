using System;
using System.Collections.Generic;
using System.Text;

namespace trading
{
    public class TradingBotClassic : TradingBotBase
    {

        public class Stonk
        {
            private decimal TakeProfitPercent = 0.09m;

            private decimal StopLossPercent = 0.03m;

            public decimal Price { get; set; }

            public decimal TakeProfit => TakeProfitPercent * Price;
            
            public  decimal StopLoss => StopLossPercent * Price;
        }

        public override string GetBotName()
        {
            return "TradingBot Classic";
        }


        List<Stonk> stonks = new List<Stonk>();

        public TradingBotClassic(decimal initMoney, decimal brokerFee) : base(initMoney, brokerFee)
        {
        }


        protected override bool BuyDecision()
        {
            var enoughMoney = Money > GetPriceAndFeeBuy(CurrentBuyPrice);
            if (!enoughMoney) return false;

            if (SellDecision(false))
            {
                return false;
            }

            var s = new Stonk()
            {
                Price = CurrentBuyPrice
            };
            stonks.Add(s);
            return true;

         
        }

        protected override bool SellDecision()
        {
            return SellDecision(true);
        }

        private bool SellDecision(bool delete)
        {
            for (int i = 0; i < stonks.Count; i++)
            {
                var s = stonks[i];
                if (//s.Price - s.StopLoss <= GetPriceAndFeeSell( CurrentSellPrice )||
                    s.Price + s.TakeProfit >= GetPriceAndFeeSell(CurrentSellPrice))
                {
                    if (delete)
                    {
                        stonks.RemoveAt(i);
                    }
                    return true;
                }
            }
           
            return false;
        }








    }
}
