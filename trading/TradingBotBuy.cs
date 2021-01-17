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
            public int Count { get; set; }
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

        public int levelsDepth = 30;

        public decimal bidLimitMoney = 0.05m;
        public decimal bidLimitStonks = 0.05m;
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


        Dictionary<decimal, decimal> BidAskEquality = new Dictionary<decimal, decimal>();

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
                //ClearAsks();
                ClearBidsInProgress();
            }

            // var maxLevelPrice = CurrentBuyPrice * maxLimit;
            // var minLevelPrice = CurrentBuyPrice * minLimit;
            // var profit = CurrentBuyPrice * profitLimit;



            var doneBids = Bids.Where(x => x.OrderResult == Order.OrderResults.Done);
            foreach (var bid in doneBids)
            {
                if (bid.ProfitSellPrice == -1)
                {
                    continue;
                }

                var stonksSum = Balance * GetPriceAndFeeSell(CurrentSellPrice);

                var stonksLevel = stonksSum / MoneyLevel;
                var moneyLevel = Money / (1 - MoneyLevel);

                AddAsk(bid.ProfitSellPrice, bid.Count);
            }


            


            var dict = GetLevels();

            var c = 0;
            foreach (var d in dict)
            {
               if (c > 5) break;
                var sellPrice = d.Price + (askLimitMoney * CurrentBuyPrice);
                AddBid(d.Price, sellPrice, d.Count);
                c++;
            }

          //  if (BuyDecisionSimple() )
          //  {
          //      var price = GetPriceAndFeeBuy(CurrentBuyPrice);
          //      
          //      var sellPrice = AveragePrice + ((SellLimitPercent / 100) * AveragePrice);
          //      AddBid(price, sellPrice, 1);
          //  }

        }

        bool BuyDecisionSimple()
        {

            var pricaAndFee = GetPriceAndFeeBuy(CurrentBuyPrice);

            if (freeMoney < GetPriceAndFeeBuy(CurrentBuyPrice))
            {
                return false;
            }

            // докупаем, когда цена покупки с комиссией и лимитом все равно меньше средней
            var simpleBuy = (AveragePrice - _buyLimit) > pricaAndFee;
            return simpleBuy;
        }


        private BidLevel[] GetLevels()
        {
            var dict = new List<BidLevel>();

            var bidLevels = GetBidLevels();
            var prices = bidLevels.Where(x => x.Price <= CurrentBuyPrice).ToArray();

            foreach (var p in prices)
            {
                var actualBids = Bids.Where(x => x.Price == p.Price && x.OrderResult == Order.OrderResults.InProcess).ToArray();
                var actualBidsCount = actualBids.Sum(x => x.Count);

                if (actualBidsCount == p.Count)
                {
                    continue;
                }

                var sum = dict.Sum(x => x.Price* x.Count);
                sum = GetPriceAndFeeBuy(sum);

                var availableCount = (int)((freeMoney - sum) / p.Price);
                if (availableCount == 0)
                {
                    return dict.ToArray();
                }

                var bidCount = p.Count - actualBidsCount;
                bidCount = Math.Min(availableCount, bidCount);

                var bidLevel = new BidLevel()
                {
                    Price = p.Price,
                    Count = bidCount
                };
                dict.Add(bidLevel);

            }
            return dict.ToArray();
        }

        private BidLevel[] GetBidLevels()
        {
            var bidLevels = new List<BidLevel>();
            var price = HighPrice;
            var bidCount = (int)((Balance - GetReservedStonks()) * bidLimitStonks);
            for (int i = 0; i < 30; i++)
            {
                price = price * (1 - bidLimitMoney);

                var bidLevel = new BidLevel()
                {
                    Price = price,
                    Count = bidCount
                };

                bidLevels.Add (bidLevel);
            }

            return bidLevels.ToArray();
        }
    }
}