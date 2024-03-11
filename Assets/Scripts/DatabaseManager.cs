using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Mono.Data.Sqlite;
using System.Data;
using System;
using System.Data.Common;
 

public class DistrictDataDistribution
{
    public float IndustryAmount { get; set; }
    public float ResidentialAmount { get; set; }
    public float CommercialAmount { get; set; }


    public DistrictDataDistribution(float industryAmount, float residentialAmount, float commercialAmount)
    {
        IndustryAmount = industryAmount;
        ResidentialAmount = residentialAmount;
        CommercialAmount = commercialAmount;
    }
}

public class DesiredDistrictDataDistribution
{
    public float DesiredIndustryAmount { get; set; }
    public float DesiredResidentialAmount { get; set; }
    public float DesiredCommercialAmount { get; set; }


    public DesiredDistrictDataDistribution(float desiredIndustryAmount, float desiredResidentialAmount, float desiredCommercialAmount)
    {
        DesiredIndustryAmount = desiredIndustryAmount;
        DesiredResidentialAmount = desiredResidentialAmount;
        DesiredCommercialAmount = desiredCommercialAmount;
    }
}


public class PopulationBatchUpdate
{
    public string Type { get; set; }
    public string Departure { get; set; }
    public string Destination { get; set; }
    public float BestTime { get; set; }
    public float ActualTime { get; set; }
    public bool TransportationNeed { get; set; }
    public float Satisfaction { get; set; }
}

public class DistrictBatchUpdate
{
    public string DistrictName { get; set; }
    public float IndustrialAmountChange { get; set; }
    public float CommercialAmountChange { get; set; }
    public float ResidentialAmountChange { get; set; }
}


//https://forum.unity.com/threads/installing-sqlite-on-macos-and-probably-for-windows-unity-2021-1-16.1179430/
public class DatabaseManager : MonoBehaviour
{

    private string connectionString;

    string conn;
    string sqlQuery;
    IDbConnection dbconn;
    IDbCommand dbcmd;
    IDataReader dbreader;  
    string DATABASE_NAME = "/commutocracy.s3db";

    int dbConnCount = 0;
    public float satisfaction = 0;
    //List of district names
    public List<string> districtNames = new List<string>();


    void Start()
    {
            string filepath = Application.dataPath + "/commutocracy.s3db";
            conn = "URI=file:" + filepath;
            OpenConnection(); // Open a single connection when the game start
            CreateATable();
            ResetTables();
     }

     private void OpenConnection()
        {
            if (dbconn == null)
            {
                dbconn = new SqliteConnection(conn);
                dbconn.Open();
                Debug.Log("Database connection opened.");
            }
     }

     void OnDestroy()
        {
            CloseConnection(); // Ensure the connection is closed properly.
        }

    private void CloseConnection()
    {
        if (dbconn != null && dbconn.State != ConnectionState.Closed)
        {
            dbconn.Close();
            dbconn.Dispose();
            dbconn = null;
            Debug.Log("Database connection closed.");
        }
    }



    //private void InitializeDatabase()
    //{
    //    using (IDbConnection dbConnection = new SqliteConnection(connectionString))
    //    {
    //        dbConnection.Open();

    //        using (IDbCommand dbCmd = dbConnection.CreateCommand())
    //        {
    //            // Create DistrictData table if it doesn't exist
    //            string query = "CREATE TABLE IF NOT EXISTS DistrictData (id INTEGER PRIMARY KEY, industry_amount FLOAT)";
    //            dbCmd.CommandText = query;
    //            dbCmd.ExecuteScalar();


    //            Debug.Log("Database and tables initialized successfully");
    //            // ... Create other tables
    //        }
    //        dbConnection.Close();
    //    }
    //}


    //Restart Data at some point at each start 
    private void CreateATable()
    {
        // Ensure the database connection is open before proceeding.
        if (dbconn == null || dbconn.State != ConnectionState.Open)
        {
            Debug.LogError("Database connection is not open.");
            return;
        }

        try
        {
            dbcmd = dbconn.CreateCommand(); // Initialize dbcmd with a new command object.

            // Transportation table
            sqlQuery = "CREATE TABLE IF NOT EXISTS [transportation_data] (" +
                        "[id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT," +
                        "[type] TEXT NOT NULL," +
                        "[line_name] TEXT NOT NULL," +
                        "[mode_id] INTEGER NOT NULL," +
                        "[route] TEXT NOT NULL," +
                        "[satisfaction_weight] FLOAT NOT NULL)";
            dbcmd.CommandText = sqlQuery;
            dbcmd.ExecuteScalar();

            // District table
            sqlQuery = "CREATE TABLE IF NOT EXISTS [district_data] (" +
                        "[id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT," +
                        "[name] TEXT NOT NULL ," +
                        "[industry_amount] FLOAT NOT NULL," +
                        "[residential_amount] FLOAT NOT NULL," +
                        "[commercial_amount] FLOAT NOT NULL," +
                        "[desired_industry_amount] FLOAT NOT NULL," +
                        "[desired_residential_amount] FLOAT NOT NULL," +
                        "[desired_commercial_amount] FLOAT NOT NULL," +
                        "[characteristics] INT NOT NULL," +
                        "[inhabitants] INT NOT NULL," +
                        "[satisfaction] FLOAT NOT NULL)";
            dbcmd.CommandText = sqlQuery;
            dbcmd.ExecuteScalar();

            // Population data table
            sqlQuery = "CREATE TABLE IF NOT EXISTS [population_data] (" +
                        "[id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT," +
                        "[type] TEXT NOT NULL," +
                        "[departure] TEXT NOT NULL," +
                        "[destination] TEXT NOT NULL," +
                        "[best_time] INT NOT NULL," +
                        "[actual_time] INT NOT NULL," +
                        "[transportation_need] BOOL NOT NULL," +
                        "[satisfaction] FLOAT NOT NULL)";
            dbcmd.CommandText = sqlQuery;
            dbcmd.ExecuteScalar();
        }
        catch (Exception ex)
        {
            Debug.LogError("Database error: " + ex.Message);
        }
        finally
        {
            // Clean up
            if (dbcmd != null)
            {
                dbcmd.Dispose();
            }
        }
    }


    public string ReadFromTable()
    {
       string databaseContent = "";
        try
        {
            using (var cmd = dbconn.CreateCommand())
            {
               
                dbConnCount += 1;
                dbcmd = dbconn.CreateCommand();

                sqlQuery = "SELECT * FROM transportation_data"; // SQL query to select all data from my_table.
                dbcmd.CommandText = sqlQuery;

                using (IDataReader reader = dbcmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int id = reader.GetInt32(0); // Read the integer value from the first column (id).
                        string type = reader.GetString(1); // Read the string value from the second column (name).
                        int mode_id = reader.GetInt32(2); // Read the integer value from the third column (age).
                        string route = reader.GetString(3);
                        float sat = reader.GetFloat(4);

                        //Debug.Log($"ID: {id}, Type: {type}, ModeId: {mode_id}, route: {route}, sat: {sat}"); // Log the row's data.
                        databaseContent = $"ID: {id}, Type: {type}, ModeId: {mode_id}, route: {route}, sat: {sat}";
                    }

                    reader.Close(); // Close the data reader.
                }

                dbConnCount -= 1;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Database reading error: " + ex.Message);
        }
        return databaseContent;

    }

    //condition table name
    public void InsertInTable()
    {
        try
        {
         
                using (var cmd = dbconn.CreateCommand())
                {
                    cmd.CommandText = "INSERT INTO my_table (name, age) VALUES ('John Doe', 30)";
                    cmd.ExecuteNonQuery();
                }
               // ReadFromTable();
                dbConnCount -= 1;
            
        }
        catch (Exception ex)
        {
            Debug.LogError("Database insertion error: " + ex.Message);
        }


    }

    public void InsertInTransport(string type, string line_name, int mode_id, string route, int satisfaction_weight)
    {
       
            dbConnCount += 1;
            using (var cmd = dbconn.CreateCommand())
            {
                cmd.CommandText = "INSERT INTO transportation_data (type, line_name, mode_id, route, satisfaction_weight) VALUES (@type, @line_name, @mode_id, @route, @satisfaction_weight)";
                cmd.Parameters.Add(new SqliteParameter("@type", type));
                cmd.Parameters.Add(new SqliteParameter("@line_name", line_name));
                cmd.Parameters.Add(new SqliteParameter("@mode_id", mode_id));
                cmd.Parameters.Add(new SqliteParameter("@route", route));
                cmd.Parameters.Add(new SqliteParameter("@satisfaction_weight", satisfaction_weight));
                cmd.ExecuteNonQuery();
            }
            //ReadFromTable();
            dbConnCount -= 1;
        
    }


    public void InsertInDistrict(string name, float industry_amount, float residential_amount, float commercial_amount, float desired_industry_amount, float desired_residential_amount, float desired_commercial_amount, string characteristics, int inhabitants, float satisfaction)
    {
        
            dbConnCount += 1;
        using (var cmd = dbconn.CreateCommand())
        {
                cmd.CommandText = "INSERT INTO district_data (name, industry_amount, residential_amount, commercial_amount, desired_industry_amount, desired_residential_amount, desired_commercial_amount, characteristics, inhabitants, satisfaction) " +
                    "VALUES (@name, @industry_amount, @residential_amount, @commercial_amount,commercial_amount, @desired_industry_amount, @desired_residential_amount, @desired_commercial_amount, @characteristics @inhabitants, satisfaction)";
                cmd.Parameters.Add(new SqliteParameter("@name", name));
                cmd.Parameters.Add(new SqliteParameter("@industry_amount", industry_amount));
                cmd.Parameters.Add(new SqliteParameter("@residential_amount", residential_amount));
                cmd.Parameters.Add(new SqliteParameter("@commercial_amount", commercial_amount));
                cmd.Parameters.Add(new SqliteParameter("@desired_industry_amount", desired_industry_amount));
                cmd.Parameters.Add(new SqliteParameter("@desired_residential_amount", desired_residential_amount));
                cmd.Parameters.Add(new SqliteParameter("@desired_commercial_amount", desired_commercial_amount));
                cmd.Parameters.Add(new SqliteParameter("@characteristics", characteristics));
                cmd.Parameters.Add(new SqliteParameter("@inhabitants", inhabitants));
                cmd.Parameters.Add(new SqliteParameter("@satisfaction", satisfaction));
                cmd.ExecuteNonQuery();
         }
            //ReadFromTable();
            dbConnCount -= 1;
        

        sqlQuery = "CREATE TABLE IF NOT EXISTS [district_data] (" +
                         "[id] INTEGER  NOT NULL PRIMARY KEY AUTOINCREMENT," +
                         "[name] TEXT  NOT NULL ," +
                         "[industry_amount] FLOAT  NOT NULL," +
                         "[residential_amount] FLOAT  NOT NULL," +
                         "[commercial_amount] FLOAT  NOT NULL," +
                         "[inhabitants] INT  NOT NULL," +
                         "[satisfaction] FLOAT  NOT NULL)";
    }



    public void ResetTables()
    {
        try
        {
            using (var cmd = dbconn.CreateCommand())
            {
                    cmd.CommandText = "DELETE FROM transportation_data; DELETE FROM district_data; DELETE FROM population_data;";
                    cmd.ExecuteNonQuery();
            }
            
        }
        catch (Exception ex)
        {
            Debug.LogError("Database population insert error: " + ex.Message);
        }

    }


    public class TransportationData
    {
        public int Id;
        public string Type;
        public int ModeId;
        public string Route;
        public float SatisfactionWeight;
    }

    public List<TransportationData> ReadFromTablePrototype()
    {
        List<TransportationData> dataList = new List<TransportationData>();

        try
        {
            using (var cmd = dbconn.CreateCommand())
            {
                dbConnCount += 1;
                dbcmd = dbconn.CreateCommand();
                sqlQuery = "SELECT * FROM transportation_data";
                dbcmd.CommandText = sqlQuery;

                using (IDataReader reader = dbcmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        TransportationData data = new TransportationData
                        {
                            Id = reader.GetInt32(0),
                            Type = reader.GetString(1),
                            ModeId = reader.GetInt32(2),
                            Route = reader.GetString(3),
                            SatisfactionWeight = reader.GetFloat(4)
                        };
                        dataList.Add(data);
                    }
                    reader.Close();
                }
                dbConnCount -= 1;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Database population insert error: " + ex.Message);
        }
        return dataList;
    }


    public void DeleteTransportationLine(string line_name)
    {
        try
        {
            using (var cmd = dbconn.CreateCommand())
            {
                // Prepare the DELETE statement to remove the line
                cmd.CommandText = "DELETE FROM transportation_data WHERE line_name = @line_name;";
                cmd.Parameters.Add(new SqliteParameter("@line_name", line_name));

                // Execute the command
                cmd.ExecuteNonQuery();

                Debug.Log("Line deleted successfully from the database.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Error deleting transportation line from database: " + ex.Message);
        }
    }


    public void UpdateTransportRoute(string type, int modeId, string route, int satisfactionWeight)
    {
        try
        {
        
                dbConnCount += 1;
                using (var cmd = dbconn.CreateCommand())
                {
                    cmd.CommandText = "UPDATE transportation_data SET route = @route, satisfaction_weight = @satisfactionWeight WHERE type = @type AND mode_id = @modeId";
                    cmd.Parameters.Add(new SqliteParameter("@route", route));
                    cmd.Parameters.Add(new SqliteParameter("@satisfactionWeight", satisfactionWeight));
                    cmd.Parameters.Add(new SqliteParameter("@type", type));
                    cmd.Parameters.Add(new SqliteParameter("@modeId", modeId));
                    cmd.ExecuteNonQuery();
                }
                dbConnCount -= 1;
            
        }
        catch (Exception ex)
        {
            Debug.LogError("Database population insert error: " + ex.Message);
        }
        //ReadFromTable();
    }

    //Insert Data into District Table
    public IEnumerator InsertDistrictData(DistrictData data)
    {
        try
        {
      
                dbConnCount += 1;
                using (var cmd = dbconn.CreateCommand())
                {
                    cmd.CommandText = "INSERT INTO district_data (name, industry_amount, residential_amount, commercial_amount, desired_industry_amount, desired_residential_amount, desired_commercial_amount, characteristics, inhabitants, satisfaction) " +
                                      "VALUES (@name, @industry_amount, @residential_amount, @commercial_amount, @desired_industry_amount, @desired_residential_amount, @desired_commercial_amount, @characteristics, @inhabitants, @satisfaction)";
                    cmd.Parameters.Add(new SqliteParameter("@name", data.DistrictName));
                    cmd.Parameters.Add(new SqliteParameter("@industry_amount", data.IndustrialAmount));
                    cmd.Parameters.Add(new SqliteParameter("@residential_amount", data.ResidentialAmount));
                    cmd.Parameters.Add(new SqliteParameter("@commercial_amount", data.CommercialAmount));
                    cmd.Parameters.Add(new SqliteParameter("@desired_industry_amount", data.DesiredIndustrialAmount));
                    cmd.Parameters.Add(new SqliteParameter("@desired_residential_amount", data.DesiredResidentialAmount));
                    cmd.Parameters.Add(new SqliteParameter("@desired_commercial_amount", data.DesiredCommercialAmount));
                    cmd.Parameters.Add(new SqliteParameter("@characteristics", data.Characteristic));
                    cmd.Parameters.Add(new SqliteParameter("@inhabitants", data.Population));
                    cmd.Parameters.Add(new SqliteParameter("@satisfaction", data.Satisfaction));
                    cmd.ExecuteNonQuery();
                }
                dbConnCount -= 1;
            
        }
        catch (Exception ex)
        {
            Debug.LogError("Database insertion error: " + ex.Message);
        }
        LoadDistrictNames(); // Reload district names to update the list

        yield return null; //Is needed for coroutines
    }

    public void InsertPopulation(string type, string departure, string destination, float best_time, float actual_time, bool transportation_need, float satisfaction)
    {
        try
        {
          
                using (var cmd = dbconn.CreateCommand())
                {
                    cmd.CommandText = "INSERT INTO population_data (type,departure, destination, best_time, actual_time, transportation_need, satisfaction) " +
                                        "VALUES (@type, @departure, @destination, @best_time, @actual_time, @transportation_need, @satisfaction)";
                    cmd.Parameters.Add(new SqliteParameter("@type", type));
                    cmd.Parameters.Add(new SqliteParameter("@departure", departure));
                    cmd.Parameters.Add(new SqliteParameter("@destination", destination));
                    cmd.Parameters.Add(new SqliteParameter("@best_time", best_time));
                    cmd.Parameters.Add(new SqliteParameter("@actual_time", actual_time));
                    cmd.Parameters.Add(new SqliteParameter("@transportation_need", transportation_need));
                    cmd.Parameters.Add(new SqliteParameter("@satisfaction", satisfaction));
                    cmd.ExecuteNonQuery();
                }
                //ReadFromTable();
                dbConnCount -= 1;
            
        }
        catch (Exception ex)
        {
            Debug.LogError("Database population insert error: " + ex.Message);
        }

    }

    public void UpdatePopulationDatabase(string departure, string destination, float actual_time, float satisfaction)
    {
        try
        {
            using (var cmd = dbconn.CreateCommand())
            {
                sqlQuery = @"
                UPDATE population_data 
                SET 
                    actual_time = @actual_time, 
                    satisfaction = @satisfaction
                WHERE departure = @departure AND destination = @destination
                    AND transportation_need = true";
                cmd.CommandText = sqlQuery;

                cmd.Parameters.Add(new SqliteParameter("@actual_time", actual_time));
                cmd.Parameters.Add(new SqliteParameter("@satisfaction", satisfaction));
                cmd.Parameters.Add(new SqliteParameter("@departure", departure));
                cmd.Parameters.Add(new SqliteParameter("@destination", destination));

                cmd.ExecuteNonQuery();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Database update error: " + ex.Message);
        }
    }


    //Load the district names and write them to the list
    public void LoadDistrictNames()
    {
        try
        {
   
                using (var cmd = dbconn.CreateCommand())
                {
                    cmd.CommandText = "SELECT name FROM district_data";
                    using (var reader = cmd.ExecuteReader())
                    {
                        districtNames.Clear(); // Clear existing items
                        while (reader.Read())
                        {
                            string districtName = reader.GetString(0); 
                            districtNames.Add(districtName);
                        }
                    }
                }
            
        }
        catch (Exception ex)
        {
            Debug.LogError("Error loading district names: " + ex.Message);
        }
        //string allDistrictNames = string.Join(", ", districtNames);
        //Debug.Log(allDistrictNames);
    }

    public void UpdateDistrictAmounts(string districtName, float industrialChange, float commercialChange, float residentialChange)
    {
        try
        {
            using (var cmd = dbconn.CreateCommand())
            {
                dbConnCount += 1;
                cmd.CommandText = "UPDATE district_data SET industry_amount = industry_amount + @industrialChange, commercial_amount = commercial_amount + @commercialChange, residential_amount = residential_amount + @residentialChange WHERE name = @districtName";

                cmd.Parameters.Add(new SqliteParameter("@industrialChange", industrialChange));
                cmd.Parameters.Add(new SqliteParameter("@districtName", districtName));
                cmd.Parameters.Add(new SqliteParameter("@commercialChange", commercialChange));
                cmd.Parameters.Add(new SqliteParameter("@residentialChange", residentialChange));

                cmd.ExecuteNonQuery();
                dbConnCount -= 1;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Database update error: " + ex.Message);
        }
    }


    //Update the population count of a district if it's the departure state of a new inhabitant
    public void UpdateDistrictPopulation(string districtName, int populationIncrease)
    {
        try
        {
           
                using (var cmd = dbconn.CreateCommand())
                {
                    cmd.CommandText = "UPDATE district_data SET inhabitants = inhabitants + @populationIncrease WHERE name = @name";
                    cmd.Parameters.Add(new SqliteParameter("@populationIncrease", populationIncrease));
                    cmd.Parameters.Add(new SqliteParameter("@name", districtName));
                    cmd.ExecuteNonQuery();
                }
            
        }
        catch (Exception ex)
        {
            Debug.LogError("Error updating district population: " + ex.Message);
        }
    }

    public int GetDistrictPopulation(string districtName)
    {
        int population = 0;
        try
        {
                dbConnCount += 1;
                using (var cmd = dbconn.CreateCommand())
                {
                    cmd.CommandText = "SELECT inhabitants FROM district_data WHERE name = @name";
                    cmd.Parameters.Add(new SqliteParameter("@name", districtName));

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            population = reader.GetInt32(0); 
                        }
                    }
                }
                dbConnCount -= 1;
        }
        catch (Exception ex)
        {
            Debug.LogError("Error retrieving district population: " + ex.Message);
        }
       
        return population;
    }

    //public float GetOverallAverageSat()
    //{
    //    float satisfaction_avg = 0;
    //    try
    //    {
    //            using (var cmd = dbconn.CreateCommand())
    //            {
    //                // Corrected variable names
    //                string sqlQuery = "SELECT AVG(satisfaction) AS average_satisfaction FROM population_data WHERE transportation_need = true";
    //                cmd.CommandText = sqlQuery;

    //                using (var reader = cmd.ExecuteReader())
    //                {
    //                    if (reader.Read())
    //                    {
    //                        // Corrected data type
    //                        satisfaction_avg = reader.GetFloat(0); // Using GetFloat for a floating-point number
    //                    }
    //                }
    //            }

    //    }
    //    catch (Exception ex)
    //    {
    //        Debug.LogError("Error retrieving overall average satisfaction: " + ex.Message);
    //    }
    //    return satisfaction_avg;
    //}

    public float GetOverallAverageSat()
    {
        float satisfaction_avg = 0;
        try
        {
            using (var cmd = dbconn.CreateCommand())
            {
                string sqlQuery = "SELECT AVG(satisfaction) AS average_satisfaction FROM population_data WHERE transportation_need = true";
                cmd.CommandText = sqlQuery;

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read() && !reader.IsDBNull(0))
                    {
                        satisfaction_avg = reader.GetFloat(0);
                    }
                    // else - satisfaction_avg remains 0
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Error retrieving overall average satisfaction: " + ex.Message);
        }
        return satisfaction_avg;
    }



    public float GetDistrictSatisfaction(string districtName)
    {
        float satisfactionScore = 0f;
        try
        {
            using (var cmd = dbconn.CreateCommand())
            {
                string sqlQuery = "SELECT satisfaction FROM district_data WHERE name = @districtName";
                cmd.Parameters.Add(new SqliteParameter("@districtName", districtName));
                cmd.CommandText = sqlQuery;

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read() && !reader.IsDBNull(0))
                    {
                        satisfactionScore = reader.GetFloat(0);
                    }
                    // else - satisfaction_avg remains 0
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Error retrieving overall average satisfaction: " + ex.Message);
        }
        return satisfactionScore;
    }



    public Dictionary<string, DistrictDataDistribution> FetchCurrentDistrictRatios()
    {
        var districtDataDict = new Dictionary<string, DistrictDataDistribution>();

        try
        {
            using (var cmd = dbconn.CreateCommand())
            {
                dbConnCount += 1;
                dbcmd = dbconn.CreateCommand();
                sqlQuery = "SELECT name, industry_amount, residential_amount, commercial_amount FROM district_data";
                dbcmd.CommandText = sqlQuery;

                using (var reader = dbcmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string name = reader.GetString(0);
                        float industryAmount = reader.GetFloat(1);
                        float residentialAmount = reader.GetFloat(2);
                        float commercialAmount = reader.GetFloat(3);

                        districtDataDict[name] = new DistrictDataDistribution(industryAmount, residentialAmount, commercialAmount);
                    }
                }
                dbConnCount -= 1;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Database reading error: " + ex.Message);
        }

        return districtDataDict;
    }

    public DistrictDataDistribution FetchDistrictRatiosByName(string districtName)
    {
        DistrictDataDistribution districtData = null;

        try
        {
            using (var cmd = dbconn.CreateCommand())
            {
                dbConnCount += 1;
                cmd.CommandText = "SELECT name, industry_amount, residential_amount, commercial_amount FROM district_data WHERE name = @name";
                cmd.Parameters.Add(new SqliteParameter("@name", districtName));

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read()) 
                    {
                        string name = reader.GetString(0);
                        float industryAmount = reader.GetFloat(1);
                        float residentialAmount = reader.GetFloat(2);
                        float commercialAmount = reader.GetFloat(3);

                        districtData = new DistrictDataDistribution(industryAmount, residentialAmount, commercialAmount);
                    }
                }
                dbConnCount -= 1;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Database reading error: " + ex.Message);
        }

        return districtData;
    }

    public Dictionary<string, DesiredDistrictDataDistribution> FetchDesiredDistrictRatios()
    {
        var districtDataDict = new Dictionary<string, DesiredDistrictDataDistribution>();

        try
        {
            using (var cmd = dbconn.CreateCommand())
            {
                sqlQuery = "SELECT name, desired_industry_amount, desired_residential_amount, desired_commercial_amount FROM district_data";
                cmd.CommandText = sqlQuery;

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string name = reader.GetString(0);
                        float desired_industryAmount = reader.GetFloat(1);
                        float desired_residentialAmount = reader.GetFloat(2);
                        float desired_commercialAmount = reader.GetFloat(3);

                        districtDataDict[name] = new DesiredDistrictDataDistribution(desired_industryAmount, desired_residentialAmount, desired_commercialAmount);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Database reading error: " + ex.Message);
        }

        return districtDataDict;
    }

    public void UpdateDistrictDesiredRatios(string districtName, DesiredDistrictDataDistribution data)
    {
        try
        {
            using (var cmd = dbconn.CreateCommand())
            {
                sqlQuery = @"
                UPDATE district_data 
                SET 
                    desired_industry_amount = @desiredIndustryAmount, 
                    desired_residential_amount = @desiredResidentialAmount, 
                    desired_commercial_amount = @desiredCommercialAmount
                WHERE name = @name";
                cmd.CommandText = sqlQuery;

                cmd.Parameters.Add(new SqliteParameter("@desiredIndustryAmount", data.DesiredIndustryAmount));
                cmd.Parameters.Add(new SqliteParameter("@desiredResidentialAmount", data.DesiredResidentialAmount));
                cmd.Parameters.Add(new SqliteParameter("@desiredCommercialAmount", data.DesiredCommercialAmount));
                cmd.Parameters.Add(new SqliteParameter("@name", districtName));

                cmd.ExecuteNonQuery();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Database update error: " + ex.Message);
        }
    }

    public void UpdateDistrictSatisfactionScore()
    {
        try
        {
            using (var cmd = dbconn.CreateCommand())
            {
                cmd.CommandText = @"
                UPDATE district_data
                SET satisfaction = (
                    SELECT AVG(i.satisfaction)
                    FROM population_data i
                    WHERE i.departure = district_data.name
                        AND i.transportation_need = true
                )
                WHERE EXISTS (
                    SELECT 1
                    FROM population_data i
                    WHERE i.departure = district_data.name
                        AND i.transportation_need = true
                );";

                // No parameters are needed since the query does not use external inputs
                cmd.ExecuteNonQuery();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Error updating district satisfaction score: " + ex.Message);
        }
    }

    public (string districtName, float difference) GetDistrictWithHighestResidentialDifference()
    {
        string districtName = "";
        float maxDifference = 0;

        try
        {

                using (var cmd = dbconn.CreateCommand())
                {
                    // Get the difference for each district and select the one with the maximum difference
                    string sqlQuery = @"
                    SELECT name, ABS(desired_residential_amount - residential_amount) AS difference
                    FROM district_data
                    WHERE desired_residential_amount > 0 AND residential_amount < desired_residential_amount
                    ORDER BY difference DESC
                    LIMIT 1";

                    cmd.CommandText = sqlQuery;

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            districtName = reader.GetString(0); 
                            maxDifference = reader.GetFloat(1); 
                        }
                    }
                }
            
        }
        catch (Exception ex)
        {
            Debug.LogError("Error finding district with highest residential difference: " + ex.Message);
        }

        return (districtName, maxDifference);
    }


    public (string, float) GetDistrictWithHighestCommercialDifference()
    {
        string districtName = "";
        float maxDifference = 0;

        try
        {

            using (var cmd = dbconn.CreateCommand())
            {
                // Get the difference for each district and select the one with the maximum difference
                string sqlQuery = @"
                SELECT name, 
                       (ABS(desired_commercial_amount - commercial_amount) / desired_commercial_amount) * 100 AS percentage_difference
                FROM district_data
                WHERE desired_commercial_amount > 0 AND commercial_amount < desired_commercial_amount
                ORDER BY percentage_difference DESC
                LIMIT 1";

                cmd.CommandText = sqlQuery;

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        districtName = reader.GetString(0);
                        maxDifference = reader.GetFloat(1);
                    }
                }
            }

        }
        catch (Exception ex)
        {
            Debug.LogError("Error finding district with highest residential difference: " + ex.Message);
        }

        return (districtName, maxDifference);
    }


    public (string, float) GetDistrictWithHighestIndustrialDifference()
    {
        string districtName = "";
        float maxDifference = 0;

        try
        {

            using (var cmd = dbconn.CreateCommand())
            {
                // Get the difference for each district and select the one with the maximum difference
                string sqlQuery = @"
                SELECT name, 
                       (ABS(desired_industry_amount - industry_amount) / desired_industry_amount) * 100 AS percentage_difference
                FROM district_data
                WHERE desired_industry_amount > 0 AND industry_amount < desired_industry_amount
                ORDER BY percentage_difference DESC
                LIMIT 1";

                cmd.CommandText = sqlQuery;

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        districtName = reader.GetString(0);
                        maxDifference = reader.GetFloat(1);
                    }
                }
            }

        }
        catch (Exception ex)
        {
            Debug.LogError("Error finding district with highest residential difference: " + ex.Message);
        }

        return (districtName, maxDifference);
    }




    public List<(string destinationDistrict, int inhabitantsCount, float averageSatisfaction)> GetDestinationDistrictsFromSpecifiedDeparture(string departure)
    {
        var results = new List<(string destinationDistrict, int inhabitantsCount, float averageSatisfaction)>();

        try
        {
            using (var cmd = dbconn.CreateCommand())
            {
                // Define SQL query to get destination district, count of inhabitants, and average satisfaction score
                // for a specified departure district, grouped by destination district.
                string sqlQuery = @"
                SELECT 
                    destination, 
                    COUNT(id) AS inhabitants_count, 
                    AVG(satisfaction) AS average_satisfaction
                FROM 
                    population_data 
                WHERE 
                    departure = @departure AND transportation_need = true
                GROUP BY 
                    destination 
                ORDER BY 
                    inhabitants_count DESC, average_satisfaction DESC";

                cmd.CommandText = sqlQuery;

                // Properly add the parameter to avoid SQL injection
                var param = cmd.CreateParameter();
                param.ParameterName = "@departure";
                param.Value = departure;
                cmd.Parameters.Add(param);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var destinationDistrict = reader.GetString(0);
                        var inhabitantsCount = reader.GetInt32(1);
                        var averageSatisfaction = reader.GetFloat(2);

                        results.Add((destinationDistrict, inhabitantsCount, averageSatisfaction));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error querying destination districts from '{departure}': {ex.Message}");
        }

        return results;

    }


// Update is called once per frame
void Update()
    {
        //Debug.Log(dbConnCount);
    }
}
