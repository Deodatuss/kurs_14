using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Data.SqlClient;
using System.Globalization;

namespace Kurs_14_Taksopark
{
    internal class Organize_User_Route
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        public Organize_User_Route(DB_names TABLES, User user, User.OperationState del, (int, int) User_Crnt_Position, (int, int) Destination)
        {
            //user's validity check
            if (user is null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (!user.Verification_Status)
            {
                throw new InsufficientRightsException(nameof(user));
            }


            Calculate_Route DIST = new Calculate_Route(User_Crnt_Position, Destination);
            double dist = DIST.GetDistance;
            bool IsLegidToOrder = true;

            //check if user did not make valid requests before
            string sqlExpressionCheck = String.Format("SELECT * FROM {0} WHERE User_Login = '{1}' AND Is_Done = 0", TABLES.USER_ORDERS, user.GetLogin);
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sqlExpressionCheck, connection);
                SqlDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    del?.Invoke($"FAILURE making Route Order from user {user.GetLogin}: user {user.GetLogin} already made a request, and has to discard his old request to make a new one");
                    IsLegidToOrder = false;
                }
            }


            //check if user has enough cash
            double User_Cash = 0;
            sqlExpressionCheck = String.Format("SELECT * FROM {0} WHERE Login = '{1}'", TABLES.USER_ACCOUNTS, user.GetLogin);
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sqlExpressionCheck, connection);
                SqlDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        User_Cash = reader.GetDouble(7);
                    }
                }
            }

            //get value constants
            sqlExpressionCheck = String.Format("SELECT Name_of_Constant, Value FROM {0}", TABLES.FINANCE_CONSTANTS);
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sqlExpressionCheck, connection);
                SqlDataReader reader = command.ExecuteReader();
                {
                    reader.Read();
                    Charge_per_KM = reader.GetDouble(1);
                    reader.Read();
                    Users_KM_to_Privilage = reader.GetDouble(1);
                    reader.Read();
                    Percent_That_Goes_to_Driver = reader.GetDouble(1);
                    reader.Read();
                    Discount_Value = reader.GetDouble(1);
                }
                if (dist * Charge_per_KM > User_Cash)
                {
                    del?.Invoke($"FAILURE making Route Order from user {user.GetLogin}: user {user.GetLogin} do not have enough on balance to pay for the route");
                    IsLegidToOrder = false;
                }
            }

            //if valid to order, write order to DB
            if (IsLegidToOrder)
            {
                string sqlExpression = String.Format("INSERT INTO {0} (User_Login, User_Email, User_Name, User_Location_X, User_Location_Y, Destination_X, Destination_Y, Distance)" +
                    " VALUES ('{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}')", TABLES.USER_ORDERS, user.GetLogin, user.GetEmail, user.GetName, User_Crnt_Position.Item1, User_Crnt_Position.Item2, Destination.Item1, Destination.Item2, dist.ToString(CultureInfo.InvariantCulture));

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand(sqlExpression, connection);
                    int number = command.ExecuteNonQuery();
                }
                del?.Invoke($"SUCCESS making Route Order from user {user.GetLogin}. Fetch data and wait for approval");
            }
        }


        //get existent open requests from a user (not creating new requests, just fetching if possible)
        public Organize_User_Route(DB_names TABLES, User user, User.OperationState del)
        {
            if (user is null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (user.Verification_Status)
            {
                USER_DRIVEN_KM = user.GetKMs;

                string sqlExpression = String.Format("SELECT * FROM {0} WHERE User_Login = '{1}' AND Is_Done = 0", TABLES.USER_ORDERS, user.GetLogin);

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand(sqlExpression, connection);
                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            R_Container.User_Login = (reader.GetValue(1)).ToString();
                            R_Container.Driver_Login = (reader.GetValue(2)).ToString();
                            R_Container.User_Email = (reader.GetValue(3)).ToString();
                            R_Container.Driver_Email = (reader.GetValue(4)).ToString();
                            R_Container.User_Name = (reader.GetValue(5)).ToString();
                            R_Container.Driver_Name = (reader.GetValue(6)).ToString();
                            R_Container.User_Location = (reader.GetInt32(7), reader.GetInt32(8));
                            R_Container.Destination = (reader.GetInt32(9), reader.GetInt32(10));
                            R_Container.Distance = reader.GetDouble(11);
                            R_Container.Car = (reader.GetValue(12)).ToString();
                            R_Container.Order_Creation_Date = (reader.GetValue(13)).ToString();

                            del?.Invoke($"SUCCESS fetching order from user {user.GetLogin}. Use Get_User_Route to return container with order data");
                        }
                    }
                    else
                    {
                        del?.Invoke($"FAILURE fetching order from user {user.GetLogin}: order does not exist, user has zero current requests");
                    }
                }
                sqlExpression = String.Format("SELECT Name_of_Constant, Value FROM {0}", TABLES.FINANCE_CONSTANTS);
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand(sqlExpression, connection);
                    SqlDataReader reader = command.ExecuteReader();
                    {
                        reader.Read();
                        Charge_per_KM = reader.GetDouble(1);
                        reader.Read();
                        Users_KM_to_Privilage = reader.GetDouble(1);
                        reader.Read();
                        Percent_That_Goes_to_Driver = reader.GetDouble(1);
                        reader.Read();
                        Discount_Value = reader.GetDouble(1);
                    }
                }
            }
            else
            {
                throw new InsufficientRightsException(nameof(user));
            }
        }

        public Route_Container Get_User_Route()
        {
             return R_Container;
        }

        

        public void Discard_Route(DB_names TABLES, User.OperationState del)
        {
            bool is_ready_to_delete = false;
            string sqlExpressionCheck = String.Format("SELECT * FROM {0} WHERE User_Login = '{1}' AND Is_Done = 0", TABLES.USER_ORDERS, R_Container.User_Login);
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sqlExpressionCheck, connection);
                SqlDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    is_ready_to_delete = true;
                }
                else
                {
                    del?.Invoke($"FAILURE deleting Route Order from user {R_Container.User_Login}: such Order does not exist in a first place");
                }
            }

            if (is_ready_to_delete)
            {
                sqlExpressionCheck = String.Format("DELETE FROM {0} WHERE User_Login = '{1}' AND Is_Done = 0", TABLES.USER_ORDERS, R_Container.User_Login);
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand(sqlExpressionCheck, connection);
                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.HasRows)
                    {
                        del?.Invoke($"FAILURE deleting Route Order from user {R_Container.User_Login}: something went wrong and request is still on server");       
                    }
                    else
                    {
                        del?.Invoke($"SUCCESS deleting Route Order from user {R_Container.User_Login} in table {TABLES.USER_ORDERS}");
                    }
                }
            }
        }

        public void Start_Route(DB_names TABLES, User.OperationState del)
        {
            bool accepted = false;
            if (R_Container.User_Login is null || R_Container.User_Login == "")
            {
                del?.Invoke($"FAILURE starting Drive to Destination for user [NULL]: open request from this user does not exist");
            }
            else
            { 
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sqlExpression = String.Format("SELECT * FROM {0} WHERE User_Login = '{1}' AND Is_Done = 0 AND Driver_Login IS NULL", TABLES.USER_ORDERS, R_Container.User_Login);
                    SqlCommand command = new SqlCommand(sqlExpression, connection);
                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.HasRows)
                    {
                        del?.Invoke($"FAILURE starting Drive to Destination for user {R_Container.User_Login}: request has not been chosen by any driver yet");
                    }
                    else
                    {
                        accepted = true;
                    }
                }

                if (accepted)
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        string sqlExpression = String.Format("UPDATE {0} SET Is_Done = 1, Order_Done_Date = GETDATE() WHERE User_Login = '{1}' AND Is_Done = 0", TABLES.USER_ORDERS, R_Container.User_Login);
                        SqlCommand command = new SqlCommand(sqlExpression, connection);
                        int number = command.ExecuteNonQuery();

                        if (USER_DRIVEN_KM > Users_KM_to_Privilage)
                        {
                            sqlExpression = String.Format("INSERT INTO {0} (Sender_Login, Sender_Role, Requester_Login, Requester_Role, Distance, Money_Transaction)" +
                                " VALUES ('{1}', 'User', 'admin', 'Admin', '{2}', '{3}')", TABLES.TRANSACTIONS, R_Container.User_Login, ((double)R_Container.Distance).ToString(CultureInfo.InvariantCulture), ((double)(R_Container.Distance * Charge_per_KM - R_Container.Distance * Charge_per_KM * Discount_Value)).ToString(CultureInfo.InvariantCulture));
                            command = new SqlCommand(sqlExpression, connection);
                            number = command.ExecuteNonQuery();

                            sqlExpression = String.Format("UPDATE {0} SET Money_Balance = Money_Balance - {1}, KMs_Drived = KMs_Drived + {2} WHERE Login = '{3}'", TABLES.USER_ACCOUNTS, ((double)(R_Container.Distance * Charge_per_KM - R_Container.Distance * Charge_per_KM * Discount_Value)).ToString(CultureInfo.InvariantCulture), ((double)(R_Container.Distance)).ToString(CultureInfo.InvariantCulture), R_Container.User_Login);
                            command = new SqlCommand(sqlExpression, connection);
                            number = command.ExecuteNonQuery();
                        }
                        else
                        {
                            sqlExpression = String.Format("INSERT INTO {0} (Sender_Login, Sender_Role, Requester_Login, Requester_Role, Distance, Money_Transaction)" +
                                " VALUES ('{1}', 'User', 'admin', 'Admin', '{2}', '{3}')", TABLES.TRANSACTIONS, R_Container.User_Login, ((double)R_Container.Distance).ToString(CultureInfo.InvariantCulture), ((double)(R_Container.Distance * Charge_per_KM)).ToString(CultureInfo.InvariantCulture));
                            command = new SqlCommand(sqlExpression, connection);
                            number = command.ExecuteNonQuery();

                            sqlExpression = String.Format("UPDATE {0} SET Money_Balance = Money_Balance - {1}, KMs_Drived = KMs_Drived + {2} WHERE Login = '{3}'", TABLES.USER_ACCOUNTS, ((double)(R_Container.Distance * Charge_per_KM)).ToString(CultureInfo.InvariantCulture), ((double)(R_Container.Distance)).ToString(CultureInfo.InvariantCulture), R_Container.User_Login);
                            command = new SqlCommand(sqlExpression, connection);
                            number = command.ExecuteNonQuery();
                        }

                        sqlExpression = String.Format("INSERT INTO {0} (Sender_Login, Sender_Role, Requester_Login, Requester_Role, Distance, Money_Transaction)" +
                            " VALUES ('admin', 'Admin', '{1}', 'Driver', '{2}', '{3}')", TABLES.TRANSACTIONS, R_Container.Driver_Login, ((double)R_Container.Distance).ToString(CultureInfo.InvariantCulture), ((double)(R_Container.Distance * Charge_per_KM * Percent_That_Goes_to_Driver)).ToString(CultureInfo.InvariantCulture));
                        command = new SqlCommand(sqlExpression, connection);
                        number = command.ExecuteNonQuery();

                        sqlExpression = String.Format("UPDATE {0} SET Money_Balance = Money_Balance + {1}, Passengers_Served = Passengers_Served + 1 WHERE Login = '{2}'", TABLES.DRIVER_ACCOUNTS, ((double)(R_Container.Distance * Charge_per_KM * Percent_That_Goes_to_Driver)).ToString(CultureInfo.InvariantCulture), R_Container.Driver_Login);
                        command = new SqlCommand(sqlExpression, connection);
                        number = command.ExecuteNonQuery();


                        del?.Invoke($"SUCCESS starting Drive to Destination for user {R_Container.User_Login} in table {TABLES.USER_ORDERS}");

                    }
                }
            }
        }

        private readonly double USER_DRIVEN_KM;

        private readonly double Charge_per_KM;
        private readonly double Users_KM_to_Privilage;
        private readonly double Percent_That_Goes_to_Driver;
        private readonly double Discount_Value;


        private readonly Route_Container R_Container = new Route_Container();
    }
}
