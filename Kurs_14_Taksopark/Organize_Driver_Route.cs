using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Data.SqlClient;

namespace Kurs_14_Taksopark
{
    internal class Organize_Driver_Route
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        public Organize_Driver_Route(DB_names TABLES, Driver driver, Driver.OperationState del)
        {
            if (driver is null)
            {
                throw new ArgumentNullException(nameof(driver));
            }
            if (driver.Verification_Status)
            {
                //////*this code is definitely redundant, but cept in case it was actually necessary 
                //using (SqlConnection connection = new SqlConnection(connectionString))
                //{
                //    string sqlExpressionCount = String.Format("SELECT COUNT(*) FROM {0} WHERE Driver_Login IS NOT NULL AND Is_Done = 0" +
                //        "", TABLES.USER_ORDERS);

                //    connection.Open();
                //    SqlCommand command = new SqlCommand(sqlExpressionCount, connection);
                //    command.ExecuteScalar();
                //}

                int number_of_requests;
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string sqlExpressionCount = String.Format("SELECT COUNT(*) FROM {0} WHERE Driver_Login IS NULL AND Is_Done = 0" +
                        "", TABLES.USER_ORDERS);

                    connection.Open();
                    SqlCommand command = new SqlCommand(sqlExpressionCount, connection);
                    number_of_requests = (int)command.ExecuteScalar();
                }


                string sqlExpression = String.Format("SELECT * FROM {0} WHERE Driver_Login IS NULL AND Is_Done = 0", TABLES.USER_ORDERS);
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand(sqlExpression, connection);
                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.HasRows)
                    {
                        R_Containers_For_Driver = new Route_Container[number_of_requests];
                        for (int j = 0; j < number_of_requests; ++j)
                        {
                            R_Containers_For_Driver[j] = new Route_Container();
                        }
                        int i = 0;
                        while (reader.Read())
                        {
                            R_Containers_For_Driver[i].User_Login = (reader.GetValue(1)).ToString();
                            R_Containers_For_Driver[i].Driver_Login = (reader.GetValue(2)).ToString();
                            R_Containers_For_Driver[i].User_Email = (reader.GetValue(3)).ToString();
                            R_Containers_For_Driver[i].Driver_Email = (reader.GetValue(4)).ToString();
                            R_Containers_For_Driver[i].User_Name = (reader.GetValue(5)).ToString();
                            R_Containers_For_Driver[i].Driver_Name = (reader.GetValue(6)).ToString();
                            R_Containers_For_Driver[i].User_Location = (reader.GetInt32(7), reader.GetInt32(8));
                            R_Containers_For_Driver[i].Destination = (reader.GetInt32(9), reader.GetInt32(10));
                            R_Containers_For_Driver[i].Distance = reader.GetDouble(11);
                            R_Containers_For_Driver[i].Car = (reader.GetValue(12)).ToString();
                            R_Containers_For_Driver[i].Order_Creation_Date = (reader.GetValue(13)).ToString();
                            ++i;
                        }
                        LOGIN = driver.GetLogin;
                        EMAIL = driver.GetEmail;
                        NAME = driver.GetName;
                        CAR = driver.GetCar;

                        del?.Invoke($"SUCCESS fetching orders from table {TABLES.USER_ORDERS}. Use Get_All_Routes to return all containers with accessible order data");
                    }
                    else
                    {
                        del?.Invoke($"FAILURE fetching orders from table {TABLES.USER_ORDERS}: table has zero valid accessible requests");
                    }
                }
            }
            else
            {
                throw new InsufficientRightsException(nameof(driver));
            }
        }
        
        public Route_Container Get_My_Route(DB_names TABLES, Driver.OperationState del)
        {
            Route_Container My_Route = new Route_Container();
            string sqlExpression = String.Format("SELECT * FROM {0} WHERE Driver_Login = '{1}' AND Is_Done = 0", TABLES.USER_ORDERS, LOGIN);
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sqlExpression, connection);
                SqlDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        My_Route.User_Login = (reader.GetValue(1)).ToString();
                        My_Route.Driver_Login = (reader.GetValue(2)).ToString();
                        My_Route.User_Email = (reader.GetValue(3)).ToString();
                        My_Route.Driver_Email = (reader.GetValue(4)).ToString();
                        My_Route.User_Name = (reader.GetValue(5)).ToString();
                        My_Route.Driver_Name = (reader.GetValue(6)).ToString();
                        My_Route.User_Location = (reader.GetInt32(7), reader.GetInt32(8));
                        My_Route.Destination = (reader.GetInt32(9), reader.GetInt32(10));
                        My_Route.Distance = reader.GetDouble(11);
                        My_Route.Car = (reader.GetValue(12)).ToString();
                        My_Route.Order_Creation_Date = (reader.GetValue(13)).ToString();
                        del?.Invoke("SUCCESS fetching user's route data");
                    }
                }
                else {
                    del?.Invoke("FAILURE fetching user's route data: no routes present");
                    return null;
                }
            }
            return My_Route;
        }


        public Route_Container[] Get_All_Routes()
        {
            return R_Containers_For_Driver;
        }
        public void Choose_User_Route_by_his_Login(DB_names TABLES, string login, Driver.OperationState del)
        {
            string sqlExpression = String.Format("SELECT * FROM {0} WHERE Driver_Login = '{1}' AND Is_Done = 0", TABLES.USER_ORDERS, LOGIN);
            bool is_free_to_book = false;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sqlExpression, connection);
                SqlDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    del?.Invoke($"FAILURE taking Order from user {login}. You already have an order and have to discard or finish it to take new one");
                }
                else
                {
                    is_free_to_book = true;
                }
            }
            if(is_free_to_book)
            { 
                sqlExpression = String.Format("UPDATE {0} SET Driver_Login = '{1}', Driver_Email = '{2}', Driver_Name = '{3}', Driver_Car = '{4}'" +
                        " WHERE User_Login = '{5}' AND Driver_Login IS NULL AND Is_Done = 0", TABLES.USER_ORDERS, LOGIN, EMAIL, NAME, CAR, login);
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    int number;
                    SqlCommand command = new SqlCommand(sqlExpression, connection);
                    number = command.ExecuteNonQuery();

                    if (number > 0)
                    {
                        del?.Invoke($"SUCCESS taking Order from user {login}. Drive to his location");
                    }
                    else
                    {
                        del?.Invoke($"FAILURE taking Order from user {login}: such user with valid(open) request does not exist in table {TABLES.USER_ORDERS}");
                    }
                }
            }
        }

        public void Discard_Route(DB_names TABLES, string login, Driver.OperationState del)
        {
            string sqlExpression = String.Format("UPDATE {0} SET Driver_Login = NULL, Driver_Email = NULL, Driver_Name = NULL, Driver_Car = NULL" +
                    " WHERE User_Login = '{1}' AND Driver_Login = '{2}' AND Is_Done = 0", TABLES.USER_ORDERS, login, LOGIN);

            int number;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sqlExpression, connection);
                number = command.ExecuteNonQuery();
            }
            if (number > 0)
            {
                del?.Invoke($"SUCCESS Discarding Order from user {login}. This order is now open");
            }
            else
            {
                del?.Invoke($"FAILURE Discarding Order from user {login}: such user with valid(open) request does not exist or it's not your order");
            }
        }


        private readonly string LOGIN;
        private readonly string EMAIL;
        private readonly string NAME;
        private readonly string CAR;

        private readonly Route_Container[] R_Containers_For_Driver;
    }
}
