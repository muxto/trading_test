using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace trading
{
    public class TradingBotSimple : TradingBotBase
    {
        public decimal BuyLimitPercent = 0.2m;
        private decimal _buyLimit => (BuyLimitPercent/100) * CurrentBuyPrice;


        public decimal SellLimitPercent = 0.6m;
        private decimal _sellLimit => (SellLimitPercent/100) * CurrentSellPrice;



        public decimal StopLossPercent = 20m;
        private decimal _stopLoss => (StopLossPercent/ 100) * CurrentSellPrice;



        private List<decimal> SellPriceList = new List<decimal>();

        private void UpdateSellPrice() => SellPriceList.Add(CurrentSellPrice);

        private decimal AverageSellPrice => SellPriceList.Average();
        private decimal AverageSellPriceLast => SellPriceList.TakeLast(20000).Average();


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

            //  /^\  /^^
            if (x2 > x1 && x2 >= x3) return 1;

            //  / / / 
            if (x2 > x1 && x3 > x2) return 2;

            //  \_/  \__
            if (x2 < x1 && x2 <= x3) return -1;

            //  \ \ \
            if (x2 < x1 && x3 < x2)
            {
                if ((x1 * (1 - SellLimitPercent)) > x2 &&
                    (x2 * (1 - SellLimitPercent) > x3))
                    return -3;

                return -2;
            }



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

            //      var simpleBuy = BuyDecisionSimple();
            //      if (simpleBuy) return true;
            //   
            //       var extremum = GetExtremum();
            //     if (extremum <= 0)
            //        {
            //   
            //   
            //                var averageBuyEnoughBudget = TotalStonks < Money;
            //             if (!averageBuyEnoughBudget) return false;
            //            
            //                 var averageBuy = BuyDecisionAverage();
            //                 if (averageBuy) return true;
            //      }



                 var simpleBuy = BuyDecisionSimple();
                 if (simpleBuy) return true;

               var extremum = GetExtremum();
             if (extremum == -3)
                {
           
           
                        var averageBuyEnoughBudget = TotalStonks < Money;
                     if (!averageBuyEnoughBudget) return false;

                return true;
                        
              }


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
               //  return true;
                return AveragePrice < AverageSellPrice;

                var decisionAverage = AveragePrice < AverageSellPriceLast;

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


            return SellDecision(AveragePrice);
        }

        private bool SellDecision (decimal averagePrice)
        {
           var canSell = Balance > 0;
           if (!canSell) return false;

         //  if (averagePrice - _stopLoss > GetPriceAndFeeSell(CurrentSellPrice))
         //  {
         //      return true;
         //  }

        //    var extremum = GetExtremum();
        //    if (extremum == 1 || extremum == 0)
            {
                // продаем, когда цена продажи с комиссией и лимитом все равно больше средней
                return (averagePrice + _sellLimit) < GetPriceAndFeeSell(CurrentSellPrice);
            }

            return false;

            
        }

        protected override void AskAndBidDecision()
        {
        }
    }
}
