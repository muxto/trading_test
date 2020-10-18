using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace trading.Prices
{
    public class InvestingPrices : PriceSourceBase
    {
        protected class StockDay
        {
            public int Period { get; set; }
            public int Day { get; set; }
            public DateTime Date { get; set; }
            public decimal Mean { get; set; }
            public decimal Opening { get; set; }
            public decimal Max { get; set; }
            public decimal Min { get; set; }
        }

        private string FilePath;

        public InvestingPrices(string filePath)
        {
            FilePath = filePath;
        }

        public override decimal[] GetPrices()
        {
            var stockDays = ReadStockDaysFromFile();

            var prices = new List<decimal>();

            foreach (var s in stockDays)
            {
                prices.Add(s.Opening);
                prices.Add(s.Min);
                prices.Add(s.Max);
            }
            return prices.ToArray ();
        }

     

        private StockDay[] ReadStockDaysFromFile()
        {
            var lines = System.IO.File.ReadAllLines(FilePath).Skip(1).ToArray();

            var stockDays = lines.Skip(1).Reverse().Select(x =>
            {
                var y = x
                .Replace(",\"", "\t")
                .Replace("\"", "")
                .Split("\t");

                var date = DateTime.Parse(y[0]);
                //var trend = double.Parse(y[1]);
                var opening = decimal.Parse(y[2]);
                var max = decimal.Parse(y[3]);
                var min = decimal.Parse(y[4]);
                return new StockDay()
                {
                    Date = date,
                    Mean = 0,
                    Opening = opening,
                    Max = max,
                    Min = min
                };
            }).ToArray();

            for (int i = 0; i < stockDays.Length; i++)
            {
                stockDays[i].Period = stockDays.Length;
                stockDays[i].Day = i + 1;
                if (i < stockDays.Length - 1)
                {
                    stockDays[i].Mean = stockDays[i + 1].Opening;
                }
            }

            return stockDays;
        }

    }
}
