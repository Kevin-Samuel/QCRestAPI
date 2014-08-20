﻿using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using QuantConnect.Securities;
using QuantConnect.Models;

namespace QuantConnect
{
    /// <summary>
    /// 3.0 CUSTOM DATA SOURCE: USE YOUR OWN MARKET DATA (OPTIONS, FOREX, FUTURES, DERIVATIVES etc).
    /// 
    /// The new QuantConnect Lean Backtesting Engine is incredibly flexible and allows you to define your own data source. 
    /// 
    /// This includes any data source which has a TIME and VALUE. These are the *only* requirements. To demonstrate this we're loading
    /// in "Weather" data. This by itself isn't special, the cool part is next:
    /// 
    /// We load the "Weather" data as a tradable security we're calling "NYCTEMP".
    /// 
    /// </summary>
    public class CustomDataSourceAlgorithm : QCAlgorithm
    {
        public override void Initialize()
        {
            //Weather data we have is within these days:
            SetStartDate(2011, 9, 13);
            SetEndDate(DateTime.Now.Date.AddDays(-1));

            //Set the cash for the strategy:
            SetCash(100000);

            //Define the symbol and "type" of our generic data:
            AddData<Bitcoin>("BTC");
        }

        /// <summary>
        /// Event Handler for Bitcoin Data Events: These weather objects are created from our 
        /// "Weather" type below and fired into this event handler.
        /// </summary>
        /// <param name="data">One(1) Weather Object, streamed into our algorithm synchronised in time with our other data streams</param>
        public void OnData(Bitcoin data)
        {
            //If we don't have any weather "SHARES" -- invest"
            if (!Portfolio.Invested)
            {
                //Weather used as a tradable asset, like stocks, futures etc. 
                if (data.Close != 0)
                {
                    Order("BTC", (Portfolio.Cash / Math.Abs(data.Close + 1)));
                }
                Console.WriteLine("Buying BTC 'Shares': BTC: " + data.Close);
            }
            Console.WriteLine("Time: " + Time.ToLongDateString() + " " + Time.ToLongTimeString() + data.Close.ToString());
        }
    }


    /// <summary>
    /// Custom Data Type: Bitcoin data from Quandl.
    /// http://www.quandl.com/help/api-for-bitcoin-data
    /// </summary>
    public class Bitcoin : BaseData
    {
        //Set the defaults:
        public decimal Open = 0;
        public decimal High = 0;
        public decimal Low = 0;
        public decimal Close = 0;
        public decimal VolumeBTC = 0;
        public decimal VolumeUSD = 0;
        public decimal WeightedPrice = 0;

        /// <summary>
        /// 1. DEFAULT CONSTRUCTOR: Custom data types need a default constructor.
        /// We search for a default constructor so please provide one here. It won't be used for data, just to generate the "Factory".
        /// </summary>
        public Bitcoin()
        {
            this.Symbol = "BTC";
        }

        /// <summary>
        /// 2. RETURN THE STRING URL SOURCE LOCATION FOR YOUR DATA:
        /// This is a powerful and dynamic select source file method. If you have a large dataset, 10+mb we recommend you break it into smaller files. E.g. One zip per year.
        /// We can accept raw text or ZIP files. We read the file extension to determine if it is a zip file.
        /// </summary>
        /// <param name="config">Subscription data, symbol name, data type</param>
        /// <param name="date">Current date we're requesting. This allows you to break up the data source into daily files.</param>
        /// <param name="datafeed">Datafeed type: Backtesting or the Live data broker who will provide live data. You can specify a different source for live trading! </param>
        /// <returns>string URL end point.</returns>
        public override string GetSource(SubscriptionDataConfig config, DateTime date, DataFeedEndpoint datafeed)
        {
            switch (datafeed)
            {
                //Backtesting Data Source: Example of a data source which varies by day (commented out)
                default:
                case DataFeedEndpoint.Backtesting:
                    //return "http://my-ftp-server.com/futures-data-" + date.ToString("Ymd") + ".zip";
                    // OR simply return a fixed small data file. Large files will slow down your backtest
                    return "http://www.quandl.com/api/v1/datasets/BITCOIN/BITSTAMPUSD.csv?sort_order=asc";

                case DataFeedEndpoint.LiveTrading:
                    //Alternative live socket data source for live trading (soon)/
                    return "....";
            }
        }

        /// <summary>
        /// 3. READER METHOD: Read 1 line from data source and convert it into Object.
        /// Each line of the CSV File is presented in here. The backend downloads your file, loads it into memory and then line by line
        /// feeds it into your algorithm
        /// </summary>
        /// <param name="line">string line from the data source file submitted above</param>
        /// <param name="config">Subscription data, symbol name, data type</param>
        /// <param name="date">Current date we're requesting. This allows you to break up the data source into daily files.</param>
        /// <param name="datafeed">Datafeed type - Backtesting or LiveTrading</param>
        /// <returns>New Weather Object which extends BaseData.</returns>
        public override BaseData Reader(SubscriptionDataConfig config, string line, DateTime date, DataFeedEndpoint datafeed)
        {
            //New weather object
            Bitcoin coin = new Bitcoin();

            try
            {
                //Example File Format:
                //Date,      Open   High    Low     Close   Volume (BTC)    Volume (Currency)   Weighted Price
                //2011-09-13 5.8    6.0     5.65    5.97    58.37138238,    346.0973893944      5.929230648356
                string[] data = line.Split(',');
                coin.Time = DateTime.Parse(data[0]);
                coin.Open = Convert.ToDecimal(data[1]);
                coin.High = Convert.ToDecimal(data[2]);
                coin.Low = Convert.ToDecimal(data[3]);
                coin.Close = Convert.ToDecimal(data[4]);
                coin.VolumeBTC = Convert.ToDecimal(data[5]);
                coin.VolumeUSD = Convert.ToDecimal(data[6]);
                coin.WeightedPrice = Convert.ToDecimal(data[7]);
                coin.Symbol = "BTC";
                coin.Value = coin.Close;
            }
            catch { /* Do nothing, skip first title row */ }

            return coin;
        }
    }
}