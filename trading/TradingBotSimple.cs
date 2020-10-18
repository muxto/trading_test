using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace trading
{
    public class TradingBotSimple : TradingBotBase
    {
        public decimal BuyLimitPercent = 0m;
        private decimal _buyLimit => (BuyLimitPercent/100) * CurrentBuyPrice;


        public decimal SellLimitPercent = 0.6m;
        private decimal _sellLimit => (SellLimitPercent/100) * CurrentSellPrice;



        private List<decimal> SellPriceList = new List<decimal>();

        private void UpdateSellPrice() => SellPriceList.Add(CurrentSellPrice);

        private decimal AverageSellPrice => SellPriceList.Average();
        private decimal AverageSellPrice100 => SellPriceList.TakeLast(100).Average();


        private int GetExtremum ()
        {
            var count = SellPriceList.Count;
            if (count < 3)
            {
                return 0;
            }

            var x1 = SellPriceList[count - 3];
            var x2 = SellPriceList[count - 2];
            var x3 = SellPriceList[count - 1];

            if (x2 > x1 && x2 > x3) return 1;
            if (x2 < x1 && x2 < x3) return -1;

            return 0;
        }


        private decimal GetSellPriceDiff()
        {
            if (SellPriceList.Count < 2)
            {
                return 0;

            }
            return  CurrentSellPrice - SellPriceList[SellPriceList.Count - 2];
        }



        public override string GetBotName()
        {
            return "TradingBot Simple";
        }

        public TradingBotSimple(decimal initMoney, decimal brokerFee ) : base(initMoney, brokerFee)
        {
        }

      

        protected override bool BuyDecision()
        {
            //return true; 

            var enoughMoney = Money > GetPriceAndFeeBuy(CurrentBuyPrice);
            if (!enoughMoney) return false;

            // первая покупка
            if (Balance == 0)
            {
                return true;
            }

            //   var diff = GetSellPriceDiff();
            //   if (diff <= 0)
            //   {
            //       return true;
            //   }

            //   return false;
            //
            //    var simpleBuy = BuyDecisionSimple();
            //    if (simpleBuy) return true;

            //    var moreBuy = BuyDecisionMore();
            //    if (moreBuy) return true;

          //  var simpleBuyEnoughBudget = TotalStonks < (Total * 0.5m);
          //  if (!simpleBuyEnoughBudget) return false;

            var simpleBuy = BuyDecisionSimple();
            if (simpleBuy) return true;

            var extremum = GetExtremum();
          if (extremum < 0)
          {
             var averageBuyEnoughBudget = TotalStonks < (Total *0.5m);
             if (!averageBuyEnoughBudget) return false;
            
                 var averageBuy = BuyDecisionAverage();
                 if (averageBuy) return true;
            }


            // return false;

            //   var averageBuyEnoughBudget = TotalStonks < (Total / 4);
            //   if (!averageBuyEnoughBudget) return false;
            //  
            //       var averageBuy = BuyDecisionAverage();
            //       if (averageBuy) return true;

            return false;

            bool BuyDecisionSimple()
            {
                var pricaAndFee =  GetPriceAndFeeBuy(CurrentBuyPrice);
                // докупаем, когда цена покупки с комиссией и лимитом все равно меньше средней
                var simpleBuy = (AveragePrice - _buyLimit) > pricaAndFee;
                return simpleBuy;
            }


            bool BuyDecisionMore()
            {

                var moreBuy = (AveragePrice + _buyLimit) > GetPriceAndFeeBuy(CurrentBuyPrice);
                return moreBuy;
            }

            bool BuyDecisionAverage()
            {
                // return true;
                 return AveragePrice < AverageSellPrice;

                var decisionAverage = AveragePrice < AverageSellPrice100;

                if (decisionAverage)
                {

                }

                return decisionAverage;

                // докупаем, если покупка даст выгодное усреднение на текущую цену продажи
                var newAveragePrice = ((Balance * AveragePrice) + CurrentBuyPrice) / (Balance + 1);

                var averageBuy = newAveragePrice < GetPriceAndFeeSell(AverageSellPrice);


                if (averageBuy)
                {
                  //  AdditinalInfo.Add($"averageBuy currentSellPrice {CurrentSellPrice}, averageSellrice {AverageSellPrice}, AveragePrice {AveragePrice}");
                }

                //var averageBuy = SellDecision(newAveragePrice);
                return averageBuy;
            }
        }

        protected override bool SellDecision()
        {
            UpdateSellPrice();


         var extremum = GetExtremum();
        if (extremum > 0)
         {
           return SellDecision(AveragePrice);
         }
        
            return false;


            
        }

        private bool SellDecision (decimal averagePrice)
        {
            var canSell = Balance > 1;
            if (!canSell) return false;

            // продаем, когда цена продажи с комиссией и лимитом все равно больше средней
            return (averagePrice + _sellLimit) < GetPriceAndFeeSell(CurrentSellPrice);
        }








    }
}
