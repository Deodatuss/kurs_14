using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Data.SqlClient;


namespace Kurs_14_Taksopark
{

    class DB_Position : Position
    {
        public delegate void OperationState(string message);
        OperationState _del;
        public void LastOperationState(OperationState del)
        {
            _del = del;
        }
        private string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;


        DB_Position(int X_Coordinate, int Y_Coordinate) :base() 
        {
            this.X_Coordinate = X_Coordinate;
            this.Y_Coordinate = Y_Coordinate;
            CREATION_STATUS = ($"SUCCESS creating new unnamed Location");
        }

        //fetch existing positions
        DB_Position(DB_names TABLES, string Existing_Location_Name) : base(Existing_Location_Name)
        {
            string sqlExpression = String.Format("SELECT * FROM {0} WHERE Name = '{1}'", TABLES.LOCATIONS, Existing_Location_Name);
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sqlExpression, connection);
                SqlDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    CREATION_STATUS = ($"SUCCESS using location {Existing_Location_Name}");
                    while (reader.Read())
                    {
                        Location_Name = (reader.GetValue(1)).ToString();
                        X_Coordinate = Int32.Parse((reader.GetValue(2)).ToString());
                        X_Coordinate = Int32.Parse((reader.GetValue(3)).ToString());

                    }
                    POSITIONS_TABLE_NAME = TABLES.USER_ACCOUNTS;
                }
                else
                {
                    CREATION_STATUS = ($"FAILURE using location {Existing_Location_Name}: such location is not present in table {TABLES.LOCATIONS}");
                    throw new DataNotFoundException(Existing_Location_Name);
                }
            }
        }


        public void Save_Position_To_DB(DB_names TABLES, Driver Certified_akk, string New_Location_Name)
        {
            if (Certified_akk is null)
            {
                throw new ArgumentNullException(nameof(Certified_akk));
            }
            if (!Certified_akk.Verification_Status)
            {
                _del?.Invoke($"FAILURE adding Location {Location_Name}: access denied because of insufficient rights");
                throw new InsufficientRightsException(nameof(Certified_akk));
            }
            else
            {
                if (New_Location_Name is null)
                {
                    _del?.Invoke($"FAILURE creating new Location: empty name for Location");
                    throw new ArgumentNullException(nameof(New_Location_Name));
                }
                else
                {
                    bool isNameFree = true;

                    //checking if input name hasn't been used before for other place
                    string sqlExpressionCheck = String.Format("SELECT * FROM {0} WHERE Name = '{1}'", TABLES.LOCATIONS, New_Location_Name);
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        SqlCommand command = new SqlCommand(sqlExpressionCheck, connection);
                        SqlDataReader reader = command.ExecuteReader();
                        if (reader.HasRows)
                        {
                            isNameFree = false;
                            _del?.Invoke($"FAILURE adding Location {New_Location_Name}: Location {New_Location_Name} already exist in table {TABLES.LOCATIONS}");
                        }
                    }
                    if (isNameFree && Certified_akk.Verification_Status)
                    {
                        Location_Name = New_Location_Name;


                        string sqlExpression = String.Format("INSERT INTO {0} (Name, X_Coordinate, Y_Coordinate, WhoAdded)" +
                            " VALUES ('{1}', '{2}', '{3}', '{4}')", TABLES.LOCATIONS, X_Coordinate, Y_Coordinate, Location_Name, Certified_akk.GetLogin);
                        using (SqlConnection connection = new SqlConnection(connectionString))
                        {
                            connection.Open();
                            SqlCommand command = new SqlCommand(sqlExpression, connection);
                            int number = command.ExecuteNonQuery();
                            _del?.Invoke($"SUCCESS adding Location {Location_Name} to table {TABLES.LOCATIONS}");
                        }
                    }
                }
            }
        }
        public void Delete_Position_from_DB(DB_names TABLES, Driver Certified_akk, string Location_Name)
        {
            if (Certified_akk is null)
            {
                throw new ArgumentNullException(nameof(Certified_akk));
            }
            if (!Certified_akk.Verification_Status)
            {
                _del?.Invoke($"FAILURE deleting Location {Location_Name}: access denied because of insufficient rights");
                throw new InsufficientRightsException(nameof(Certified_akk));
            }
            else
            {
                if (Location_Name is null)
                {
                    _del?.Invoke($"FAILURE deleting Location: empty name for Location");
                    throw new ArgumentNullException(nameof(Location_Name));
                }
                else
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        string sqlExpression = String.Format("DELETE FROM {0} WHERE Name = '{1}'", TABLES.LOCATIONS, Location_Name);
                        SqlCommand command = new SqlCommand(sqlExpression, connection);
                        int number = command.ExecuteNonQuery();
                        _del?.Invoke($"SUCCESS deleting Location {Location_Name} from table {TABLES.LOCATIONS}");
                    }
                }
            }
        }


        


        public (int, int) GetCoordinates
        {
            get { return (X_Coordinate, Y_Coordinate); }
        }
        public string GetCreation_Status
        {
            get { return CREATION_STATUS; }
        }


        private string POSITIONS_TABLE_NAME = null;
        private string CREATION_STATUS = null;
    }
}
