using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using Kurs_14_Taksopark;

namespace ConsoleApp
{
    namespace Kurs_14_Taksopark
    {
        class Program
        {
            static void Main(string[] args)
            {
                DB_names DBs = new DB_names("Taxi_Users", "Taxi_Drivers", "Location", "Orders", "Economic_Constants", "Finances");
                bool keep_working_app = true;
                int choose_acc;
                int choose_new_or_existed;
                User Syst_User;
                Driver Syst_Driver;
                string LastOperationState_Message = "*empty operation state status (no operations done yet)*";
                void Show_Message(String message)
                {
                    LastOperationState_Message = message;
                }

                try
                {
                    string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        string sqlExpression = ($"SELECT * FROM {DBs.USER_ACCOUNTS} WHERE Login = '0' AND Password = '0'");
                        SqlCommand command = new SqlCommand(sqlExpression, connection);
                        int number = command.ExecuteNonQuery();
                    }               
                }
                catch
                {
                    Console.WriteLine("Exception was risen because program can not connect to the BD");
                    Environment.Exit(-1);
                }

                Console.ForegroundColor = ConsoleColor.DarkGreen;
                while (keep_working_app)
                {
                    Console.WriteLine("Choose Type of account:");
                    Console.WriteLine("1 - User, 2 - Driver, 0 - quit program");
                    choose_acc = Int32.Parse(Console.ReadLine());
                    Console.WriteLine("\n#-----#");

                    if (choose_acc == 1)
                    {
                        Console.WriteLine("User account chosen.");
                        Console.WriteLine("1 - create new account, 2 - login into account that already exist, 0 - stop and start from \"Choose Type of account\"");
                        choose_new_or_existed = Int32.Parse(Console.ReadLine());
                        Console.WriteLine("\n#-----#");
                        if (choose_new_or_existed == 1)
                        {
                            string Name;
                            string Surname;
                            string EMail;
                            string Login;
                            string Password;
                            bool is_not_created = true;

                            bool start_from_beginning = false;
                            do
                            {
                                Console.WriteLine("You are creating new User account.");
                                Console.WriteLine("Set account Name:");
                                Name = Console.ReadLine();
                                Console.WriteLine("Set account Surname:");
                                Surname = Console.ReadLine();
                                Console.WriteLine("Set account EMail:");
                                EMail = Console.ReadLine();
                                Console.WriteLine("Set account Login:");
                                Login = Console.ReadLine();
                                Console.WriteLine("Set account Password:");
                                Password = Console.ReadLine();
                                Console.WriteLine("\nNew account credentials:");
                                Console.WriteLine($"Name: {Name}; Surname: {Surname}; EMail: {EMail}");
                                Console.WriteLine($"Login: {Login}; Password: {Password}");
                                Console.WriteLine("1 - confirm, 2 - discard and start over, 0 - stop and start from \"Choose Type of account\"");
                                int choose;
                                choose = Int32.Parse(Console.ReadLine());
                                if (choose == 1)
                                {
                                    is_not_created = false;
                                }
                                else if (choose == 2)
                                {
                                    //do nothing, start over
                                }
                                else
                                {
                                    start_from_beginning = true;
                                    break;
                                }
                            }
                            while (is_not_created);
                            if(start_from_beginning)
                            {
                                continue;
                            }
                            try
                            {
                                Syst_User = new User(DBs, Name, Surname, EMail, Login, Password);
                            }
                            catch (ArgumentNullException ex)
                            {
                                Console.WriteLine(ex.Message);
                                Console.WriteLine("Please try again\n\n");
                                continue;
                            }
                            catch (InsufficientRightsException ex)
                            {
                                Console.WriteLine(ex.Message);
                                Console.WriteLine("Please try again\n\n");
                                continue;
                            }
                            Syst_User.LastOperationState(new User.OperationState(Show_Message));
                        }
                        else if (choose_new_or_existed == 2)
                        {
                            string Login;
                            string Password;

                            Console.WriteLine("You are using existing User account.");
                            Console.WriteLine("Imput Login:");
                            Login = Console.ReadLine();
                            Console.WriteLine("Imput Password:");
                            Password = Console.ReadLine();

                            try
                            {
                                Syst_User = new User(DBs, Login, Password);
                            }
                            catch (DataNotFoundException ex)
                            {
                                Console.WriteLine(ex.Message);
                                Console.WriteLine("Please try again\n\n");
                                continue;
                            }
                            catch (InsufficientRightsException ex)
                            {
                                Console.WriteLine(ex.Message);
                                Console.WriteLine("Please try again\n\n");
                                continue;
                            }
                            Syst_User.LastOperationState(new User.OperationState(Show_Message));
                        }
                        else
                        {
                            continue;
                        }
                        while (keep_working_app)
                        {
                            Console.WriteLine("\n|############|\\\\");
                            Console.WriteLine($"You are now into \"{Syst_User.GetLogin}\" User account. Available options:");
                            Console.WriteLine("1 - get acc credentials, 2 - update password, 3 - delete current acc");
                            Console.WriteLine("41 - make route request, 42 - get request info, 43 - discard request, 44 - start drive");
                            Console.WriteLine("51 - check balance, 52 - check active drivers");
                            Console.WriteLine("61 - get status of last command, 62 - get DB locations");
                            Console.WriteLine("0 - quit current session (logout from app)");
                            Console.WriteLine("[don't forget to check status of last executed command]");
                            Console.WriteLine("[to update user credentials, logout and then login again]");
                            int caseSwitch = Int32.Parse(Console.ReadLine());
                            switch (caseSwitch)
                            {
                                case 1:
                                    Console.WriteLine(String.Format("Name : {0}, Surname: {1}", Syst_User.GetName, Syst_User.GetSurname));
                                    Console.WriteLine(String.Format("Login: {0}, Email: {1}", Syst_User.GetLogin, Syst_User.GetEmail));
                                    Console.WriteLine(String.Format("Registration date: {0}", Syst_User.GetRegistrationDate));
                                    Console.WriteLine(String.Format("Kilometers driven: {0}", Syst_User.GetKMs));
                                    break;
                                case 2:
                                    Console.WriteLine("Input your current password:");
                                    string cur_pswd = Console.ReadLine();
                                    Console.WriteLine("Input new password:");
                                    string new_pswd = Console.ReadLine();
                                    Syst_User.UpdatePassword(cur_pswd, new_pswd);
                                    break;
                                case 3:
                                    Console.WriteLine("Input your login:");
                                    string login = Console.ReadLine();
                                    Console.WriteLine("Input new password:");
                                    string pswd = Console.ReadLine();
                                    Syst_User.Delete_Account(login, pswd);
                                    keep_working_app = false;
                                    break;
                                case 41:
                                    (int, int) your_location;
                                    (int, int) destination;
                                    Console.WriteLine("Input your location's X coordinate:");
                                    your_location.Item1 = Int32.Parse(Console.ReadLine());
                                    Console.WriteLine("Input your location's Y coordinate:");
                                    your_location.Item2 = Int32.Parse(Console.ReadLine());

                                    Console.WriteLine("Input your destination's X coordinate:");
                                    destination.Item1 = Int32.Parse(Console.ReadLine());
                                    Console.WriteLine("Input your destination's Y coordinate:");
                                    destination.Item2 = Int32.Parse(Console.ReadLine());

                                    Syst_User.Make_Route_Order(DBs, your_location, destination);
                                    break;
                                case 42:
                                    Route_Container Rout = Syst_User.Get_My_Order(DBs);
                                    if (Rout.User_Login == "" || Rout.User_Login is null)
                                    {
                                        Console.WriteLine("User does not have active requests:");
                                    }
                                    else
                                    {
                                        Console.ForegroundColor = ConsoleColor.Red;
                                        if (Rout.Driver_Login == "" || Rout.Driver_Login == null || Rout.Driver_Login is null)
                                        {
                                            Console.WriteLine("Your request is not chosen yet.");
                                        }
                                        else
                                        {
                                            Console.WriteLine($"Your request was chosen by driver {Rout.Driver_Name}.");
                                        }
                                        
                                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                                        Console.WriteLine("User's credentials:");
                                        Console.WriteLine(String.Format("Name: {0}; Login: {1}; Email: {2}", Rout.User_Name, Rout.User_Login, Rout.User_Email));
                                        Console.WriteLine("Driver's credentials:");
                                        Console.WriteLine(String.Format("Name: {0}; Email: {1}; Car: {2}", Rout.Driver_Name, Rout.Driver_Email, Rout.Car));
                                        Console.WriteLine("Coordinates and distance:");
                                        Console.WriteLine(String.Format("Your location: {0}; Destination: {1}; Distance: {2}", Rout.User_Location, Rout.Destination, Rout.Distance));
                                        Console.WriteLine(String.Format("Request creation time: {0}", Rout.Order_Creation_Date));
                                    }
                                    break;
                                case 43:
                                    Syst_User.Discard_Order(DBs);
                                    break;
                                case 44:
                                    Syst_User.Accept_Order(DBs);
                                    break;
                                case 51:
                                    Console.WriteLine(String.Format("User's balance: {0} \n", Syst_User.Check_Balance(DBs)));
                                    break;
                                case 52:
                                    Driver_Container[] Active_Drivers = Syst_User.GetActiveDrivers(DBs);
                                    if (Active_Drivers != null)
                                    {
                                        for (int i = 0; i < Active_Drivers.Length; i += 1)
                                        {
                                            Console.WriteLine($"Driver #{i+1} credentials:");
                                            Console.WriteLine(String.Format("Name: {0}; Surname: {1}; Car: {2}", Active_Drivers[i].Name, Active_Drivers[i].Surname, Active_Drivers[i].Car));
                                            Console.WriteLine(String.Format("Passengers served: {0}; Working hours: {1}", Active_Drivers[i].Passengers_Served, Active_Drivers[i].Working_Hours));
                                            Console.WriteLine(String.Format("Registration date: {0}", Active_Drivers[i].Registration_Date));
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("There are no active drivers in DB");
                                    }
                                    break;
                                case 61:
                                    Console.WriteLine(LastOperationState_Message);
                                    break;
                                case 62:
                                    Dictionary<string, (int, int)> Locations = Syst_User.GetDBLocations(DBs);
                                    if (Locations is null)
                                    {
                                        Console.WriteLine("DB has no saved locations");
                                        break;
                                    }
                                    foreach (var item in Locations)
                                    {
                                        Console.WriteLine(String.Format("{0}: {1}", item.Key, item.Value));
                                    }
                                    break;
                                default:
                                    keep_working_app = false;
                                    break;
                            }
                            Console.WriteLine("|############|////");
                        }
                    }
                    else if (choose_acc == 2)
                    {
                        Console.WriteLine("Driver account chosen.");
                        Console.WriteLine("1 - create new account, 2 - login into account that already exist, 0 - stop and start from \"Choose Type of account\"");
                        choose_new_or_existed = Int32.Parse(Console.ReadLine());
                        Console.WriteLine("\n#-----#");
                        if (choose_new_or_existed == 1)
                        {
                            string Name;
                            string Surname;
                            string EMail;
                            string Car;
                            string[] Birth_Arr;
                            int DD;
                            int MM;
                            int YYYY;
                            string Login;
                            string Password;

                            bool is_not_created = true;
                            bool start_from_beginning = false;
                            do
                            {
                            Console.WriteLine("You are creating new Driver account.");
                            Console.WriteLine("Set account Name:");
                            Name = Console.ReadLine();
                            Console.WriteLine("Set account Surname:");
                            Surname = Console.ReadLine();
                            Console.WriteLine("Set account EMail:");
                            EMail = Console.ReadLine();
                            Console.WriteLine("Set your Car model:");
                            Car = Console.ReadLine();

                            Console.WriteLine("Set your birth date (DD-MM-YYYY):");
                            string Birth = Console.ReadLine();
                            Birth_Arr = Birth.Split('-');
                            DD = Int32.Parse(Birth_Arr[0]);
                            MM = Int32.Parse(Birth_Arr[1]);
                            YYYY = Int32.Parse(Birth_Arr[2]);

                            Console.WriteLine("Set account Login:");
                            Login = Console.ReadLine();
                            Console.WriteLine("Set account Password:");
                            Password = Console.ReadLine();
                                Console.WriteLine("1 - confirm, 2 - discard and start over, 0 - stop and start from \"Choose Type of account\"");
                                int choose;
                                choose = Int32.Parse(Console.ReadLine());
                                if (choose == 1)
                                {
                                    is_not_created = false;
                                }
                                else if (choose == 2)
                                {
                                    //do nothing, start over
                                }
                                else
                                {
                                    start_from_beginning = true;
                                    break;
                                }
                            }
                            while (is_not_created);
                            if(start_from_beginning)
                            {
                                continue;
                            }

                            try
                            {
                                Syst_Driver = new Driver(DBs, Name, Surname, EMail, Login, Password, DD, MM, YYYY, Car);
                            }
                            catch (ArgumentNullException ex)
                            {
                                Console.WriteLine(ex.Message);
                                Console.WriteLine("Please try again\n\n");
                                continue;
                            }
                            catch (InsufficientRightsException ex)
                            {
                                Console.WriteLine(ex.Message);
                                Console.WriteLine("Please try again\n\n");
                                continue;
                            }


                            Syst_Driver.LastOperationState(new Driver.OperationState(Show_Message));
                        }
                        else if (choose_new_or_existed == 2)
                        {
                            string Login;
                            string Password;


                            Console.WriteLine("You are using existing Driver account.");
                            Console.WriteLine("Imput account Login:");
                            Login = Console.ReadLine();
                            Console.WriteLine("Imput account Password:");
                            Password = Console.ReadLine();

                            try
                            {
                                Syst_Driver = new Driver(DBs, Login, Password);
                            }
                            catch (DataNotFoundException ex)
                            {
                                Console.WriteLine(ex.Message);
                                Console.WriteLine("Please try again\n\n");
                                continue;
                            }
                            catch (InsufficientRightsException ex)
                            {
                                Console.WriteLine(ex.Message);
                                Console.WriteLine("Please try again\n\n");
                                continue;
                            }

                            Syst_Driver.LastOperationState(new Driver.OperationState(Show_Message));
                        }
                        else
                        {
                            continue;
                        }
                        while (keep_working_app)
                        {
                            Console.WriteLine("\n|############|\\\\");
                            Console.WriteLine($"You are now into \"{Syst_Driver.GetLogin}\" Driver account. Available options:");
                            Console.WriteLine("1 - get acc credentials, 2 - update password");
                            Console.WriteLine("31 - start working session, 32 - stop working session");
                            Console.WriteLine("41 - fetch open requests, 42 - book open request, 43 - discard booking");
                            Console.WriteLine("61 - get status of last command, 62 - fetch my request");
                            Console.WriteLine("0 - quit current session (logout from app)");
                            Console.WriteLine("[don't forget to check status of last executed command]");
                            Console.WriteLine("[to update driver credentials, logout and then login again]");
                            int caseSwitch = Int32.Parse(Console.ReadLine());

                            switch (caseSwitch)
                            {
                                case 1:
                                    Console.WriteLine(String.Format("Name : {0}, Surname: {1}", Syst_Driver.GetName, Syst_Driver.GetSurname));
                                    Console.WriteLine(String.Format("Login: {0}, Email: {1}", Syst_Driver.GetLogin, Syst_Driver.GetEmail));
                                    Console.WriteLine(String.Format("Registration date: {0}", Syst_Driver.GetRegistrationDate));
                                    Console.WriteLine(String.Format("Car model: {0}", Syst_Driver.GetCar));
                                    Console.WriteLine(String.Format("Is working now: {0}", Syst_Driver.GetWorkingStatus));
                                    break;
                                case 2:
                                    Console.WriteLine("Input your current password:");
                                    string cur_pswd = Console.ReadLine();
                                    Console.WriteLine("Input new password:");
                                    string new_pswd = Console.ReadLine();
                                    Syst_Driver.UpdatePassword(cur_pswd, new_pswd);
                                    break;
                                case 31:
                                    Syst_Driver.Start_Working_Session();
                                    break;
                                case 32:
                                    Syst_Driver.Stop_Working_Session();
                                    break;
                                case 41:
                                    Route_Container[] Fetch = Syst_Driver.Fetch_Open_Orders(DBs);
                                    if (Syst_Driver.GetWorkingStatus)
                                    {
                                        if (Fetch != null)
                                        {
                                            for (int i = 0; i < Fetch.Length; i += 1)
                                            {
                                                Console.WriteLine($"~~~~~User #{i + 1}:");
                                                Console.WriteLine("User credentials:");
                                                Console.WriteLine(String.Format("Name: {0}; Login: {1}; Email: {2}", Fetch[i].User_Name, Fetch[i].User_Login, Fetch[i].User_Email));
                                                Console.WriteLine("Driver's credentials:");
                                                Console.WriteLine(String.Format("Name: {0}; Email: {1}; Car: {2}", Fetch[i].Driver_Name, Fetch[i].Driver_Email, Fetch[i].Car));
                                                Console.WriteLine("Coordinates and distance:");
                                                Console.WriteLine(String.Format("User's location: {0}; Destination: {1}; Distance: {2}", Fetch[i].User_Location, Fetch[i].Destination, Fetch[i].Distance));
                                                Console.WriteLine(String.Format("Request creation time: {0}", Fetch[i].Order_Creation_Date));
                                                Console.WriteLine($"~~~~~~~~~~~~~~~\n");
                                            }
                                        }
                                        else
                                        {
                                            Console.WriteLine("There are no available open requeste in DB");
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine(LastOperationState_Message);
                                    }
                                    break;
                                case 42:
                                    Console.WriteLine("Input user's login to book his request:");
                                    string login = Console.ReadLine();
                                    Syst_Driver.Book_Open_Order_by_User(DBs, login);
                                    if (!Syst_Driver.GetWorkingStatus)
                                    {
                                        Console.WriteLine(LastOperationState_Message);
                                    }
                                    break;
                                case 43:
                                    Console.WriteLine("Input user's login (whom you booked) to discard his request:");
                                    string login2 = Console.ReadLine();
                                    Syst_Driver.Discard_Order(DBs, login2);
                                    if (!Syst_Driver.GetWorkingStatus)
                                    {
                                        Console.WriteLine(LastOperationState_Message);
                                    }
                                    break;
                                case 61:
                                    Console.WriteLine(LastOperationState_Message);
                                    break;
                                case 62:
                                    Route_Container Fetch_My = Syst_Driver.Fetch_My_Order(DBs);
                                    if (Syst_Driver.GetWorkingStatus)
                                    {
                                        if (Fetch_My != null)
                                        {
                                            Console.WriteLine($"~~~~~~~~~~~~~~~");
                                            Console.WriteLine("User credentials:");
                                            Console.WriteLine(String.Format("Name: {0}; Login: {1}; Email: {2}", Fetch_My.User_Name, Fetch_My.User_Login, Fetch_My.User_Email));
                                            Console.WriteLine("Driver's credentials:");
                                            Console.WriteLine(String.Format("Name: {0}; Email: {1}; Car: {2}", Fetch_My.Driver_Name, Fetch_My.Driver_Email, Fetch_My.Car));
                                            Console.WriteLine("Coordinates and distance:");
                                            Console.WriteLine(String.Format("User's location: {0}; Destination: {1}; Distance: {2}", Fetch_My.User_Location, Fetch_My.Destination, Fetch_My.Distance));
                                            Console.WriteLine(String.Format("Request creation time: {0}", Fetch_My.Order_Creation_Date));
                                            Console.WriteLine($"~~~~~~~~~~~~~~~\n");
                                        }
                                        else
                                        {
                                            Console.WriteLine("You have no booked requests in DB");
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine(LastOperationState_Message);
                                    }
                                    break;
                                default:
                                    Syst_Driver.Stop_Working_Session();
                                    keep_working_app = false;
                                    break;
                            }

                            Console.WriteLine("|############|////");
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
    }
}
