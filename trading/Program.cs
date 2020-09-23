using System;
using System.Collections.Generic;
using System.Linq;




namespace trading
{
    class Program
    {


        class StockDay
        {
            public int Period { get; set; }
            public int Day { get; set; }
            public DateTime Date { get; set; }
            public double Trend { get; set; }
            public double Opening { get; set; }
            public double Max { get; set; }
            public double Min { get; set; }
        }


        class Bag
        {
            Stonks _stonks { get; set; }

            public Bag(Stonks stonks)
            {
                _stonks = stonks;
            }

            // текущая сумма денег
            public double Money { get; set; }
            // денег на отдельном счете, выведено
            public double MoneySaved { get; set; }

            // всего комиссии заплачено
            public double Stonks { get; set; }


            // флажок - нет акций в портфеле
            public bool FirstTime = true;


            // средняя цена за акцию в портфеле
            public double PriceAvg { get; set; }

            // общее количество заплаченных комиссий
            public double TotalFee { get; set; }

            public bool BuyOneStonk()
            {
                if (Money > _stonks.GetPriceAndFee())
                {
                    Money -= _stonks.GetPriceAndFee();

                    TotalFee += _stonks.GetFee();

                    PriceAvg = (Stonks * PriceAvg + _stonks.CurrentPrice) / (Stonks + 1);
                    Stonks++;

                    Console.WriteLine($"Купил акцию за {_stonks.CurrentPrice:N2}$ + {_stonks.GetSellFee():N2}$, всего {Stonks}, средняя {PriceAvg:N2}$, осталось {Money}$");

                    return true;
                }

                return false;
            }

            public bool SellOneStonk()
            {
                if (Stonks > 0)
                {
                    Money += _stonks.GetCurrentSellPriceAndFee();

                    TotalFee += _stonks.GetSellFee();
                    Stonks--;

                    Console.WriteLine($"Продал акцию за {_stonks.GetCurrentSellPrice():N2}$ + {_stonks.GetSellFee():N2}$, всего {Stonks}, средняя {PriceAvg:N2}$, осталось {Money}$");
                    return true;
                }

                return false;
            }
        }

        class Options
        {
            // начальная сумма денег
            public double InitMoney = 1000;

            // комиссия
            public double MoneyFee = 0.0003;

            // разница в цене выбирается минимальная
            // условие покупки акции, цена % от средней
            public double PriceToBuy = 0.99;
            // условие покупки акции, цена в $ от средней
            public double PriceToBuyDollar = 0.02;

            // разница в цене выбирается минимальная
            // условие продажи акции, цена % от средней
            public double PriceToSell = 1.01;
            // условие продажи акции, цена в $ от средней
            public double PriceToSellDollar = 0.02;

            // выводить ли деньги
            public bool SaveMoney = false;
            // выводить начиная с какой суммы
            public double SaveMoneyMore;

            // количество изменений за 1 отчетный период (день)
            public int PeriodTicks = 600;
            // макс количество периодов для тренда
            public int MaxPeriod = 90;
            // мин количество периодов для тренда
            public int MinPeriod = 1;

            // максимальная разница в разы между начальной ценой и ценой в конце периода
            public double MaxTrendVolatile = 1.2;

            // максимальная разница в разы между начальной ценой и ценой в конце периода за тик
            public double SpreadTrendVolatile = 0.05;
            // максимальная разница в разы к текущей цене
            public double SpreadPriceVolatile = 0.1;

            // разница в покупке-продаже в стакане выбирается максимальная
            // разница в покупке-продаже в %
            public double SpreadDiff = 0.01;
            // разница в покупке-продаже в $
            public double SpreadDiffDollar = 0.02;

            public double InitPrice = 45;

            public bool ReadStockDaysFromFile = true;

            public Options()
            {
                SaveMoneyMore = InitMoney;
            }

        }

        class Stonks
        {
            Options _options { get; set; }

            public Stonks(Options options)
            {
                _options = options;
            }

            // начальная цена акции
            public double InitPrice { get; set; }

            public double CurrentPrice { get; set; }

            public double GetCurrentSellPriceAndFee()
            {
                return GetCurrentSellPrice() + GetSellFee();
            }

            public double GetSellFee()
            {
                return GetCurrentSellPrice() * _options.MoneyFee;
            }

            public double GetCurrentSellPrice()
            {
                return CurrentPrice - Math.Max(CurrentPrice * _options.SpreadDiff, _options.SpreadDiffDollar);
            }

            public double GetPriceAndFee()
            {
                return CurrentPrice + GetFee();
            }

            public double GetFee()
            {
                return CurrentPrice * _options.MoneyFee;
            }
        }


        static void Main(string[] args)
        {
            /// Программуля-игруля для эмуляции движения цен на акции

            Options options = new Options();

            Stonks stonks = new Stonks(options);

            Bag bag = new Bag(stonks);
            bag.Money = options.InitMoney;

            Random r = new Random();

            while (true)
            {
                foreach (var s in GetStockDay())
                {
                    stonks.CurrentPrice = s.Opening;

                    var maxToday = stonks.CurrentPrice;
                    var minToday = stonks.CurrentPrice;

                    Console.WriteLine("");
                    Console.WriteLine($"Начальная цена {stonks.InitPrice}$, период {s.Period} дней, тренд {s.Trend}");
                    Console.WriteLine($"Открытие {s.Opening}$, минимум {s.Min}$, максимум {s.Max}$");
                    var countStonks = (int)(options.InitMoney / stonks.InitPrice);

                    Console.WriteLine($"Купил бы {countStonks} акций в начале за {stonks.InitPrice}$, сейчас бы стоили {(countStonks * stonks.CurrentPrice):N2}");
                    Console.WriteLine($"Новый период {s.Day}/{s.Period} ");
                    Console.WriteLine($"Идеальная трендная цена {s.Trend}$");
                    Console.WriteLine($"Акций {bag.Stonks}, средняя {bag.PriceAvg:N2}$, сейчас стоят {(bag.Stonks * stonks.CurrentPrice):N2}");

                    var sellWant = Math.Min(bag.PriceAvg * options.PriceToSell, bag.PriceAvg + options.PriceToSellDollar);

                    Console.WriteLine($"Продам за {sellWant}");
                    Console.WriteLine($"Денег осталось {bag.Money}$, отложил {bag.MoneySaved}$, всего вместе {(bag.Money + bag.MoneySaved + bag.Stonks * stonks.CurrentPrice):N2}$");
                    Console.WriteLine($"Комиссии выплачено {bag.TotalFee:N2}$");

                    Console.ReadKey();


                    var newTargetPrice = true;
                    int targetPrice = 0;

                    for (int j = 0; j < options.PeriodTicks; j++)
                    {
                        if (newTargetPrice)
                        {
                            newTargetPrice = false;
                            targetPrice = r.Next((int)s.Min, (int)s.Max);
                            targetPrice = targetPrice > 0 ? targetPrice : 0;

                            Console.WriteLine($"Целевая цена {targetPrice}$");
                        }

                        var direction = stonks.CurrentPrice < targetPrice ? 1 : -1;
                        var addPrice = (r.NextDouble() - 0.5) * 2 + 0.2 * direction;

                        if (options.ReadStockDaysFromFile)
                        {
                            if (((stonks.CurrentPrice + addPrice) > s.Max) ||
                                  (stonks.CurrentPrice + addPrice) < s.Min)
                            {
                                addPrice = 0;
                                newTargetPrice = true;
                            }
                        }

                        stonks.CurrentPrice += addPrice;

                        maxToday = maxToday > stonks.CurrentPrice ? maxToday : stonks.CurrentPrice;
                        minToday = minToday < stonks.CurrentPrice ? minToday : stonks.CurrentPrice;

                        Console.WriteLine($"{s.Date.ToShortDateString()} {s.Date.ToShortTimeString()} Покупка {stonks.CurrentPrice:N2} Продажа {stonks.GetCurrentSellPrice():N2}");

                        //System.Threading.Thread.Sleep(10);

                        var currentPriceInt = (int)stonks.CurrentPrice;

                        if (currentPriceInt == targetPrice)
                        {
                            newTargetPrice = true;
                        }




                        if (bag.Stonks == 0)
                        {
                            bag.BuyOneStonk();
                            //bag.FirstTime = false;
                        }

                        bool buy = true;
                        while (buy)
                        {
                            if ((stonks.GetPriceAndFee() < bag.PriceAvg * options.PriceToBuy) ||
                               (stonks.GetPriceAndFee() < bag.PriceAvg - options.PriceToBuyDollar))
                            {
                                buy = bag.BuyOneStonk();
                                continue;
                            }

                            buy = false;
                        }


                        bool sell = true;
                        while (sell)
                        {
                            if ((stonks.GetCurrentSellPriceAndFee() > bag.PriceAvg * options.PriceToSell) ||
                                (stonks.GetCurrentSellPriceAndFee() > bag.PriceAvg + options.PriceToSellDollar))
                            {
                               sell =  bag.SellOneStonk();
                                continue;
                            }
                            sell = false;
                        }

                        if (options.SaveMoney)
                        {
                            if (bag.Money > options.SaveMoneyMore)
                            {
                                var saved = bag.Money - options.SaveMoneyMore;
                                bag.MoneySaved += saved;
                                bag.Money = options.SaveMoneyMore;

                                Console.WriteLine($"Отложил {saved}$, всего {bag.MoneySaved}$, осталось {bag.Money}$");
                            }
                        }

                        s.Date = s.Date.AddMinutes(1);

                        
                    }
                    Console.WriteLine($"Минимум за сегодня {minToday}$, максимум за сегодня {maxToday}$");

                }
                if (options.ReadStockDaysFromFile)
                {
                    break;
                }
            }


            IEnumerable<StockDay> GetStockDay()
            {
                StockDay[] stockDays;

                if (options.ReadStockDaysFromFile)
                {
                    stockDays = ReadStockDaysFromFile();
                    stonks.InitPrice = stockDays[0].Opening;
                    stonks.CurrentPrice = stonks.InitPrice;
                }
                else
                {
                    stonks.InitPrice = options.InitPrice;
                    stonks.CurrentPrice = stonks.InitPrice;
                    stockDays = GenerateStockDays();
                }

                foreach (var s in stockDays)
                {
                    yield return s;
                }
            }

            StockDay[] GenerateStockDays()
            {
                // определяем длину периода
                var period = r.Next(options.MinPeriod, options.MaxPeriod);

                int minTrend = (int)(stonks.CurrentPrice > 0 ? stonks.CurrentPrice / options.MaxTrendVolatile : 0);
                int maxTrend = (int)(stonks.CurrentPrice * options.MaxTrendVolatile);

                // конечная цена
                double trendPrice = r.Next(minTrend, maxTrend);

                // максимальное колебание за тик по разнице тренда
                var spreadByTrendMax = Math.Abs(trendPrice - stonks.InitPrice) * options.SpreadTrendVolatile;
                // максимальное колебание за тик по начальной цене
                var spreadByInitMax = stonks.InitPrice * options.SpreadPriceVolatile;

                // размер окна колебания
                var spreadMax = Math.Max(spreadByTrendMax, spreadByInitMax);

                List<StockDay> list = new List<StockDay>();

                // новый день
                for (int i = 0; i < period; i++)
                {
                    var trend = stonks.InitPrice + ((trendPrice - stonks.InitPrice) / period * (i+1));
                    var min = trend - spreadMax;
                    var max = trend + spreadMax;
                    var opening = stonks.CurrentPrice;

                    StockDay oneDay = new StockDay()
                    {
                        Period = period,
                        Day = i + 1,
                        Date = DateTime.Today,
                        Trend = trend,
                        Min = min,
                        Max = max,
                        Opening = opening
                    };
                    list.Add(oneDay);
                }
                return list.ToArray();
            }






            StockDay[] ReadStockDaysFromFile()
            {
                var lines = System.IO.File.ReadAllLines("file.csv")
                .Skip(1).
                ToArray();

                var ff = lines.Skip(1).Reverse().Select(x =>
                {
                    var y = x
                    .Replace(",\"", "\t")
                    .Replace("\"", "")
                    .Split("\t");

                    var date = DateTime.Parse(y[0]);
                    //var trend = double.Parse(y[1]);
                    var opening = double.Parse(y[2]);
                    var max = double.Parse(y[3]);
                    var min = double.Parse(y[4]);
                    return new StockDay()
                    {
                        Date = date,
                        Trend = 0,
                        Opening = opening,
                        Max = max,
                        Min = min
                    };
                }).ToArray();

                for (int i = 0; i < ff.Length; i++)
                {
                    ff[i].Period = ff.Length;
                    ff[i].Day = i + 1;
                    if (i < ff.Length-1)
                    {
                        ff[i].Trend = ff[i + 1].Opening;
                    }
                }





                return ff;
            }

            Console.ReadKey();
        }

    }
}