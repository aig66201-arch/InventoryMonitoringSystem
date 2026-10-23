using Microsoft.Data.SqlClient;
using InventoryMonitoringSystem.Services;

namespace InventoryMonitoringSystem.Data
{
    public static class DatabaseInitializer
    {
        public static void Initialize()
        {
            CreateDatabaseIfNotExists();
            CreateTablesIfNotExists();
            UpdateUsersTableIfNeeded();
            MedicineService.EnsureSchema();
        }

        private static void CreateDatabaseIfNotExists()
        {
            using SqlConnection connection = new(DatabaseConnection.MasterConnectionString);
            connection.Open();

            const string sql = @"
IF DB_ID(N'InventoryMonitoringSystemDB') IS NULL
BEGIN
    CREATE DATABASE InventoryMonitoringSystemDB;
END";

            using SqlCommand command = new(sql, connection);
            command.ExecuteNonQuery();
        }

        private static void CreateTablesIfNotExists()
        {
            using SqlConnection connection = new(DatabaseConnection.DatabaseConnectionString);
            connection.Open();

            const string sql = @"
IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users
    (
        UserID INT IDENTITY(1,1) NOT NULL,
        Username NVARCHAR(50) NOT NULL,
        PasswordHash NVARCHAR(255) NOT NULL,
        Role NVARCHAR(20) NOT NULL,
        FirstName NVARCHAR(100) NULL,
        LastName NVARCHAR(100) NULL,
        Email NVARCHAR(150) NULL,
        ContactNumber NVARCHAR(50) NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
        CONSTRAINT PK_Users PRIMARY KEY (UserID),
        CONSTRAINT UQ_Users_Username UNIQUE (Username),
        CONSTRAINT CK_Users_Role CHECK (Role IN ('Administrator', 'User'))
    );
END;

";

            using SqlCommand command = new(sql, connection);
            command.ExecuteNonQuery();
        }

        private static void UpdateUsersTableIfNeeded()
        {
            using SqlConnection connection = new(DatabaseConnection.DatabaseConnectionString);
            connection.Open();

            const string sql = @"
IF COL_LENGTH('dbo.Users', 'FirstName') IS NULL
BEGIN
    ALTER TABLE dbo.Users ADD FirstName NVARCHAR(100) NULL;
END;

IF COL_LENGTH('dbo.Users', 'LastName') IS NULL
BEGIN
    ALTER TABLE dbo.Users ADD LastName NVARCHAR(100) NULL;
END;

IF COL_LENGTH('dbo.Users', 'Email') IS NULL
BEGIN
    ALTER TABLE dbo.Users ADD Email NVARCHAR(150) NULL;
END;

IF COL_LENGTH('dbo.Users', 'ContactNumber') IS NULL
BEGIN
    ALTER TABLE dbo.Users ADD ContactNumber NVARCHAR(50) NULL;
END;";

            using SqlCommand command = new(sql, connection);
            command.ExecuteNonQuery();
        }
    }
}
