using System;
using System.Collections.Generic;
using System.Text;

namespace Kurs_14_Taksopark
{
    public class DB_names
    {
        public DB_names(string USER_ACCOUNTS, string DRIVER_ACCOUNTS, string LOCATIONS, string USER_ORDERS, string FINANCE_CONSTANTS, string TRANSACTIONS)
        {
            this.USER_ACCOUNTS = USER_ACCOUNTS;
            this.DRIVER_ACCOUNTS = DRIVER_ACCOUNTS;
            this.LOCATIONS = LOCATIONS;
            this.USER_ORDERS = USER_ORDERS;
            this.FINANCE_CONSTANTS = FINANCE_CONSTANTS;
            this.TRANSACTIONS = TRANSACTIONS;

        }
        public readonly string USER_ACCOUNTS;
        public readonly string DRIVER_ACCOUNTS;
        public readonly string LOCATIONS;
        public readonly string USER_ORDERS;
        public readonly string FINANCE_CONSTANTS;
        public readonly string TRANSACTIONS;
    }
}
