using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Data.SqlClient;

namespace Kurs_14_Taksopark
{
    public class Driver : IAccount
    {
        public delegate void OperationState(string message);
        OperationState _del;
        public void LastOperationState(OperationState del)
        {
            _del = del;
        }
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;



        public Driver(DB_names TABLES, string NAME, string SURNAME, string EMAIL, string LOGIN, string PASSWORD, int DD_birth, int MM_birth, int YYYY_birth, string CAR)
        {
            bool isLoginFree = true;
            // checking if input login hasn't been used before
            string sqlExpressionCheck = String.Format("SELECT * FROM {0} WHERE Login = '{1}'", TABLES.DRIVER_ACCOUNTS, LOGIN);
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sqlExpressionCheck, connection);
                SqlDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    isLoginFree = false;
                    CREATION_STATUS = ($"FAILURE creating new driver {LOGIN}: driver {LOGIN} already exist in table {TABLES.DRIVER_ACCOUNTS}");
                    throw new InsufficientRightsException(CREATION_STATUS);
                }
            }
            if ((NAME != null && NAME != "") && (SURNAME != null && SURNAME != "") && (EMAIL != null && EMAIL != "") && (LOGIN != null && LOGIN != "") && 
                (PASSWORD != null && PASSWORD != "") && (CAR != null && CAR != "") && (0<DD_birth && DD_birth<32) && (0 < MM_birth && MM_birth < 13) && (YYYY_birth > 1900))
            {
                if (isLoginFree)
                {
                    this.NAME = NAME;
                    this.SURNAME = SURNAME;
                    this.EMAIL = EMAIL;
                    this.LOGIN = LOGIN;
                    this.PASSWORD = PASSWORD;
                    this.CAR = CAR;
                    this.BIRTH_DATE = String.Format("{0}-{1}-{2}", YYYY_birth, MM_birth, DD_birth);

                    string sqlExpression = String.Format("INSERT INTO {0} (Name, Surname, Email, Login, Password, Car, Birth_Date)" +
                        " VALUES ('{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}')", TABLES.DRIVER_ACCOUNTS, NAME, SURNAME, EMAIL, LOGIN, PASSWORD, CAR, BIRTH_DATE);

                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        SqlCommand command = new SqlCommand(sqlExpression, connection);
                        int number = command.ExecuteNonQuery();
                        CREATION_STATUS = ($"SUCCESS creating new driver {LOGIN} and adding him to table {TABLES.DRIVER_ACCOUNTS}");
                    }
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        sqlExpression = String.Format("SELECT * FROM {0} WHERE Login = '{1}'", TABLES.DRIVER_ACCOUNTS, LOGIN);
                        SqlCommand command = new SqlCommand(sqlExpression, connection);
                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            this.REGISTRATION_DATE = reader.GetValue(3).ToString();
                        }
                    }
                    DRIVERS_TABLE_NAME = TABLES.DRIVER_ACCOUNTS;
                    IS_REAL_AKK = true;
                }
            }
            else
            {
                CREATION_STATUS = ($"FAILURE creating new driver {LOGIN}: some of input credencials are empty, or date is incorrect");
                new ArgumentNullException(CREATION_STATUS);
            }
        }


        //login from existing acc, checking if login and password match
        //Can throw:
        //DataNotFoundException
        //InsufficientRightsException
        public Driver(DB_names TABLES, string LOGIN, string PASSWORD)
        {
            string sqlExpression = String.Format("SELECT * FROM {0} WHERE Login = '{1}' AND Password = '{2}'", TABLES.DRIVER_ACCOUNTS, LOGIN, PASSWORD);

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sqlExpression, connection);
                SqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    CREATION_STATUS = ($"SUCCESS login as driver {LOGIN}");

                    while (reader.Read()) // построчно считываем данные
                    {
                        this.NAME = (reader.GetValue(1)).ToString();
                        this.SURNAME = (reader.GetValue(2)).ToString();
                        this.REGISTRATION_DATE = (reader.GetValue(3)).ToString();
                        this.EMAIL = (reader.GetValue(4)).ToString();
                        this.LOGIN = (reader.GetValue(5)).ToString();
                        this.PASSWORD = (reader.GetValue(6)).ToString();
                        this.CAR = (reader.GetValue(7)).ToString();
                        this.BIRTH_DATE = (reader.GetValue(11)).ToString();
                        this.IS_AT_WORK_NOW = (reader.GetBoolean(13));
                        //    this.IS_AT_WORK_NOW = false;
                        //else
                        //    this.IS_AT_WORK_NOW = true;
                    }

                    DRIVERS_TABLE_NAME = TABLES.DRIVER_ACCOUNTS;
                    IS_REAL_AKK = true;
                }
                else
                {
                    CREATION_STATUS = ($"FAILURE login as driver {LOGIN}: incorrect login or password");
                    throw new DataNotFoundException(CREATION_STATUS);
                }
            }
        }

        public void Start_Working_Session()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sqlExpression = String.Format("UPDATE {0} SET IsAt_Work_Now = 'True' WHERE Login = '{1}'", DRIVERS_TABLE_NAME, LOGIN);
                SqlCommand command = new SqlCommand(sqlExpression, connection);
                int number = command.ExecuteNonQuery();
                _del?.Invoke($"SUCCESS starting working session for driver {LOGIN}");
                IS_AT_WORK_NOW = true;
            }
        }
        public void Stop_Working_Session()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sqlExpression = String.Format("UPDATE {0} SET IsAt_Work_Now = 'False' WHERE Login = '{1}'", DRIVERS_TABLE_NAME, LOGIN);
                SqlCommand command = new SqlCommand(sqlExpression, connection);
                int number = command.ExecuteNonQuery();
                _del?.Invoke($"SUCCESS stop working session for driver {LOGIN}");
                IS_AT_WORK_NOW = false;
            }
        }


        public Route_Container[] Fetch_Open_Orders(DB_names TABLES)
        {
            if (IS_AT_WORK_NOW)
            {
                Organize_Driver_Route Order = new Organize_Driver_Route(TABLES, this, _del);
                return Order.Get_All_Routes();
            }
            else {
                _del?.Invoke($"FAILURE fetching open requests from table {TABLES.USER_ORDERS}: start your working session first");
                return null; }
        }
        public void Book_Open_Order_by_User (DB_names TABLES, string user_login)
        {
            if (IS_AT_WORK_NOW)
            {
                Organize_Driver_Route Order = new Organize_Driver_Route(TABLES, this, _del);
            Order.Choose_User_Route_by_his_Login(TABLES, user_login, _del);
            }
            else {
                _del?.Invoke($"FAILURE booking request: start your working session first");
            }
        }
        //discard route (if driver wants to discard current route)
        public void Discard_Order(DB_names TABLES, string user_login)
        {
            if (IS_AT_WORK_NOW)
            {
                Organize_Driver_Route Order = new Organize_Driver_Route(TABLES, this, _del);
                Order.Discard_Route(TABLES, user_login, _del);
            }
            else
            {
                _del?.Invoke($"FAILURE discarding request: start your working session first");
            }
        }

        public Route_Container Fetch_My_Order(DB_names TABLES)
        {
            if (IS_AT_WORK_NOW)
            {
                Organize_Driver_Route Order = new Organize_Driver_Route(TABLES, this, _del);
                return Order.Get_My_Route(TABLES, _del);
            }
            else
            {
                _del?.Invoke($"FAILURE fetching your booked request: start your working session first");
                return null;
            }
        }
        public void UpdatePassword(string YOUR_PASSWORD, string NEW_PASSWORD) 
        {
            if (IS_REAL_AKK)
            {
                if (PASSWORD == YOUR_PASSWORD)
                {

                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        string sqlExpression = String.Format("UPDATE {0} SET Password = '{1}' WHERE Login = '{2}'", DRIVERS_TABLE_NAME, NEW_PASSWORD, LOGIN);
                        SqlCommand command = new SqlCommand(sqlExpression, connection);
                        int number = command.ExecuteNonQuery();
                        _del?.Invoke($"SUCCESS changing password for driver {LOGIN} in table {DRIVERS_TABLE_NAME}");
                        this.PASSWORD = NEW_PASSWORD;
                    }
                }
                else
                {
                    _del?.Invoke($"FAILURE changing password for driver {LOGIN}: invalid old password");
                    //new InsufficientRightsException();
                }
            }
            else
            {
                _del?.Invoke($"FAILURE changing password for driver {LOGIN}: such driver does not exist");
                //new DataNotFoundException();
            }
        }


        public string GetName
        {
            get { return NAME; }
        }
        public string GetSurname
        {
            get { return SURNAME; }
        }
        public string GetRegistrationDate
        {
            get { return REGISTRATION_DATE; }
        }
        public string GetEmail
        {
            get { return EMAIL; }
        }
        public string GetLogin
        {
            get { return LOGIN; }
        }
        public string GetCar
        {
            get { return CAR; }
        }
        public bool GetWorkingStatus
        {
            get { return IS_AT_WORK_NOW; }
        }
        public bool Verification_Status
        {
            get { return IS_REAL_AKK; }
        }



        private readonly string DRIVERS_TABLE_NAME = null;
        private readonly string NAME = null;
        private readonly string SURNAME = null;
        private readonly string REGISTRATION_DATE = null;
        private readonly string EMAIL = null;
        private readonly string LOGIN = null;
        private readonly string CAR = null;
        private readonly string BIRTH_DATE = null;

        private readonly string CREATION_STATUS = null;
        private readonly bool IS_REAL_AKK = false;

        private string PASSWORD = null;
        private bool IS_AT_WORK_NOW = false;
    }
}
