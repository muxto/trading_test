using System;
using System.Collections.Generic;
using System.Linq;




namespace trading
{
    class Program
    {
        static void Main(string[] args)
        {

        //   var intraday = new Prices.IntradayGenerator();
        //   var price =  intraday.GetPrices();
        //
        //   var priceLines = price.Select(x => x.ToString()).ToArray();
        //
        //   System.IO.File.WriteAllLines("intraday.generated.txt", priceLines);

           var tradeTest = new TradeManager();
               tradeTest.Run();
            //tradeTest.GetBestLimits();

            Console.ReadKey();
        }
    }
}