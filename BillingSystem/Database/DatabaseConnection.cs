using System;
using MySql.Data.MySqlClient;

namespace BillingSystem.Database
{
    // This class provides a reusable database connection.
    public class DatabaseConnection
    {
        // Connection string settings — update Password if needed
        private const string SERVER = "localhost";
        private const string DATABASE = "BillingDB";
        private const string UID = "root";
        private const string PASSWORD = "App_Dev_@Grp6"; // Update inside quotes if your password differs

        private static string ConnectionString =>
            $"server={SERVER};database={DATABASE};uid={UID};pwd={PASSWORD};";

        // Returns an open-ready MySqlConnection object.
        public static MySqlConnection GetConnection()
        {
            return new MySqlConnection(ConnectionString);
        }

        // Returns true if the app can connect to MySQL.
        public static bool TestConnection()
        {
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}