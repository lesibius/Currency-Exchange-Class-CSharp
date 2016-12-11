using System;
using System.Collections.Generic;

namespace CurrencyConverter
{
    interface ICurrencyConverter
    {
        //-----------------Properties--------------------------


        /********************************************************************************
        *                           Properties                                          *
        ********************************************************************************/

        /// <summary>
        /// Use this readonly property to check the actual date for the rates
        /// </summary>
        DateTime Date { get; }

        /// <summary>
        /// Get or set the base currency
        /// </summary>
        string BaseCurrency { get; set; } 

        /********************************************************************************
        *                                   Methods                                     *
        ********************************************************************************/

        /// <summary>
        /// Exchanges the given amount from one currency to the other
        /// </summary>
        /// <param name="amount">The amount to be exchanged</param>
        /// <param name="from">Currency of the amount (three letter code)</param>
        /// <param name="to">Currency to witch we wish to exchange. Base currency if not specified.</param>
        /// <returns>the exchanged amount on success</returns>
        decimal Exchange(decimal amount, string from, string to = null);


        /// <summary>
        /// Gets the cross rate between two currencies
        /// </summary>
        /// <param name="from">String from first Currency (three letter code)</param>
        /// <param name="to">String to second Currency (three letter code). Base currency if not specified.</param>
        /// <returns>the cross rate on success</returns>
        decimal CrossRate(string from, string to = null);

        /// <summary>
        /// Gets the rates table based on Base currency
        /// </summary>
        /// <param name="currencyList">list of comma separated Currencies to be included in the table. All currencies by default</param>
        /// <returns>IEnumerable<Rates> containing desired currencies and rates</returns>
        IEnumerable<Rates> GetRatesTable(string currencyList = null);

        /// <summary>
        /// Gets the list of currencies.
        /// </summary>
        /// <param name="sorted">True to sort the list</param>
        /// <returns>IEnumerable<string> of all available currencies</returns>
        IEnumerable<string> GetCurrencyList(bool sorted = false);
    }
}
