using System;
using MySql.Data.MySqlClient;

namespace BillingSystem.Database
{
    // This class provides a reusable database connection.
    public class DatabaseConnection
    {
        // Connection string settings — updated for remote network access
        private const string SERVER = "10.0.0.32";
        private const string DATABASE = "billingdb";
        private const string UID = "billing_user";
        private const string PASSWORD = "BillingPass123!";

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