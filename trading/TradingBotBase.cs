using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace trading
{
    public abstract class TradingBotBase
    {
        public class Order
        {
            public enum OrderTypes
            {
                Ask,
                Bid
            }

            public enum OrderResults
            {
                InProcess,
                Done
            }

            public OrderTypes OrderType;
            public Guid Guid;
            public decimal Price;
            public decimal ProfitSellPrice;
            public int Count;
            public OrderResults OrderResult;

            public Order(OrderTypes orderType, decimal price, int count)
            {
                Guid = Guid.NewGuid();
                OrderType = orderType;
                Price = price;
                Count = count;
                OrderResult = OrderResults.InProcess;
            }
        }


        public abstract string GetBotName();

        private int _number = 0;
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

        protected decimal PreviousBuyPrice => _previousBuyPrice;
        private decimal _previousBuyPrice;

        private decimal _currentSellPrice;
        public decimal CurrentSellPrice => _currentSellPrice;

        private decimal _previousSellPrice;

        private decimal _realAveragePrice = 0m;
        private decimal _averagePrice = 0m;

        protected void SetAveragePrice(decimal averagePrice)
        {
            _averagePrice = averagePrice;
        }

        protected void ResetAveragePrice()
        {
            _averagePrice = _realAveragePrice;
        }

        private void UpdateAveragePrice(int count)
        {
            _averagePrice = (_averagePrice * (_balance - count) + _currentBuyPrice * count) / (_balance);
           // _realAveragePrice = (_realAveragePrice * (_balance - 1) + _currentBuyPrice) / (_balance);
        }

        public decimal AveragePrice => _averagePrice;


        public decimal TotalStonks => _balance * _currentSellPrice;
        public decimal Total => TotalStonks + _money;

        private int _stonksBuy = 0;
        private int _stonksSell = 0;
        private decimal _totalBrokerFee = 0m;

        public string Info => $"Stonks buy {_stonksBuy}, stonks sell {_stonksSell}, total broker fee {_totalBrokerFee}";


        private List<string> _fullInfo = new List<string>();

        public List<string> FullInfo => _fullInfo;

        private void AddFullInfo(decimal buyPrice, decimal sellPrice, bool buy, bool sell)
        {
            var buyOrSellStr = buy ? "buy" : sell ? "sell" : "";


            var record = $"{_number}\t{buyPrice}\t{sellPrice}\t{buyOrSellStr}\t{AveragePrice}";
            _fullInfo.Add(record);
        }



        public List<string> AdditinalInfo = new List<string>();

        private bool _hasBuy;
        private bool _hasSell;


        private decimal _freeMoney => _money - _reservedMoney;

        private decimal _reservedMoney => GetReservedMoney();

        protected decimal GetReservedMoney()
        {
            var asksReserved = 0;// Asks.Where(x => x.OrderResult == Order.OrderResults.InProcess)
                //.Sum(x => GetFee(x.Price * x.Count));
            var bidsReserved = Bids.Where(x => x.OrderResult == Order.OrderResults.InProcess)
                .Sum(x => GetPriceAndFeeBuy(x.Price * x.Count));

            return asksReserved + bidsReserved;
        }

        protected int GetReservedStonks()
        {
            return Asks.Where(x => x.OrderResult == Order.OrderResults.InProcess).Sum(x => x.Count);
        }

        // заявки на продажу
        private List<Order> _orderList = new List<Order>();

        public List<Order> Asks => _orderList.Where(x => x.OrderType == Order.OrderTypes.Ask).ToList();
        public List<Order> Bids => _orderList.Where(x => x.OrderType == Order.OrderTypes.Bid).ToList();



        protected bool AddAsk(decimal price, int count)
        {
            if ((Balance - GetReservedStonks()) < count)
            {
                return false;
            }

            _orderList.Add(new Order(Order.OrderTypes.Ask, price, count));
            return true;
        }

        protected void ClearAsks()
        {
            _orderList = _orderList.Where(x => x.OrderType != Order.OrderTypes.Ask).ToList();
        }

        protected bool AddBid(decimal price, decimal profitPrice, int count)
        {
            if (GetPriceAndFeeBuy(price * count) > _freeMoney) return false;

            var order = new Order(Order.OrderTypes.Bid, price, count);
            order.ProfitSellPrice = profitPrice;
            
            _orderList.Add(order);
            return true;
        }

        protected void ClearBidsInProgress()
        {
            var myBids = _orderList.Where(x =>
                x.OrderType == Order.OrderTypes.Bid &&
                x.OrderResult == Order.OrderResults.InProcess
                ).ToList();

            foreach (var b in myBids)
            {
                _orderList.RemoveAll(x => x.Guid == b.Guid);
            }

        }

        private void ClearDoneOrders()
        {
            _orderList = _orderList.Where(x => x.OrderResult == Order.OrderResults.InProcess).ToList();
        }



        public TradingBotBase(decimal initMoney, decimal brokerFee)
        {
            _initMoney = initMoney;
            _money = initMoney;
            _brokerFee = brokerFee;
        }

        public void Trade(decimal buyPrice, decimal sellPrice)
        {
            _hasBuy = false;
            _hasSell = false;

            _previousBuyPrice = _currentBuyPrice;
            _previousSellPrice = _currentSellPrice;

            _currentBuyPrice = buyPrice;
            _currentSellPrice = sellPrice;

            AskAndBidDecision();
            AsksAndBids();


            BuyOrSell();

            AddFullInfo(_currentBuyPrice, _currentSellPrice, _hasBuy, _hasSell);

            _number++;
        }


        private bool IsPriceInLimits(decimal limit1, decimal limit2, decimal price)
        {
            var max = Math.Max(limit1, limit2);
            var min = Math.Min(limit1, limit2);
            return price == max || price == min || (price < max && price > min);
        }

        private void AsksAndBids()
        {
            ClearDoneOrders();

            var asks = Asks.Where(x => x.Price <= _currentSellPrice).ToList();


            foreach (var ask in asks)
            {
                SellStonks(ask.Price, ask.Count);
                ask.OrderResult = Order.OrderResults.Done;
            }

            var bids = Bids.Where(x => x.Price >= _currentBuyPrice ).ToList();

            foreach (var bid in bids)
            {
                BuyStonks(bid.Price, bid.Count);
                bid.OrderResult = Order.OrderResults.Done;
            }
        }

        private void BuyOrSell()
        {
            if (SellDecision())
            {
                SellStonks(_currentSellPrice, 1);
                return;
            }

            if (BuyDecision())
            {
                BuyStonks(_currentBuyPrice, 1);
                //return;
            }
        }

        private void BuyStonks(decimal price, int count)
        {
            var fullPrice = price * count;
            var b = GetPriceAndFeeBuy(fullPrice);
            if (_money < b)
            {
                return;
            }

            _money -= b;
            _balance += count;

            UpdateAveragePrice(count);
            _stonksBuy += count;
            _totalBrokerFee += GetFee(fullPrice);

            _hasBuy = true;
        }

        private void SellStonks(decimal price, int count)
        {
            var fullPrice = price * count;
            if (_balance - count < 1)
            {
                return;
            }


            _money += GetPriceAndFeeSell(fullPrice);
            _balance -= count;

            _stonksSell += count;
            _totalBrokerFee += GetFee(fullPrice);

            _hasSell = true;
        }

        protected abstract bool BuyDecision();

        protected abstract bool SellDecision();

        protected abstract void AskAndBidDecision();

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