﻿/*
* QUANTCONNECT.COM - Portfolio Manager
* Wrapper around securities list to give easy access to company objects Portfolio["IBM"].Holdings
*/

/**********************************************************
* USING NAMESPACES
**********************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using QuantConnect.Logging;
using QuantConnect.Models;
using QuantConnect.Securities;

namespace QuantConnect.Securities {

    /// <summary>
    /// Algorithm Portfolio Manager: Indexed by the vehicle symbol, portfolio object displaying core properties and holdings
    /// </summary>
    public class SecurityPortfolioManager : IDictionary<string, SecurityHolding> {

        /******************************************************** 
        * CLASS VARIABLES
        *********************************************************/
        //Security Manager for Portfolio Collection:
        /// <summary>
        /// Security Manager Collection
        /// </summary>
        public SecurityManager Securities;

        /// <summary>
        /// Security Transaction Methods
        /// </summary>
        public SecurityTransactionManager Transactions;
        
        //Record keeping variables
        private decimal _cash = 100000;
        private decimal _lastTradeProfit = 0;
        private decimal _profit = 0;

        /******************************************************** 
        * CLASS CONSTRUCTOR
        *********************************************************/
        /// <summary>
        /// Initialise Security Portfolio Manager
        /// </summary>
        public SecurityPortfolioManager(SecurityManager securityManager, SecurityTransactionManager transactions) {
            this.Securities = securityManager;
            this.Transactions = transactions;
        }

        /******************************************************** 
        * DICTIONARY IMPLEMENTATION
        *********************************************************/
        /// <summary>
        /// Portfolio Manager Dictionary Implementation Placeholder - Not Implemented.
        /// </summary>
        /// <param name="symbol">Symbol of dictionary</param>
        /// <param name="holding">SecurityHoldings object</param>
        public void Add(string symbol, SecurityHolding holding) { throw new Exception("Portfolio object is an adaptor for Security Manager. To add a new asset add the required data during initialization."); }

        /// <summary>
        /// Portfolio Manager Dictionary Implementation Placeholder - Not Implemented.
        /// </summary>
        /// <param name="pair">Key value pair of dictionary</param>
        public void Add(KeyValuePair<string, SecurityHolding> pair) { throw new Exception("Portfolio object is an adaptor for Security Manager. To add a new asset add the required data during initialization."); }

        /// <summary>
        /// Portfolio Manager Dictionary Implementation Placeholder - Not Implemented.
        /// </summary>
        public void Clear() { throw new Exception("Portfolio object is an adaptor for Security Manager and cannot be cleared."); }

        /// <summary>
        /// Portfolio Manager Dictionary Implementation Placeholder - Not Implemented.
        /// </summary>
        /// <param name="pair">Key value pair of dictionary</param>
        public bool Remove(KeyValuePair<string, SecurityHolding> pair) { throw new Exception("Portfolio object is an adaptor for Security Manager and objects cannot be removed."); }

        /// <summary>
        /// Portfolio Manager Dictionary Implementation Placeholder - Not Implemented.
        /// </summary>
        /// <param name="symbol">Symbol of dictionary</param>
        public bool Remove(string symbol) { throw new Exception("Portfolio object is an adaptor for Security Manager and objects cannot be removed."); }

        /// <summary>
        /// Dictionary Interface Implementation: Contains security symbol
        /// </summary>
        /// <param name="symbol">Symbol we're searching for</param>
        /// <returns>true if contains this key</returns>
        public bool ContainsKey(string symbol)
        {
            return Securities.ContainsKey(symbol);
        }

        /// <summary>
        /// Dictionary Interface Implementation: Contains this keyvalue pair
        /// </summary>
        /// <param name="pair">Pair we're searching for</param>
        /// <returns>True if we have this object</returns>
        public bool Contains(KeyValuePair<string, SecurityHolding> pair)
        {
            return Securities.ContainsKey(pair.Key);
        }

        /// <summary>
        /// Dictionary Interface Implementation: Count of securities in this portfolio
        /// </summary>
        public int Count
        {
            get { return Securities.Count; }
        }

        /// <summary>
        /// Dictionary Interface Implementation: Is the object read only?
        /// </summary>
        public bool IsReadOnly
        {
            get { return Securities.IsReadOnly; }
        }

        /// <summary>
        /// Dictionary Interface Implementation: Copy contents to array:
        /// </summary>
        /// <param name="array"></param>
        /// <param name="index"></param>
        public void CopyTo(KeyValuePair<string, SecurityHolding>[] array, int index)
        {
            array = new KeyValuePair<string, SecurityHolding>[Securities.Count];
            int i = 0;
            foreach (KeyValuePair<string, Security> asset in Securities)
            {
                if (i >= index)
                {
                    array[i] = new KeyValuePair<string, SecurityHolding>(asset.Key, asset.Value.Holdings);
                }
                i++;
            }
        }

        /// <summary>
        /// Dictionary Implementation of the Keys in Portfolio
        /// </summary>
        public ICollection<string> Keys
        {
            get { return Securities.Keys; }
        }

        /// <summary>
        /// Dictionary Implementation of Values objects in portfolio:
        /// </summary>
        public ICollection<SecurityHolding> Values
        {
            get
            {
                return (from asset in Securities.Values
                        select asset.Holdings).ToList();
            }
        }

        /// <summary>
        /// Dictionary Interface Implementation: Try get the value with the string:
        /// </summary>
        /// <param name="symbol">Symbol we're looking for</param>
        /// <param name="holding">Holdings object of this security</param>
        /// <returns>True if successful</returns>
        public bool TryGetValue(string symbol, out SecurityHolding holding)
        {
            if (Securities.ContainsKey(symbol))
            {
                holding = Securities[symbol].Holdings;
                return true;
            }
            else
            {
                holding = null;
                return false;
            }
        }

        /// <summary>
        /// Dictionary Interface Implementation: Get the Enumerator for this object
        /// </summary>
        /// <returns>Enumerable key value pair</returns>
        IEnumerator<KeyValuePair<string, SecurityHolding>> IEnumerable<KeyValuePair<string, SecurityHolding>>.GetEnumerator()
        {
            return Securities.GetInternalPortfolioCollection().GetEnumerator();
        }

        /// <summary>
        /// Dictionary Interface Implementation: Get the enumerator for this object:
        /// </summary>
        /// <returns>Enumerator.</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return Securities.GetInternalPortfolioCollection().GetEnumerator();
        }

        /******************************************************** 
        * CLASS PROPERTIES
        *********************************************************/
        /// <summary>
        /// Cash allocated to this company, from which we can find the buying power available.
        /// When Equity turns profit available cash increases, generating a positive feed back 
        /// for successful Security.
        /// </summary>
        public decimal Cash 
        {
            get
            {
                return _cash;
            }
        }

        /// <summary>
        /// Sum the individual asset holdings
        /// </summary>
        public decimal TotalUnleveredAbsoluteHoldingsCost 
        {
            get 
            {
                //Sum sum of holdings
                return (from position in this.Securities.Values
                        select position.Holdings.AbsoluteHoldingsCost / position.Leverage).Sum();
            }
        }


        /// <summary>
        /// Holdings Market Value Sum
        /// </summary>
        public decimal TotalHoldingsValue
        {
            get
            {
                //Sum sum of holdings
                return (from position in this.Securities.Values
                        select position.Holdings.AbsoluteHoldingsValue).Sum();
            }
        }

        /// <summary>
        /// Check if we have holdings:
        /// </summary>
        public bool HoldStock 
        {
            get {
                if (TotalHoldingsValue > 0) 
                {
                    return true;
                } 
                else 
                {
                    return false;
                }
            }
        }


        /// <summary>
        /// Alias for HoldStock. Check if we have and holdings.
        /// </summary>
        public bool Invested 
        {
            get 
            {
                return HoldStock;
            }
        }

        /// <summary>
        /// Get the total unrealised profit in our portfolio
        /// </summary>
        public decimal TotalUnrealisedProfit 
        {
            get 
            {
                return (from position in Securities.Values
                               select position.Holdings.UnrealizedProfit).Sum();
            }
        }


        /// <summary>
        /// Added a wrapper for American spelling.
        /// </summary>
        public decimal TotalUnrealizedProfit 
        {
            get 
            {
                return TotalUnrealisedProfit;
            }
        }

        /// <summary>
        /// Total portfolio value if we sold all holdings:
        /// </summary>
        public decimal TotalPortfolioValue 
        {
            get 
            {
                return Cash + TotalUnrealisedProfit + TotalUnleveredAbsoluteHoldingsCost;
            }
        }

        /// <summary>
        /// Total Order Fees across all Securities.
        /// </summary>
        public decimal TotalFees 
        {
            get 
            {
                return (from position in Securities.Values
                        select position.Holdings.TotalFees).Sum();
            }
        }

        /// <summary>
        /// Total Profit across all Securities
        /// </summary>
        public decimal TotalProfit 
        {
            get 
            {
                return (from position in Securities.Values
                        select position.Holdings.Profit).Sum();
            }
        }

        /// <summary>
        /// Total Sale Volume Today
        /// </summary>
        public decimal TotalSaleVolume 
        {
            get 
            {
                return (from position in Securities.Values
                        select position.Holdings.TotalSaleVolume).Sum();
            }
        }

        /******************************************************** 
        * CLASS METHODS
        *********************************************************/
        /// <summary>
        /// Primary Iterator for Portfolio Class: 
        /// </summary>
        /// <param name="symbol">string Symbol indexer</param>
        /// <returns>MarketHolding Class from the Algorithm Securities</returns>
        public SecurityHolding this [string symbol] 
        {
            get 
            {
                return Securities[symbol].Holdings;
            }
            set 
            {
                Securities[symbol].Holdings = value;
            }
        }

        /// <summary>
        /// Set the cash this algorithm is to manage:
        /// </summary>
        /// <param name="cash">Decimal Cash</param>
        public void SetCash(decimal cash) 
        {
            this._cash = cash;
        }

        /// <summary>
        /// The total buying power remaining factoring in leverage.
        /// A Security affect on buying power = holdings / leverage.
        /// </summary>
        public decimal GetBuyingPower(string symbol, OrderDirection direction = OrderDirection.Hold) 
        {
            //Each asset has different leverage values, so affects our cash position in different ways.
            // Basically position affect on cash = holdings / leverage
            decimal holdings = (Securities[symbol].Holdings.AbsoluteQuantity * Securities[symbol].Close);
            decimal remainingBuyingPower = Cash * Securities[symbol].Leverage;

            if (direction == OrderDirection.Hold) return remainingBuyingPower;
            //Log.Debug("SecurityPortfolioManager.GetBuyingPower(): Direction: " + direction.ToString());

            if (Securities[symbol].Holdings.IsLong)
            {
                switch (direction)
                {
                    case OrderDirection.Buy:
                        return remainingBuyingPower;
                    case OrderDirection.Sell:
                        return holdings * 2  + remainingBuyingPower;
                }
            } 
            else if (Securities[symbol].Holdings.IsShort) 
            {
                switch (direction)
                {
                    case OrderDirection.Buy:
                        return holdings*2 + remainingBuyingPower;
                    case OrderDirection.Sell:
                        return remainingBuyingPower;
                }
            }

            //No holdings buying power is cash x leverage
            return remainingBuyingPower;
        }



        /// <summary>
        /// Calculate the new average price (if buying), and new quantity/profit if selling.
        /// </summary>
        public virtual void ProcessFill(Order order) 
        {
            //Get the required information from the vehicle this order will affect
            decimal feeThisOrder = 0;
            string symbol = order.Symbol;
            Security vehicle = Securities[symbol];
            bool isLong = vehicle.Holdings.IsLong;
            bool isShort = vehicle.Holdings.IsShort;
            bool closedPosition = false;
            //Make local decimals to avoid any rounding errors from int multiplication
            decimal quantityHoldings = (decimal)vehicle.Holdings.Quantity;
            decimal absoluteHoldingsQuantity = vehicle.Holdings.AbsoluteQuantity;
            decimal averageHoldingsPrice = vehicle.Holdings.AveragePrice;
            decimal leverage = vehicle.Leverage;

            try
            {
                //Add the transOrderDirection to the company Cache (plotting):
                Securities[symbol].Cache.AddOrder(order);
                //Update the Vehicle approximate total sales volume.
                vehicle.Holdings.AddNewSale(order.Price * Convert.ToDecimal(order.AbsoluteQuantity));

                //Get the Fee for this Order - Update the Portfolio Cash Balance: Remove Transacion Fees.
                feeThisOrder = Math.Abs(Securities[symbol].Model.GetOrderFee(order.AbsoluteQuantity, order.Price));
                vehicle.Holdings.AddNewFee(feeThisOrder);
                _cash -= feeThisOrder;

                
                //Calculate & Update the Last Trade Profit
                if (isLong && order.Direction == OrderDirection.Sell) 
                {
                    //Closing up a long position
                    if (quantityHoldings >= order.AbsoluteQuantity) 
                    {
                        //Closing up towards Zero.
                        _lastTradeProfit = (order.Price - averageHoldingsPrice) * order.AbsoluteQuantity;
                        
                        //New cash += profitLoss + costOfAsset/leverage.
                        _cash += _lastTradeProfit + ( (averageHoldingsPrice * order.AbsoluteQuantity) / leverage );
                    } 
                    else 
                    {
                        //Closing up to Neg/Short Position (selling more than we have) - Only calc profit on the stock we have to sell.
                        _lastTradeProfit = (order.Price - averageHoldingsPrice) * quantityHoldings;

                        //New cash += profitLoss + costOfAsset/leverage.
                        _cash += _lastTradeProfit + ((averageHoldingsPrice * quantityHoldings) / leverage);
                    }
                    closedPosition = true;
                }
                else if (isShort && order.Direction == OrderDirection.Buy)
                {
                    //Closing up a short position.
                    if (absoluteHoldingsQuantity >= order.Quantity) 
                    {
                        //Reducing the stock we have, and enough stock on hand to process order.
                        _lastTradeProfit = (averageHoldingsPrice - order.Price) * order.AbsoluteQuantity;

                        //New cash += profitLoss + costOfAsset/leverage.
                        _cash += _lastTradeProfit + ((averageHoldingsPrice * order.AbsoluteQuantity) / leverage);
                    }
                    else 
                    {
                        //Increasing stock holdings, short to positive through zero, but only calc profit on stock we Buy.
                        _lastTradeProfit = (averageHoldingsPrice - order.Price) * absoluteHoldingsQuantity;

                        //New cash += profitLoss + costOfAsset/leverage.
                        _cash += _lastTradeProfit + ((averageHoldingsPrice * absoluteHoldingsQuantity) / leverage);
                    }
                    closedPosition = true;
                }


                if (closedPosition)
                {
                    //Update Vehicle Profit Tracking:
                    _profit += _lastTradeProfit;
                    vehicle.Holdings.AddNewProfit(_lastTradeProfit);
                    vehicle.Holdings.SetLastTradeProfit(_lastTradeProfit);
                    AddTransactionRecord(vehicle.Time, _lastTradeProfit - 2 * feeThisOrder);
                }


                //UPDATE HOLDINGS QUANTITY, AVG PRICE:
                //Currently NO holdings. The order is ALL our holdings.
                if (quantityHoldings == 0) 
                {
                    //First transaction just subtract order from cash and set our holdings:
                    averageHoldingsPrice = order.Price;
                    quantityHoldings = order.Quantity;
                    _cash -= (order.Price * Convert.ToDecimal(order.AbsoluteQuantity)) / leverage;
                }
                else if (isLong) 
                {
                    //If we're currently LONG on the stock.
                    switch (order.Direction) 
                    {
                        case OrderDirection.Buy:
                            //Update the Holding Average Price: Total Value / Total Quantity:
                            averageHoldingsPrice = ((averageHoldingsPrice * quantityHoldings) + (order.Quantity * order.Price)) / (quantityHoldings + (decimal)order.Quantity);
                            //Add the new quantity:
                            quantityHoldings += order.Quantity;
                            //Subtract this order from cash:
                            _cash -= (order.Price * Convert.ToDecimal(order.AbsoluteQuantity)) / leverage;
                            break;

                        case OrderDirection.Sell:
                            quantityHoldings += order.Quantity; //+ a short = a subtraction
                            if (quantityHoldings < 0) 
                            {
                                //If we've now passed through zero from selling stock: new avg price:
                                averageHoldingsPrice = order.Price;
                                _cash -= (order.Price * Math.Abs(quantityHoldings)) / leverage;
                            }
                            else if (quantityHoldings == 0) 
                            {
                                averageHoldingsPrice = 0;
                            }
                            break;
                    }
                } 
                else if (isShort) 
                {
                    //We're currently SHORTING the stock: What is the new position now?
                    switch (order.Direction) 
                    {
                        case OrderDirection.Buy:
                            //Buying when we're shorting moves to close position:
                            quantityHoldings += order.Quantity;
                            if (quantityHoldings > 0) 
                            {
                                //If we were short but passed through zero, new average price is what we paid. The short position was closed.
                                averageHoldingsPrice = order.Price;
                                _cash -= (order.Price * Math.Abs(quantityHoldings)) / leverage;
                            }
                            else if (quantityHoldings == 0) 
                            {
                                averageHoldingsPrice = 0;
                            }
                            break;

                        case OrderDirection.Sell:
                            //We are increasing a Short position:
                            //E.g.  -100 @ $5, adding -100 @ $10: Avg: $7.5
                            //      dAvg = (-500 + -1000) / -200 = 7.5
                            averageHoldingsPrice = ((averageHoldingsPrice * quantityHoldings) + (Convert.ToDecimal(order.Quantity) * order.Price)) / (quantityHoldings + (decimal)order.Quantity);
                            quantityHoldings += order.Quantity;
                            _cash -= (order.Price * Convert.ToDecimal(order.AbsoluteQuantity)) / leverage;
                            break;
                    }
                }
            } 
            catch( Exception err )
            {
                Log.Error("SecurityPortfolioManager.ProcessFill(): " + err.Message);
            }
            
            //Set the results back to the vehicle.
            vehicle.Holdings.SetHoldings(averageHoldingsPrice, Convert.ToInt32(quantityHoldings));
        } // End Process Fill


        /// <summary>
        /// Scan the portfolio and the updated data for a potential margin call situation. 
        /// If there is a margin call, place the order, process it and 
        /// </summary>
        /// <returns></returns>
        public bool ScanForMarginCall()
        {
            return false;
        }


        /// <summary>
        /// Bit of a hack -- but using datetime as dictionary key is dangerous as you can process multiple orders within a second.
        /// For the accounting / statistics generating purposes its not really critical anyway, so just add a millisecond while there's an identical key.
        /// </summary>
        /// <param name="time">Time of order processed </param>
        /// <param name="transactionProfitLoss">Profit Loss.</param>
        private void AddTransactionRecord(DateTime time, decimal transactionProfitLoss)
        {
            DateTime clone = time;

            while (Transactions.TransactionRecord.ContainsKey(clone))
            {
                clone = clone.AddMilliseconds(1);
            }

            Transactions.TransactionRecord.Add(clone, transactionProfitLoss);
        }


    } //End Algorithm Portfolio Class


} // End QC Namespace
