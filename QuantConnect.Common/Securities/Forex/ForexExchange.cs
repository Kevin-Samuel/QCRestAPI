/*
* QUANTCONNECT.COM: FOREX Exchange
* FOREX Exchange Class - Track if the FX market is open
*/

/**********************************************************
 * USING NAMESPACES
 **********************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Diagnostics;

//QuantConnect Libraries:
using QuantConnect;
using QuantConnect.Logging;
using QuantConnect.Models;

namespace QuantConnect.Securities {

    /******************************************************** 
    * CLASS DEFINITIONS
    *********************************************************/

    /// <summary>
    /// Forex Exchange Class - Information and Helper Tools for Exchange Situation
    /// </summary>
    public class ForexExchange : SecurityExchange {

        /******************************************************** 
        * CLASS VARIABLES
        *********************************************************/


        /******************************************************** 
        * CLASS CONSTRUCTION
        *********************************************************/
        /// <summary>
        /// Initialise Forex Exchange Objects
        /// </summary>
        public ForexExchange() : 
            base() {
        }

        /******************************************************** 
        * CLASS PROPERTIES
        *********************************************************/
            


        /******************************************************** 
        * CLASS METHODS
        *********************************************************/
        /// <summary>
        /// Override the base ExchangeOpen property with FXCM Market Hours
        /// </summary>
        public override bool ExchangeOpen {
            get { return DateTimeIsOpen(Time); }
        }


        /// <summary>
        /// Number of trading days per year for this security, used for performance statistics.
        /// </summary>
        public override int TradingDaysPerYear
        {
            get
            {
                return 365;
            }
        }


        /// <summary>
        /// Check this date time is open for the forex market.
        /// </summary>
        /// <param name="dateToCheck">time of day</param>
        /// <returns>true if open</returns>
        public override bool DateTimeIsOpen(DateTime dateToCheck) {
            if (!DateIsOpen(dateToCheck))
                return false;

            if (dateToCheck.DayOfWeek == DayOfWeek.Friday && dateToCheck.TimeOfDay.TotalHours >= 16)
                return false;

            if (dateToCheck.DayOfWeek == DayOfWeek.Sunday && dateToCheck.TimeOfDay.TotalHours < 17)
                return false;

            return true;
        }


        /// <summary>
        /// Check if this datetime is open for the FXCM markets:
        /// </summary>
        /// <param name="dateToCheck">datetime current date</param>
        /// <returns>true if open</returns>
        public override bool DateIsOpen(DateTime dateToCheck)
        {
            if (dateToCheck.DayOfWeek == DayOfWeek.Saturday)
                return false;

            //Otherwise: True
            return true;
        }


    } //End of ForexExchange

} //End Namespace