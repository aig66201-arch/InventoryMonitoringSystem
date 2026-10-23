using Microsoft.Data.SqlClient;

namespace InventoryMonitoringSystem.Data
{
    public static class DatabaseConnection
    {
        private const string ServerName = @"(localdb)\MSSQLLocalDB";

        private const string DatabaseName =
            "InventoryMonitoringSystemDB";

        public static string MasterConnectionString =>
            $"Server={ServerName};" +
            $"Integrated Security=True;" +
            $"TrustServerCertificate=True;" +
            $"Database=master;";

        public static string DatabaseConnectionString =>
            $"Server={ServerName};" +
            $"Integrated Security=True;" +
            $"TrustServerCertificate=True;" +
            $"Database={DatabaseName};";
    }
}
