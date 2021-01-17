using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace trading
{
    public class TradingBotBuy : TradingBotBase
    {

        class BidLevel
        {
            public decimal Price { get; set; }
            public int AvailableCount { get; set; }
            public int ExpectedCount { get; set; }
        }

        public decimal BuyLimitPercent = 2m;
        private decimal _buyLimit => (BuyLimitPercent / 100) * CurrentBuyPrice;


        public decimal SellLimitPercent = 2m;
        private decimal _sellLimit => (SellLimitPercent / 100) * CurrentSellPrice;



        public decimal StopLossPercent = 20m;
        private decimal _stopLoss => (StopLossPercent / 100) * CurrentSellPrice;



        private List<decimal> SellPriceList = new List<decimal>();

        private void UpdateSellPrice() => SellPriceList.Add(CurrentSellPrice);

        private decimal AverageSellPrice => SellPriceList.Average();
        private decimal AverageSellPriceLast => SellPriceList.TakeLast(20000).Average();


        public override string GetBotName()
        {
            return "TradingBot Buy";
        }

        public TradingBotBuy(decimal initMoney, decimal brokerFee) : base(initMoney, brokerFee)
        {
        }




        protected override bool BuyDecision()
        {
            return false;

        }

        protected override bool SellDecision()
        {
            return false;
        }


        public decimal MoneyLevel = 0.60m;

        public decimal maxLevelDepth = 0.95m;
        public decimal minLevelDepth = 0.3m;


        public decimal bidLimitMoney = 0.05m;
        public decimal askLimitMoney = 0.10m;



        private decimal HighPrice;
        private decimal BestBalance;

        private decimal freeMoney => Money - GetReservedMoney();


        private bool HighPriceUpdated()
        {
            if (HighPrice >= CurrentBuyPrice)
            {
                return false;
            }

            HighPrice = CurrentBuyPrice;
            return true;
        }


        protected override void AskAndBidDecision()
        {
            if (Balance == 0)
            {
                var count = (int)((Money * MoneyLevel) / GetPriceAndFeeBuy(CurrentBuyPrice));

                AddBid(CurrentBuyPrice, -1, count);

                BestBalance = count;

                return;
            }

            if (HighPriceUpdated())
            {
                ClearBidsInProgress();
            }

            var doneBids = Bids.Where(x => x.OrderResult == Order.OrderResults.Done);
            foreach (var bid in doneBids)
            {
                if (bid.ProfitSellPrice == -1)
                {
                    continue;
                }

                AddAsk(bid.ProfitSellPrice, bid.Count);
            }

            var dict = GetLevels();
            foreach (var d in dict)
            {
                if (d.AvailableCount == 0) continue;

                var sellPrice = d.Price + (askLimitMoney * CurrentBuyPrice);
                AddBid(d.Price, sellPrice, d.AvailableCount);
            }
        }

        private BidLevel[] GetLevels()
        {
            var bidLevels = GetBidLevels();
            var prices = bidLevels.Where(x => x.Price <= CurrentBuyPrice).ToArray();

            foreach (var p in prices)
            {
                var actualBids = Bids.Where(x => x.Price == p.Price && x.OrderResult == Order.OrderResults.InProcess).ToArray();
                var actualBidsCount = actualBids.Sum(x => x.Count);

                if (actualBidsCount == p.ExpectedCount)
                {
                    continue;
                }

                var sum = bidLevels.Sum(x => x.Price * x.AvailableCount);
                sum = GetPriceAndFeeBuy(sum);

                var availableCount = (int)((freeMoney - sum) / p.Price);
                if (availableCount == 0)
                {
                    return bidLevels.ToArray();
                }

                var bidCount = p.ExpectedCount - actualBidsCount;
                p.AvailableCount = Math.Min(availableCount, bidCount);
            }
            return bidLevels.ToArray();
        }

        private BidLevel[] GetBidLevels()
        {
            var bidLevels = new List<BidLevel>();
            var maxLevelPrice = HighPrice * maxLevelDepth;
            var minLevelPrice = HighPrice * minLevelDepth;

            var n =(int)( Money * 2 / (maxLevelPrice + minLevelPrice));
            var step = (maxLevelPrice - minLevelPrice) / n;


            var bidCount = 1;
            
            while (step < 0.01m)
            {
                step = step * 2;
                bidCount = bidCount * 2;
            }

            step = Math.Round(step, 2);


            var price = maxLevelPrice;
            for (var i = 0; i< n; i++)
            {
                var bidLevel = new BidLevel()
                {
                    Price = price,
                    ExpectedCount = bidCount
                };

                bidLevels.Add(bidLevel);
                price -= step;
            }

            while (true)
            {
                var sum = bidLevels.Sum(x => x.Price * x.ExpectedCount);
                var diff = Money - sum;
                if (diff > 0)
                {
                    break;
                    
                }
                bidLevels.RemoveAt(bidLevels.Count - 1);
            }

            return bidLevels.ToArray();
        }
    }
}