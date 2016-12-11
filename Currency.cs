﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Globalization;
using System.Xml;

namespace CurrencyConverter
{
 
    /// <summary>
    /// This class contains some functions to manipulate currencies.
    ///It gets information from the servers of European Central Bank.
    ///To get list of available currencies, please use GetCurrencyList() method, the return type is IEnumerable<string>.
    ///On construction the XML file is parsed, if something goes wrong Exeption will be thrown(WebException, FormatException or XmlException).
    ///Even if there is no connection to ECB servers, default value is created for BGN / EUR convertion (the rate is constant).
    /// </summary>
    public class Currency : ICurrencyConverter
    {

        /****************************************************************************************
        *                                   Constructor                                         *
        ****************************************************************************************/
        
        
        public Currency()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            try
            {
                xml = new XmlTextReader(sourceUrl); //tries to download XML file and create the Reader object
            }
            catch (WebException we) // if download is imposible, creates defalt value for BGN and EUR and throws an exception
            {
                this.baseCurrency = "EUR";
                this.exchangeRates.Add(this.baseCurrency, 1M);
                this.exchangeRates.Add("BGN", 1.9558M);
                this.date = DateTime.Now;
                throw new WebException("Error downloading XML, exchange rate created for BGN only, base currency EUR!", we);
            }
            try
            {
                while (xml.Read())
                {
                    if (xml.Name == "Cube")
                    {
                        if (xml.AttributeCount == 1)
                        {
                            xml.MoveToAttribute("time");
                            this.date = DateTime.Parse(xml.Value); // gets the date on which this rates are valid
                        }
                        if (xml.AttributeCount == 2)
                        {
                            xml.MoveToAttribute("currency");
                            this.currency = xml.Value;
                            xml.MoveToAttribute("rate");
                            try
                            {
                                this.rate = decimal.Parse(xml.Value);
                            }
                            catch (FormatException fe)
                            {
                                throw new FormatException("Urecognised format!", fe);
                            }
                            this.exchangeRates.Add(currency, rate); //ads currency and rate to exchange rate table
                        }
                        xml.MoveToNextAttribute();
                    }
                }
            }
            catch (XmlException xe)
            {
                throw new XmlException("Unable to parse Euro foreign exchange reference rates XML!", xe);
            }
            this.baseCurrency = "EUR"; // if XML parsed, add base currency
            this.exchangeRates.Add(this.baseCurrency, 1M);
        }

        /****************************************************************************************
        *                                   Variables                                           *
        ****************************************************************************************/


        private string sourceUrl = @"http://www.ecb.europa.eu/stats/eurofxref/eurofxref-daily.xml";
        private XmlTextReader xml;
        private string baseCurrency;
        private Dictionary<string, decimal> exchangeRates = new Dictionary<string,decimal>();
        private DateTime date = new DateTime();
        private string currency;
        private decimal rate;
        

        /****************************************************************************************
        *                                   Properties                                          *
        ****************************************************************************************/

        /// <summary>
        /// Provide the actual date for the rates from the ECB
        /// </summary>
        public DateTime Date
        {
            get
            {
                return this.date;
            }
        }


        /// <summary>
        /// Get or set base currency. EUR by default
        /// </summary>
        public string BaseCurrency
        {
            get
            {
                return this.baseCurrency;
            }
            set
            {
                if (value == null)
                {
                    value = "EUR";
                }
                value = value.ToUpper();
                value = value.Trim();
                CheckCurrency(value);
                this.baseCurrency = value;
                decimal factor = this.exchangeRates[this.baseCurrency];
                List<string> keys = new List<string>(this.exchangeRates.Keys);
                foreach (string key in keys)
                {
                    this.exchangeRates[key] /= factor; 
                }
            }
        }


        /****************************************************************************************
        *                                       Private Methods                                 *
        ****************************************************************************************/

        /// <summary>
        /// checks if currency is in currency list and throws exception if not
        /// </summary>
        /// <param name="currency">Currency to check</param>
        private void CheckCurrency(string currency)
        {
            if (!this.exchangeRates.ContainsKey(currency))
            {
                throw new ApplicationException("Unknown currency '" + currency + "', please use GetCurrencyList() to get list of available currencies!", new KeyNotFoundException());
            }
        }

        /****************************************************************************************
        *                                       Public Methods                                  *
        ****************************************************************************************/

        /// <summary>
        /// Override the <c>ToString</c> method
        /// </summary>
        /// <returns>A string representation of the <c>Currency</c> instance</returns>
        public override string ToString() // Converts Exchange Rate Table to String
        {
            StringBuilder str = new StringBuilder();
            str.Append("Reference rates of European Central Bank\nAll rates are for 1 " + this.baseCurrency + "\n\n");
            foreach (KeyValuePair<string, decimal> kvp in this.exchangeRates)
            {
                str.Append(String.Format("{0}{1,15:0.0000}\n", kvp.Key, kvp.Value));
            }
            return str.ToString();
        }

        
       /// <summary>
        /// Exchanges the givven amount from one currency to the other
        /// </summary>
        /// <param name="amount">The amount to be exchanged</param>
        /// <param name="from">Currency of the amount (three letter code)</param>
        /// <param name="to">Currency to witch we wish to exchange. Base currency if not specified.</param>
        /// <returns>the exchanged amount on success</returns>
        public decimal Exchange(decimal amount, string from, string to = null)
        {
            decimal result = 0M;
            if (to == null)
            {
                to = this.baseCurrency;
            }
            from = from.ToUpper().Trim();
            to = to.ToUpper().Trim();
            CheckCurrency(from);
            CheckCurrency(to);
            result = amount * this.exchangeRates[to] / this.exchangeRates[from];
            return result;
        }


        /// <summary>
        /// Gets the cross rate between two currencies
        /// </summary>
        /// <param name="from">first Currency (three letter code)</param>
        /// <param name="to">second Currency (three letter code). Base currency if not specified.</param>
        /// <returns>the cross rate on success</returns>
        public decimal CrossRate(string from, string to = null)
        {
            decimal result = 0M;
            if (to == null)
            {
                to = this.baseCurrency;
            }
            from = from.ToUpper().Trim();
            to = to.ToUpper().Trim();
            CheckCurrency(from);
            CheckCurrency(to);
            result = this.exchangeRates[to] / this.exchangeRates[from];
            return result;
        }


        /// <summary>
        /// Gets the rates table based on Base currency
        /// </summary>
        /// <param name="currencyList">list of comma separated Currencies to be included in the table. All currencies by default</param>
        /// <returns>IEnumerable<Rates> containing desired currencies and rates</returns>
        public IEnumerable<Rates> GetRatesTable(string currencyList = null)
        {
            List<Rates> result = new List<Rates>();
            Rates tempRate = new Rates();
            if (currencyList == null)
            {
                foreach (string currency in this.exchangeRates.Keys)
                {
                    tempRate.Currency = currency;
                    tempRate.Rate = String.Format("{0:0.0000}", this.exchangeRates[currency]);
                    result.Add(tempRate);
                }
            }
            else 
            {
                currencyList = currencyList.ToUpper();
                char[] delimiter = {',', ' ', ';'}; //just in case some one don't know what comma separated is
                string[] list = currencyList.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);
                foreach (string currency in list)
                {
                    currency.Trim();
                    CheckCurrency(currency);
                    tempRate.Currency = currency;
                    tempRate.Rate = String.Format("{0:0.0000}", this.exchangeRates[currency]);
                    result.Add(tempRate);
                }
            }
            return result;
        }


        /// <summary>
        /// Gets the list of currencies
        /// </summary>
        /// <param name="sorted">If sorted is true, the returned list is sorted. False by default</param>
        /// <returns>IEnumerable<string> of all available currencies </returns>
        public IEnumerable<string> GetCurrencyList(bool sorted = false)
        {
            List<string> currencyList = new List<string>(this.exchangeRates.Keys);
            if (sorted)
            {
                currencyList.Sort();
            } 
            return currencyList;
        }
    }
}