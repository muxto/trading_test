using System;
using System.Collections.Generic;
using System.Text;

namespace trading.Prices
{
    public class IntradayGenerator : PriceSourceBase
    {
        class Options
        {
            // количество дней
            public int Days = 30;

            // количество изменений за день
            public int DayTicks = 192;

            // максимальная разница в разы между начальной ценой и ценой в конце периода
            //    public decimal MaxTrendVolatile = 1.06m;

            // максимальная разница в разы за тик
            public decimal MaxVolatile = 0.02m;

            public decimal InitPrice = 20;
        }

        Random r = new Random();

        private decimal GetRandomNumber(decimal current, decimal maxVolatile)
        {
            var sign = GetSign();
            if (sign == 0) return current;

            var x = (decimal)r.NextDouble();
            var newNumber = current + sign * current * maxVolatile * x;
            return newNumber;
        }

        private int GetSign()
        {
            var x = r.Next(250);

            if (x > 200) return 0;
            if (x > 100 && x <= 200) return 1;
            return -1;
        }

        public override decimal[] GetPrices()
        {
            Options options = new Options();

            var currentPrice = options.InitPrice;

            List<decimal> prices = new List<decimal>();

            for (int d = 0; d < options.Days; d++)
            {
                for (int t = 0; t < options.DayTicks; t++)
                {
                    currentPrice = GetRandomNumber(currentPrice, options.MaxVolatile);
                    prices.Add(currentPrice);
                }
            }

            return prices.ToArray();
        }
    }
}
