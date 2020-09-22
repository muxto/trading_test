using System;




namespace trading
{
    class Program
    {

        static void Main(string[] args)
        {
            /// Программуля-игруля для эмуляции движения цен на акции


            // начальная сумма денег
            double initMoney = 1000;

            // текущая сумма денег
            double money = initMoney;
            // денег на отдельном счете, выведено
            double moneySaved = 0;
            // комиссия
            double moneyFee = 0.0003;
            // всего комиссии заплачено
            double stonks = 0;

            // флажок - нет акций в портфеле
            bool firstTime = true;

            // средняя цена за акцию в портфеле
            double priceAvg = 0;

            // разница в цене выбирается минимальная
            // условие покупки акции, цена % от средней
            double priceToBuy = 0.95;
            // условие покупки акции, цена в $ от средней
            double priceToBuyDollar = 0.5;

            // разница в цене выбирается минимальная
            // условие продажи акции, цена % от средней
            double priceToSell = 1.05;
            // условие продажи акции, цена в $ от средней
            double priceToSellDollar = 0.5;

            // выводить ли деньги
            bool saveMoney = true;
            // выводить начиная с какой суммы
            double saveMoneyMore = initMoney;

            // общее количество заплаченных комиссий
            double allFee = 0;

            // количество отрезков для тренда (день)
            int period = 0;
            // количество изменений за 1 отчетный период (день)
            int periodTicks = 600;
            // макс количество периодов для тренда
            int maxPeriod = 90;
            // мин количество периодов для тренда
            int minPeriod = 1;

            // максимальная разница в разы между начальной ценой и ценой в конце периода
            double maxTrendVolatile = 1.2;

            // начальная цена акции
            double initPrice = 76;
            // текущая цена акции
            double currentPrice = initPrice;


            // максимальная разница в разы между начальной ценой и ценой в конце периода за тик
            double spreadTrendVolatile = 0.02;
            // максимальная разница в разы к текущей цене
            double spreadPriceVolatile = 0.01;

            // разница в покупке-продаже в стакане выбирается максимальная
            // разница в покупке-продаже в %
            double spreadDiff = 0.01;
            // разница в покупке-продаже в $
            double spreadDiffDollar = 0.2;

            Random r = new Random();

            // новый тренд
            while (true)
            {
                // определяем длину периода
                period = r.Next(minPeriod, maxPeriod);

                int minTrend = (int)(currentPrice > 0 ? currentPrice / maxTrendVolatile : 0);
                int maxTrend = (int)(currentPrice * maxTrendVolatile);

                // конечная цена
                double trendPrice = r.Next(minTrend, maxTrend);

                // максимальное колебание за тик по разнице тренда
                var spreadByTrendMax = Math.Abs(trendPrice - initPrice) * spreadTrendVolatile;
                // максимальное колебание за тик по начальной цене
                var spreadByInitMax = initPrice * spreadPriceVolatile;

                // размер окна колебания
                var spreadMax = (int)Math.Max(spreadByTrendMax, spreadByInitMax);



                // новый день
                for (int i = 1; i <= period; i++)
                {
                    var dateTime = DateTime.Today.AddHours(8);

                    var idealTrendPrice = initPrice + ((trendPrice - initPrice) / period * i);

                    Console.WriteLine("");
                    Console.WriteLine($"Начальная цена {initPrice}$, период {period} дней, тренд {trendPrice}");
                    var countStonks = (int)(initMoney / initPrice);


                    Console.WriteLine($"Купил бы {countStonks} акций в начале за {initPrice}$, сейчас бы стоили {(countStonks * currentPrice):N2}");
                    Console.WriteLine($"Новый период {i}/{period} ");
                    Console.WriteLine($"Идеальная трендная цена {idealTrendPrice}$");
                    Console.WriteLine($"Акций {stonks}, средняя {priceAvg:N2}$, сейчас стоят {(stonks * currentPrice):N2}");
                    Console.WriteLine($"Денег осталось {money}$, отложил {moneySaved}$, всего вместе {(money + moneySaved + stonks * currentPrice):N2}$");
                    Console.WriteLine($"Комиссии выплачено {allFee:N2}$");

                    Console.ReadKey();

                    var newTargetPrice = true;
                    int targetPrice = 0;

                    for (int j = 0; j < periodTicks; j++)
                    {
                        if (newTargetPrice)
                        {
                            newTargetPrice = false;
                            var balancePoint = (int)(currentPrice + idealTrendPrice) / 2;
                            targetPrice = r.Next(balancePoint - spreadMax, balancePoint + spreadMax);
                            targetPrice = targetPrice > 0 ? targetPrice : 0;

                            Console.WriteLine($"Балансная цена {balancePoint}$, целевая цена {targetPrice}$");
                        }

                        var direction = currentPrice < targetPrice ? 1 : -1;
                        var addPrice = (r.NextDouble() - 0.5) * 2 + 0.2 * direction;

                        currentPrice += addPrice;

                        Console.WriteLine($"{dateTime.ToShortDateString()} {dateTime.ToShortTimeString()} Покупка {currentPrice:N2} Продажа {GetCurrentSellPrice():N2}");

                        //System.Threading.Thread.Sleep(10);

                        var currentPriceInt = (int)currentPrice;

                        if (currentPriceInt == targetPrice)
                        {
                            newTargetPrice = true;
                        }

                        if (firstTime)
                        {
                            BuyOneStonk();
                            firstTime = false;
                        }

                        if ((GetPriceAndFee() < priceAvg * priceToBuy) ||
                           (GetPriceAndFee() < priceAvg - priceToBuyDollar))
                        {
                            BuyOneStonk();
                        }

                        if ((GetCurrentSellPriceAndFee() > priceAvg * priceToSell) ||
                            (GetCurrentSellPriceAndFee() > priceAvg + priceToSellDollar))
                        {
                            SellOneStonk();
                        }

                        if (saveMoney)
                        {
                            if (money > saveMoneyMore)
                            {
                                var saved = money - saveMoneyMore;
                                moneySaved += saved;
                                money = saveMoneyMore;

                                Console.WriteLine($"Отложил {saved}$, всего {moneySaved}$, осталось {money}$");
                            }
                        }

                        bool BuyOneStonk()
                        {
                            if (money > GetPriceAndFee())
                            {
                                money -= GetPriceAndFee();

                                allFee += GetFee();

                                priceAvg = (stonks * priceAvg + currentPrice) / (stonks + 1);
                                stonks++;

                                Console.WriteLine($"Купил акцию за {currentPrice:N2}$ + {currentPrice * moneyFee:N2}$, всего {stonks}, средняя {priceAvg:N2}$, осталось {money}$");

                                return true;
                            }

                            return false;
                        }

                        bool SellOneStonk()
                        {
                            if (stonks > 0)
                            {
                                money += GetCurrentSellPriceAndFee();

                                allFee += GetSellFee();
                                stonks--;

                                Console.WriteLine($"Продал акцию за {GetCurrentSellPrice():N2}$ + {GetSellFee():N2}$, всего {stonks}, средняя {priceAvg:N2}$, осталось {money}$");

                                if (stonks == 0)
                                {
                                    firstTime = true;
                                }
                                return true;
                            }

                            return false;
                        }










                        dateTime = dateTime.AddMinutes(1);
                    }

                }

                double GetCurrentSellPriceAndFee()
                {
                    return GetCurrentSellPrice() + GetSellFee();
                }

                double GetSellFee()
                {
                    return GetCurrentSellPrice() * moneyFee;
                }

                double GetCurrentSellPrice()
                {
                    return currentPrice - Math.Max(currentPrice * spreadDiff, spreadDiffDollar);
                }

                double GetPriceAndFee()
                {
                    return currentPrice + GetFee();
                }

                double GetFee()
                {
                    return currentPrice * moneyFee;
                }

            }
            Console.ReadKey();
        }
    }

}