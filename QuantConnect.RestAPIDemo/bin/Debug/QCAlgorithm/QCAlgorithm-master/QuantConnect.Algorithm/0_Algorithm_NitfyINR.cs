﻿using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using QuantConnect.Securities;
using QuantConnect.Models;
using System.Linq;
using MathNet.Numerics.Statistics;

namespace QuantConnect
{
    /// <summary>
    /// 3.0 CUSTOM DATA SOURCE: USE YOUR OWN MARKET DATA (OPTIONS, FOREX, FUTURES, DERIVATIVES etc).
    /// 
    /// The new QuantConnect Lean Backtesting Engine is incredibly flexible and allows you to define your own data source. 
    /// 
    /// This includes any data source which has a TIME and VALUE. These are the *only* requirements. To demonstrate this we're loading
    /// in "Nifty" data. This by itself isn't special, the cool part is next:
    /// 
    /// We load the "Nifty" data as a tradable security we're calling "NIFTY".
    /// 
    /// </summary>
    public class CustomDataSourceAlgorithm : QCAlgorithm
    {
        //Create variables for analyzing Nifty
        CorrelationPair today = new CorrelationPair();
        List<CorrelationPair> prices = new List<CorrelationPair>();
        int minimumCorrelationHistory = 50;

        public override void Initialize()
        {
            SetStartDate(2008, 1, 8);
            SetEndDate(2014, 7, 25);

            //Set the cash for the strategy:
            SetCash(100000);

            //Define the symbol and "type" of our generic data:
            AddData<DollarRupee>("USDINR");
            AddData<Nifty>("NIFTY");
        }

        /// <summary>
        /// Event Handler for Nifty Data Events: These Nifty objects are created from our 
        /// "Nifty" type below and fired into this event handler.
        /// </summary>
        /// <param name="data">One(1) Nifty Object, streamed into our algorithm synchronised in time with our other data streams</param>
        public void OnData(DollarRupee data)
        {
            today = new CorrelationPair(data.Time);
            today.CurrencyPrice = Convert.ToDouble(data.Close);
        }

        public void OnData(Nifty data)
        {
            try
            {
                int quantity = (int)(Portfolio.TotalPortfolioValue * 0.9m / data.Close);

                today.NiftyPrice = Convert.ToDouble(data.Close);
                if (today.Date == data.Time)
                {
                    prices.Add(today);

                    if (prices.Count > minimumCorrelationHistory)
                    {
                        prices.RemoveAt(0);
                    }
                }

                //Strategy
                double highestNifty = (from pair in prices select pair.NiftyPrice).Max();
                double lowestNifty = (from pair in prices select pair.NiftyPrice).Min();
                if (Time.DayOfWeek == DayOfWeek.Wednesday) //prices.Count >= minimumCorrelationHistory && 
                {
                    //List<double> niftyPrices = (from pair in prices select pair.NiftyPrice).ToList();
                    //List<double> currencyPrices = (from pair in prices select pair.CurrencyPrice).ToList();
                    //double correlation = Correlation.Pearson(niftyPrices, currencyPrices);
                    //double niftyFraction = (correlation)/2;

                    if (Convert.ToDouble(data.Open) >= highestNifty)
                    {
                        int code = Order("NIFTY", quantity - Portfolio["NIFTY"].Quantity);
                        Debug("LONG " + code + " Time: " + Time.ToShortDateString() + " Quantity: " + quantity + " Portfolio:" + Portfolio["NIFTY"].Quantity + " Nifty: " + data.Close + " Buying Power: " + Portfolio.TotalPortfolioValue);
                    }
                    else if (Convert.ToDouble(data.Open) <= lowestNifty)
                    {
                        int code = Order("NIFTY", -quantity - Portfolio["NIFTY"].Quantity);
                        Debug("SHORT " + code + " Time: " + Time.ToShortDateString() + " Quantity: " + quantity + " Portfolio:" + Portfolio["NIFTY"].Quantity + " Nifty: " + data.Close + " Buying Power: " + Portfolio.TotalPortfolioValue);
                    }
                }
            }
            catch (Exception err)
            {
                Debug("Error: " + err.Message);
            }
        }


        //Plot Nifty
        public override void OnEndOfDay()
        {
            //if(niftyData != null)
            {
                Plot("Nifty Closing Price", today.NiftyPrice);
            }
        }
    }

    public class Nifty : BaseData
    {
        public decimal Open = 0;
        public decimal High = 0;
        public decimal Low = 0;
        public decimal Close = 0;

        public Nifty()
        {
            this.Symbol = "NIFTY";
        }

        public override string GetSource(SubscriptionDataConfig config, DateTime date, DataFeedEndpoint datafeed)
        {
            return "https://www.dropbox.com/s/rsmg44jr6wexn2h/CNXNIFTY.csv?dl=1";
        }

        public override BaseData Reader(SubscriptionDataConfig config, string line, DateTime date, DataFeedEndpoint datafeed)
        {
            //New Nifty object
            Nifty index = new Nifty();

            try
            {
                //Example File Format:
                //Date,       Open       High        Low       Close     Volume      Turnover
                //2011-09-13  7792.9    7799.9     7722.65    7748.7    116534670    6107.78
                string[] data = line.Split(',');
                index.Time = DateTime.Parse(data[0]);
                index.Open = Convert.ToDecimal(data[1]);
                index.High = Convert.ToDecimal(data[2]);
                index.Low = Convert.ToDecimal(data[3]);
                index.Close = Convert.ToDecimal(data[4]);
                index.Symbol = "NIFTY";
                index.Value = index.Close;
            }
            catch
            {

            }

            return index;
        }
    }


    public class DollarRupee : BaseData
    {
        public decimal Open = 0;
        public decimal High = 0;
        public decimal Low = 0;
        public decimal Close = 0;

        public DollarRupee()
        {
            this.Symbol = "USDINR";
        }

        public override string GetSource(SubscriptionDataConfig config, DateTime date, DataFeedEndpoint datafeed)
        {
            return "https://www.dropbox.com/s/m6ecmkg9aijwzy2/USDINR.csv?dl=1";
        }

        public override BaseData Reader(SubscriptionDataConfig config, string line, DateTime date, DataFeedEndpoint datafeed)
        {
            //New USDINR object
            DollarRupee currency = new DollarRupee();

            try
            {
                string[] data = line.Split(',');
                currency.Time = DateTime.Parse(data[0]);
                currency.Close = Convert.ToDecimal(data[1]);
                currency.Symbol = "USDINR";
                currency.Value = currency.Close;
            }
            catch
            {

            }

            return currency;
        }
    }

    public class CorrelationPair
    {
        public DateTime Date = new DateTime();
        public double NiftyPrice = 0;
        public double CurrencyPrice = 0;

        public CorrelationPair()
        {

        }

        public CorrelationPair(DateTime date)
        {
            Date = date.Date;
        }

    }


}