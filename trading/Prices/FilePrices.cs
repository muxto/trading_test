using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace trading.Prices
{
    public class FilePrices : PriceSourceBase
    {
        private string FilePath;

        public FilePrices(string filePath)
        {
            FilePath = filePath;
        }

        public override decimal[] GetPrices()
        {
           return  ReadFromFile();
        }

        private decimal[] ReadFromFile()
        {
            var prices = System.IO.File.ReadAllLines(FilePath)
                .Select(x => decimal.Truncate ( decimal.Parse(x) *10000) / 10000).ToArray();
            return prices;


        }
    }
}
