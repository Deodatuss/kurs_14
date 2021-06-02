using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;

namespace Kurs_14_Taksopark
{
    interface IAccount
    {
        string GetName { get; }
        string GetSurname { get; }
        string GetRegistrationDate { get; }//return in "DD-MM-YYYY"
        string GetEmail { get; }
        string GetLogin { get; }

        void UpdatePassword(string VERIFY_PASSWORD, string NEW_PASSWORD);
    }
}
