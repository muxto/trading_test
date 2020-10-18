using System;
using System.Collections.Generic;
using System.Text;

namespace trading.Prices
{
    public abstract class PriceSourceBase
    {
        public string Info { get; set; }

        public abstract decimal[] GetPrices();
    }
}
