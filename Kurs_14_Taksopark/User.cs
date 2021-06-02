using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Data.SqlClient;

namespace Kurs_14_Taksopark
{
    public class User : IAccount
    {
        public delegate void OperationState(string message);
        OperationState _del;
        public void LastOperationState(OperationState del)
        {
            _del = del;
        }
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;



        //create new acc with credentials
        //Can throw:
        //ArgumentNullException
        //InsufficientRightsException
        public User(DB_names TABLES, string NAME, string SURNAME, string EMAIL, string LOGIN, string PASSWORD)
        {
            bool isLoginFree = true;
            
            //checking if input login hasn't been used before
            string sqlExpressionCheck = String.Format("SELECT * FROM {0} WHERE Login = '{1}'", TABLES.USER_ACCOUNTS, LOGIN);
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sqlExpressionCheck, connection);
                SqlDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    isLoginFree = false;
                    CREATION_STATUS = ($"FAILURE creating new user {LOGIN}: user {LOGIN} already exist in table {TABLES.USER_ACCOUNTS}");
                    throw new InsufficientRightsException(CREATION_STATUS);
                }
            }
            if ((NAME != null && NAME != "") && (SURNAME != null && SURNAME != "") && (EMAIL != null && EMAIL != "") && (LOGIN != null && LOGIN != "") && (PASSWORD != null && PASSWORD != ""))
            {
                if (isLoginFree)
                {
                    this.NAME = NAME;
                    this.SURNAME = SURNAME;
                    this.EMAIL = EMAIL;
                    this.LOGIN = LOGIN;
                    this.PASSWORD = PASSWORD;
                    this.Driven_KMs = 0;
                    
                    string sqlExpression = String.Format("INSERT INTO {0} (Name, Surname, Email, Login, Password)" +
                        " VALUES ('{1}', '{2}', '{3}', '{4}', '{5}')", TABLES.USER_ACCOUNTS, NAME, SURNAME, EMAIL, LOGIN, PASSWORD);

                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        SqlCommand command = new SqlCommand(sqlExpression, connection);
                        int number = command.ExecuteNonQuery();
                        CREATION_STATUS = ($"SUCCESS creating new user {LOGIN} and adding him to table {TABLES.USER_ACCOUNTS}");
                    }
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        sqlExpression = String.Format("SELECT * FROM {0} WHERE Login = '{1}'", TABLES.USER_ACCOUNTS, LOGIN);
                        SqlCommand command = new SqlCommand(sqlExpression, connection);
                        SqlDataReader reader = command.ExecuteReader();
                        while(reader.Read())
                        {
                            this.REGISTRATION_DATE = reader.GetValue(3).ToString();
                        }
                    }
                    USERS_TABLE_NAME = TABLES.USER_ACCOUNTS;
                    IS_REAL_AKK = true;
                }
            }
            else
            {
                CREATION_STATUS = ($"FAILURE creating new user {LOGIN}: some of input credencials are empty");
                throw new ArgumentNullException(CREATION_STATUS);
            }
        }



        //login from existing acc, checking if login and password match
        //Can throw:
        //DataNotFoundException
        //InsufficientRightsException
        public User(DB_names TABLES, string YOUR_LOGIN, string YOUR_PASSWORD)
        {
            string sqlExpression = String.Format("SELECT * FROM {0} WHERE Login = '{1}' AND Password = '{2}'", TABLES.USER_ACCOUNTS, YOUR_LOGIN, YOUR_PASSWORD);
            
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sqlExpression, connection);
                SqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    CREATION_STATUS = ($"SUCCESS login as user {YOUR_LOGIN}");

                    while (reader.Read()) // построчно считываем данные
                    {
                        this.NAME = (reader.GetValue(1)).ToString();
                        this.SURNAME = (reader.GetValue(2)).ToString();
                        this.REGISTRATION_DATE = (reader.GetValue(3)).ToString();
                        this.EMAIL = (reader.GetValue(4)).ToString();
                        this.LOGIN = (reader.GetValue(5)).ToString();
                        this.PASSWORD = (reader.GetValue(6)).ToString();
                        this.Driven_KMs = reader.GetDouble(9);
                    }

                    USERS_TABLE_NAME = TABLES.USER_ACCOUNTS;
                    IS_REAL_AKK = true;
                }   
            }
            if (!IS_REAL_AKK)
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    sqlExpression = String.Format("SELECT * FROM {0} WHERE Login = '{1}'", TABLES.USER_ACCOUNTS, YOUR_LOGIN);

                    connection.Open();
                    SqlCommand command2 = new SqlCommand(sqlExpression, connection);
                    SqlDataReader reader2 = command2.ExecuteReader();

                    if (reader2.HasRows)
                    {
                        CREATION_STATUS = ($"FAILURE login as user {YOUR_LOGIN}: invalid password for user {YOUR_LOGIN}");
                        throw new InsufficientRightsException(CREATION_STATUS);
                    }
                    else
                    {
                        CREATION_STATUS = ($"FAILURE login as user {YOUR_LOGIN}: such user is not present in table {TABLES.USER_ACCOUNTS}");
                        throw new DataNotFoundException(CREATION_STATUS);
                    }
                }
            }
        }


        public void Delete_Account(string YOUR_LOGIN, string YOUR_PASSWORD)
        {
            if (IS_REAL_AKK)
            {
                if (LOGIN == YOUR_LOGIN && PASSWORD == YOUR_PASSWORD)
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        string sqlExpression = String.Format("DELETE FROM {0} WHERE Login = '{1}'", USERS_TABLE_NAME, YOUR_LOGIN);
                        SqlCommand command = new SqlCommand(sqlExpression, connection);
                        int number = command.ExecuteNonQuery();
                        _del?.Invoke($"SUCCESS deleting user {YOUR_LOGIN} from table {USERS_TABLE_NAME}");
                    }
                }
                else
                {
                    _del?.Invoke($"FAILURE deleting user {YOUR_LOGIN}: invalid password");
                    //throw new InsufficientRightsException();
                }
            }
            else 
            {
                _del?.Invoke($"FAILURE deleting user {YOUR_LOGIN}: such user does not exist");
                //throw new DataNotFoundException();
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
                        string sqlExpression = String.Format("UPDATE {0} SET Password = '{1}' WHERE Login = '{2}'", USERS_TABLE_NAME, NEW_PASSWORD, LOGIN);
                        SqlCommand command = new SqlCommand(sqlExpression, connection);
                        int number = command.ExecuteNonQuery();
                        _del?.Invoke($"SUCCESS changing password for user {LOGIN} in table {USERS_TABLE_NAME}");
                        this.PASSWORD = NEW_PASSWORD;
                    }
                }
                else
                {
                    _del?.Invoke($"FAILURE changing password for user {LOGIN}: invalid old password");
                    //throw new InsufficientRightsException(_del.ToString());
                }
            }
            else
            {
                _del?.Invoke($"FAILURE changing password for user {LOGIN}: such user does not exist");
                //throw new DataNotFoundException();
            }
        }



        public Driver_Container[] GetActiveDrivers(DB_names TABLES)
        {
            if (Verification_Status)
            {
                int number_of_free_drivers;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string sqlExpression = String.Format("SELECT COUNT(*) FROM {0} WHERE {2} = '{1}'" +
                        "", TABLES.DRIVER_ACCOUNTS, "0", "Whom_Drives");

                    connection.Open();
                    SqlCommand command = new SqlCommand(sqlExpression, connection);
                    number_of_free_drivers = (int)command.ExecuteScalar();
                }
                Driver_Container[] Drivers_lst = new Driver_Container[number_of_free_drivers];
                for (int i = 0; i < number_of_free_drivers; ++i)
                {
                    Drivers_lst[i] = new Driver_Container();
                }

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string sqlExpression = String.Format("SELECT * FROM {0} WHERE Whom_Drives = '{1}'" +
                        "", TABLES.DRIVER_ACCOUNTS, "0");

                    connection.Open();
                    SqlCommand command = new SqlCommand(sqlExpression, connection);
                    SqlDataReader reader = command.ExecuteReader();
                    int i = 0;
                    while (reader.Read())
                    {
                        Drivers_lst[i].Name = (reader.GetValue(1)).ToString();
                        Drivers_lst[i].Surname = (reader.GetValue(2)).ToString();
                        Drivers_lst[i].Registration_Date = (reader.GetValue(3)).ToString();
                        Drivers_lst[i].Car = (reader.GetValue(7)).ToString();
                        Drivers_lst[i].Passengers_Served = (int)reader.GetInt32(9);
                        Drivers_lst[i].Working_Hours = reader.GetValue(12).ToString();
                        ++i;
                    }

                }
                return Drivers_lst;
            }
            else
            {
                return null;
            }
        }

        public Dictionary<string, (int, int)> GetDBLocations(DB_names TABLES)
        {
            if (Verification_Status)
            {
                Dictionary<string, (int, int)> Locations = new Dictionary<string, (int, int)>();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string sqlExpression = String.Format("SELECT * FROM {0}", TABLES.LOCATIONS);

                    connection.Open();
                    SqlCommand command = new SqlCommand(sqlExpression, connection);
                    SqlDataReader reader = command.ExecuteReader();
                    int i = 0;
                    while (reader.Read())
                    {
                        Locations.Add((reader.GetValue(1)).ToString(), ((int)reader.GetValue(2), (int)reader.GetValue(3)));
                        ++i;
                    }

                }
                _del?.Invoke($"SUCCESS fetching active drivers from table {TABLES.DRIVER_ACCOUNTS}");
                return Locations;
            }
            else
            {
                _del?.Invoke($"FAILURE fetching active drivers from table {TABLES.DRIVER_ACCOUNTS}: no active drivers present");
                return null;
            }
        }
   
        
        public void Make_Route_Order(DB_names TABLES, (int, int) User_Crnt_Position, (int, int) Destination)
        {
            //check if user did not made Order yet. User can make only one order at a time, and have to discard old order to make new one
            new Organize_User_Route(TABLES, this, _del, User_Crnt_Position, Destination);
        }

        public Route_Container Get_My_Order(DB_names TABLES)
        {
            Organize_User_Route Route = new Organize_User_Route(TABLES, this, _del);
            return Route.Get_User_Route();
        }
        public void Discard_Order(DB_names TABLES) {
            Organize_User_Route Route = new Organize_User_Route(TABLES, this, _del);
            Route.Discard_Route(TABLES, _del);
        }
        public void Accept_Order(DB_names TABLES)
        {
            Organize_User_Route Route = new Organize_User_Route(TABLES, this, _del);
            Route.Start_Route(TABLES, _del);
        }

        public double Check_Balance(DB_names TABLES) {
            string sqlExpression = String.Format("SELECT * FROM {0} WHERE Login = '{1}'", TABLES.USER_ACCOUNTS, LOGIN);
            double BALANCE = -1;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sqlExpression, connection);
                SqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    _del?.Invoke($"SUCCESS fetching balance of user {LOGIN}");

                    while (reader.Read())
                    {
                        BALANCE = reader.GetDouble(7);
                    }
                }
                else
                {
                    _del?.Invoke($"FAILURE fetching balance of user {LOGIN}");
                    //throw new DataNotFoundException();
                }
            }
            return BALANCE;
        }




        public string GetName {
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
        public double GetKMs
        {
            get { return Driven_KMs; }
        }
        public string GetCreation_Status
        {
            get { return CREATION_STATUS; }
        }
        public bool Verification_Status
        {
            get { return IS_REAL_AKK; }
        }


        private readonly string USERS_TABLE_NAME = null;
        private readonly string NAME = null;
        private readonly string SURNAME = null;
        private readonly string REGISTRATION_DATE = null;
        private readonly string EMAIL = null;
        private readonly string LOGIN = null;
        private string PASSWORD = null;

        private readonly double Driven_KMs = 0;
        private readonly string CREATION_STATUS = null;
        private readonly bool IS_REAL_AKK = false;
    }
}
