using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using trading.Prices;

namespace trading
{
    public class TradeTest
    {

        // разница в покупке-продаже в стакане выбирается максимальная
        // разница в покупке-продаже в %
        // public double SpreadDiff = 0.01;
        // разница в покупке-продаже в $
        private decimal SpreadDiffDollar = 0.02m;

        private decimal InitMoney = 1000m;
        private decimal BrokerFee = 0.0005m;

        public TradeTest()
        {
        }

        public void Run()
        {
            var prices = GetPrices();

            
           
            foreach (var p in prices)
            {
                var tradingBots = GetTradingBots();

                Calculate(p, tradingBots);

                Console.WriteLine(p.Info);

                foreach(var t in tradingBots)
                {
                    ShowResult(t);
                }

            }

            
        }

        class aaa
        {
            public int b;
            public int s;
            public decimal total;
        }

        public void GetBestLimits()
        {
            
          var price = new FilePrices("intraday.generated.txt");

            List<aaa> l = new List<aaa>();

            TradingBotSimple tradingBotSimple;


            for (int b = 0; b< 20; b++ )
            {
                //int s = 3;
                for (int s = 0; s<10; s++)
                {
                    tradingBotSimple = new TradingBotSimple(InitMoney, BrokerFee);
                    tradingBotSimple.BuyLimitPercent = (decimal) b / 10;
                    tradingBotSimple.SellLimitPercent = (decimal) s / 10;


                    Calculate(price, tradingBotSimple);
                    var a = new aaa
                    {
                        b = b,
                        s = s,
                        total = tradingBotSimple.Total
                    };
                    l.Add(a);
                }
            }
            
            foreach (var ff in l.OrderBy(x=>x.total))

            {
                Console.WriteLine($"total={ff.total} b={ff.b} s={ff.s}");
            }
        }


        private PriceSourceBase[] GetPrices()
        {
            var prices = new List<PriceSourceBase>();

            var investingInfo = new string[][]
            {
           //     new string[] { "Прошлые данные - AAPL.csv", "AAPL Цена растет" },
           //     new string[] { "Прошлые данные - V.csv", "V Цена растет, а потом сидит на месте" },
           //     new string[] { "Прошлые данные - NOK.csv", "NOK Цена растет, а потом падает" },
           //     new string[] { "Прошлые данные - AIR.csv", "AIR Рынок непонятно" },

        };
            var generatedInfo = new string[][]
          {
           //  new string[] { "intraday.generated.txt", "Интрадей сгенерированный" },
          };

            var realInfo = new string[][]
         {
             new string[] { "air.txt", "Интрадей настоящий октябрь" },
             new string[] { "air full.txt", "Интрадей настоящий 2020" },
             new string[] { "air full reverse.txt", "Интрадей настоящий 2020 наоборот" },
             new string[] { "air 6.txt", "Интрадей настоящий 2020 наоборот" },
         };

            foreach (var info in investingInfo)
            {
                prices.Add(new InvestingPrices(info[0])
                {
                    Info = info[1]
                });
            }
            foreach (var info in generatedInfo)
            {
                prices.Add(new FilePrices(info[0])
                {
                    Info = info[1]
                });
            }
            foreach (var info in realInfo)
            {
                prices.Add(new FilePrices(info[0])
                {
                    Info = info[1]
                });
            }

            return prices.ToArray();
        }

        private TradingBotBase[] GetTradingBots()
        {
            var tradingBotInvestor = new TradingBotInvestor(InitMoney, BrokerFee);
            var tradingBotSimple = new TradingBotSimple(InitMoney, BrokerFee);
            //   var tradingBotClassic = new TradingBotClassic(InitMoney, BrokerFee);

            var tradingBots = new TradingBotBase[] { tradingBotInvestor, tradingBotSimple }; // , tradingBotClassic };
            return tradingBots;
        }

        private void Calculate(PriceSourceBase orderBook, params TradingBotBase[] tradingBots)
        {
            var prices = orderBook.GetPrices();

            foreach (var p in prices)
            {
                var buyPrice = GetBuyPrice(p);
                var sellPrice = GetSellPrice(p);

                foreach (var t in tradingBots)
                {
                    t.Trade(buyPrice, sellPrice);
                }
            }
        }

        private void ShowResult(TradingBotBase tradingBot)
        {
            Console.WriteLine();
            Console.WriteLine($"TradingBot {tradingBot.GetBotName()}");
            Console.WriteLine($"Init money {tradingBot.InitMoney}, money {tradingBot.Money}");
            Console.WriteLine($"Balance {tradingBot.Balance}, Total stonks {tradingBot.TotalStonks}, Total {tradingBot.Total}");
            Console.WriteLine(tradingBot.Info);
            //Console.WriteLine($"All buy {tradingBot.InfoBuy}");
            //Console.WriteLine($"All sell {tradingBot.InfoSell}");

            Console.WriteLine();
            foreach (var i in tradingBot.AdditinalInfo)
            {
                Console.WriteLine(i);
            }
        }

        private decimal GetBuyPrice(decimal price)
        {
            // return price + SpreadDiffDollar;
            return price;
        }

        private decimal GetSellPrice(decimal price)
        {
            return price - SpreadDiffDollar;
        }
    }
}