using System;
using System.Collections.Generic;
using System.Text;

namespace trading
{
    public abstract class TradingBotBase
    {
        public abstract string GetBotName();

        private int _days = 0;
        private string _tab = "\t";


        private decimal _initMoney;
        public decimal InitMoney => _initMoney;


        private decimal _money;
        public decimal Money => _money;

        private readonly decimal _brokerFee;

        private int _balance = 0;
        public int Balance => _balance;

        private decimal _currentBuyPrice;
        public decimal CurrentBuyPrice => _currentBuyPrice;

        private decimal _currentSellPrice;
        public decimal CurrentSellPrice => _currentSellPrice;

        private decimal _averagePrice = 0m;
        private void UpdateAveragePrice()
        {
            _averagePrice = (_averagePrice * (_balance -1) + _currentBuyPrice) / (_balance);
        }
        public decimal AveragePrice => _averagePrice;


        public decimal TotalStonks => _balance * _currentSellPrice;
        public decimal Total => TotalStonks + _money;

        private int _stonksBuy = 0;
        private int _stonksSell = 0;
        private decimal _totalBrokerFee = 0m;

        public string Info => $"Stonks buy {_stonksBuy}, stonks sell {_stonksSell}, total broker fee {_totalBrokerFee}";

        private List<decimal> _buyPriceList = new List<decimal>();
        public string InfoBuy => string.Join(", ", _buyPriceList);


        private List<decimal> _sellPriceList = new List<decimal>();
        public string InfoSell => string.Join(", ", _sellPriceList);



        public List<string> AdditinalInfo = new List<string>();


        public TradingBotBase(decimal initMoney, decimal brokerFee)
        {
            _initMoney = initMoney;
            _money = initMoney;
            _brokerFee = brokerFee;
        }

        public void Trade(decimal buyPrice, decimal sellPrice)
        {
            _currentBuyPrice = buyPrice;
            _currentSellPrice = sellPrice;

            BuyOrSell();

            _days++;
        }

        private void BuyOrSell ()
        {
            if (SellDecision())
            {
                SellOneStonk();
                return;
            }

            if (BuyDecision())
            {
                BuyOneStonk();
                //return;
            }
        }



        private void BuyOneStonk()
        {
            var buyPrice = _currentBuyPrice;

            var b = GetPriceAndFeeBuy(buyPrice);
            if (_money < b)
            {
                return;
            }

            _money -= b;
            _balance++;

            UpdateAveragePrice();
            _stonksBuy++;
            _totalBrokerFee += GetFee(buyPrice);
            _buyPriceList.Add(buyPrice);
        }

        private void SellOneStonk()
        {
            if (_balance < 1)
            {
                return;
            }
            var sellPrice = CurrentSellPrice;

            _money += GetPriceAndFeeSell(sellPrice);
            _balance--;

            _stonksSell++;
            _totalBrokerFee += GetFee(sellPrice);
            _sellPriceList.Add(sellPrice);
        }

        protected abstract bool BuyDecision();

        protected abstract bool SellDecision();

        protected decimal GetPriceAndFeeBuy(decimal price)
        {
            return price + GetFee(price);
        }

        protected decimal GetPriceAndFeeSell(decimal price)
        {
            return price - GetFee(price);
        }

        protected decimal GetFee(decimal price)
        {
            return price * _brokerFee;
        }
    }
}
