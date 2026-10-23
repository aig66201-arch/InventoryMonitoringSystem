using System.Data;
using Microsoft.Data.SqlClient;
using InventoryMonitoringSystem.Data;

namespace InventoryMonitoringSystem.Services;

/// <summary>
/// Database access for the Medicine Catalog.
/// The service is intentionally schema-tolerant so older InventoryMonitoringSystemDB
/// databases can be opened and upgraded without requiring the user to delete data.
/// </summary>
public static class MedicineService
{
    private static readonly object SchemaLock = new();
    private static bool schemaReady;
    private static string? medicineTableName;

    public static void EnsureSchema()
    {
        if (schemaReady) return;

        lock (SchemaLock)
        {
            if (schemaReady) return;

            using SqlConnection connection = new(DatabaseConnection.DatabaseConnectionString);
            connection.Open();

            medicineTableName = FindOrCreateMedicineTable(connection);
            UpgradeMedicineTable(connection, medicineTableName);
            schemaReady = true;
        }
    }

    private static string FindOrCreateMedicineTable(SqlConnection connection)
    {
        bool medicines = TableExists(connection, "Medicines");
        bool medicine = TableExists(connection, "Medicine");

        if (medicines && medicine)
        {
            int medicinesRows = CountRows(connection, "Medicines");
            int medicineRows = CountRows(connection, "Medicine");
            return medicineRows > 0 && medicinesRows == 0 ? "Medicine" : "Medicines";
        }

        if (medicines) return "Medicines";
        if (medicine) return "Medicine";

        ExecuteNonQuery(connection, @"
CREATE TABLE dbo.Medicines
(
    Med_ID NVARCHAR(20) NOT NULL,
    MedicineName NVARCHAR(150) NOT NULL CONSTRAINT DF_Medicines_MedicineName DEFAULT N'',
    Generic_Name NVARCHAR(150) NOT NULL CONSTRAINT DF_Medicines_GenericName DEFAULT N'',
    Brand_Name NVARCHAR(150) NULL,
    Dosage NVARCHAR(50) NOT NULL CONSTRAINT DF_Medicines_Dosage DEFAULT N'',
    Dosage_Form NVARCHAR(50) NOT NULL CONSTRAINT DF_Medicines_DosageForm DEFAULT N'',
    Unit NVARCHAR(50) NOT NULL CONSTRAINT DF_Medicines_Unit DEFAULT N'',
    Reorder_Level INT NOT NULL CONSTRAINT DF_Medicines_ReorderLevel DEFAULT 50,
    IsActive BIT NOT NULL CONSTRAINT DF_Medicines_IsActive DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Medicines_CreatedAt DEFAULT SYSDATETIME(),
    CONSTRAINT PK_Medicines PRIMARY KEY (Med_ID)
);");

        return "Medicines";
    }

    private static void UpgradeMedicineTable(SqlConnection connection, string tableName)
    {
        // IMPORTANT: each schema change is executed separately. SQL Server can compile
        // a whole ALTER/UPDATE batch before the newly-added column exists, which caused
        // the previous "Invalid column name 'Med_ID'" failure.
        string safeTable = QuoteIdentifier(tableName);

        EnsureColumn(connection, tableName, "Med_ID", "NVARCHAR(20) NULL");
        EnsureColumn(connection, tableName, "MedicineName", "NVARCHAR(150) NOT NULL CONSTRAINT " + DefaultName(tableName, "MedicineName") + " DEFAULT N''");
        EnsureColumn(connection, tableName, "Generic_Name", "NVARCHAR(150) NOT NULL CONSTRAINT " + DefaultName(tableName, "GenericName") + " DEFAULT N''");
        EnsureColumn(connection, tableName, "Brand_Name", "NVARCHAR(150) NULL");
        EnsureColumn(connection, tableName, "Dosage", "NVARCHAR(50) NOT NULL CONSTRAINT " + DefaultName(tableName, "Dosage") + " DEFAULT N''");
        EnsureColumn(connection, tableName, "Dosage_Form", "NVARCHAR(50) NOT NULL CONSTRAINT " + DefaultName(tableName, "DosageForm") + " DEFAULT N''");
        EnsureColumn(connection, tableName, "Unit", "NVARCHAR(50) NOT NULL CONSTRAINT " + DefaultName(tableName, "Unit") + " DEFAULT N''");
        EnsureColumn(connection, tableName, "Reorder_Level", "INT NOT NULL CONSTRAINT " + DefaultName(tableName, "ReorderLevel") + " DEFAULT 50");
        EnsureColumn(connection, tableName, "IsActive", "BIT NOT NULL CONSTRAINT " + DefaultName(tableName, "IsActive") + " DEFAULT 1");
        EnsureColumn(connection, tableName, "CreatedAt", "DATETIME2 NOT NULL CONSTRAINT " + DefaultName(tableName, "CreatedAt") + " DEFAULT SYSDATETIME()");

        // Existing databases commonly use MedicineName as their required name field.
        // Synchronize it into the application's Generic_Name field without deleting it.
        if (ColumnExists(connection, tableName, "MedicineName"))
        {
            ExecuteNonQuery(connection, $@"
UPDATE {safeTable}
SET Generic_Name = MedicineName
WHERE (Generic_Name IS NULL OR LTRIM(RTRIM(Generic_Name)) = N'')
  AND MedicineName IS NOT NULL AND LTRIM(RTRIM(MedicineName)) <> N'';");
        }

        // Fill missing IDs before making the newly-created ID column NOT NULL.
        ExecuteNonQuery(connection, $@"
;WITH MissingIds AS
(
    SELECT Med_ID,
           ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS RowNumber
    FROM {safeTable}
    WHERE Med_ID IS NULL OR LTRIM(RTRIM(Med_ID)) = N''
)
UPDATE MissingIds
SET Med_ID = N'M-' + RIGHT(N'0000' + CONVERT(NVARCHAR(10), RowNumber), 4);");

        ExecuteNonQuery(connection, $"UPDATE {safeTable} SET Med_ID = N'M-' + RIGHT(N'0000' + CONVERT(NVARCHAR(10), ABS(CHECKSUM(NEWID())) % 10000), 4) WHERE Med_ID IS NULL OR LTRIM(RTRIM(Med_ID)) = N'';");

        // Make Med_ID required only after every row has a value.
        ExecuteNonQuery(connection, $"ALTER TABLE {safeTable} ALTER COLUMN Med_ID NVARCHAR(20) NOT NULL;");
    }

    private static void EnsureColumn(SqlConnection connection, string tableName, string columnName, string definition)
    {
        if (ColumnExists(connection, tableName, columnName)) return;

        string table = QuoteIdentifier(tableName);
        string column = QuoteIdentifier(columnName);
        ExecuteNonQuery(connection, $"ALTER TABLE {table} ADD {column} {definition};");
    }

    private static string DefaultName(string tableName, string suffix)
    {
        string prefix = tableName.Equals("Medicine", StringComparison.OrdinalIgnoreCase) ? "Medicine" : "Medicines";
        return $"DF_{prefix}_{suffix}";
    }

    private static string QuoteIdentifier(string name) => "[" + name.Replace("]", "]]", StringComparison.Ordinal) + "]";

    public static DataTable GetAllMedicines()
    {
        EnsureSchema();
        DataTable table = new("Medicines");

        using SqlConnection connection = new(DatabaseConnection.DatabaseConnectionString);
        connection.Open();
        string tableName = TableName;

        const string sqlTemplate = @"
SELECT Med_ID, Generic_Name, Brand_Name, Dosage, Dosage_Form, Unit, Reorder_Level
FROM {0}
WHERE IsActive = 1
ORDER BY Med_ID ASC;";

        using SqlCommand command = new(string.Format(sqlTemplate, QuoteIdentifier(tableName)), connection);
        using SqlDataAdapter adapter = new(command);
        adapter.Fill(table);
        return table;
    }

    public static string GenerateNextMedId()
    {
        EnsureSchema();
        using SqlConnection connection = new(DatabaseConnection.DatabaseConnectionString);
        connection.Open();

        // Reuse the first genuinely unused M-#### number instead of always taking MAX + 1.
        // Inactive records that still exist (for example, medicines with movement history)
        // remain reserved to avoid assigning their ID to a different medicine.
        string sql = $"SELECT Med_ID FROM {QuoteIdentifier(TableName)} WHERE Med_ID LIKE N'M-%' OR Med_ID LIKE N'MED-%';";
        var used = new HashSet<int>();
        using (SqlCommand command = new(sql, connection))
        using (SqlDataReader reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                string medId = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
                string numericPart = medId.StartsWith("MED-", StringComparison.OrdinalIgnoreCase)
                    ? medId[4..]
                    : medId.StartsWith("M-", StringComparison.OrdinalIgnoreCase) ? medId[2..] : string.Empty;
                if (int.TryParse(numericPart, out int number) && number > 0)
                    used.Add(number);
            }
        }

        int next = 1;
        while (used.Contains(next)) next++;
        return $"M-{next:D4}";
    }

    public static bool AddMedicine(string medId, string genericName, string brandName, string dosage,
        string dosageForm, string unit, int reorderLevel)
    {
        EnsureSchema();
        using SqlConnection connection = new(DatabaseConnection.DatabaseConnectionString);
        connection.Open();

        string table = QuoteIdentifier(TableName);
        bool hasMedicineName = ColumnExists(connection, TableName, "MedicineName");

        // MedicineName is included specifically for older databases where it is NOT NULL.
        string columns = hasMedicineName
            ? "Med_ID, MedicineName, Generic_Name, Brand_Name, Dosage, Dosage_Form, Unit, Reorder_Level, IsActive"
            : "Med_ID, Generic_Name, Brand_Name, Dosage, Dosage_Form, Unit, Reorder_Level, IsActive";
        string values = hasMedicineName
            ? "@Med_ID, @MedicineName, @Generic_Name, @Brand_Name, @Dosage, @Dosage_Form, @Unit, @Reorder_Level, 1"
            : "@Med_ID, @Generic_Name, @Brand_Name, @Dosage, @Dosage_Form, @Unit, @Reorder_Level, 1";

        string sql = $"INSERT INTO {table} ({columns}) VALUES ({values});";
        using SqlCommand command = new(sql, connection);
        AddParameters(command, medId, genericName, brandName, dosage, dosageForm, unit, reorderLevel, hasMedicineName);
        return command.ExecuteNonQuery() > 0;
    }

    public static bool UpdateMedicine(string medId, string genericName, string brandName, string dosage,
        string dosageForm, string unit, int reorderLevel)
    {
        EnsureSchema();
        using SqlConnection connection = new(DatabaseConnection.DatabaseConnectionString);
        connection.Open();

        string table = QuoteIdentifier(TableName);
        bool hasMedicineName = ColumnExists(connection, TableName, "MedicineName");
        string legacyName = hasMedicineName ? ", MedicineName = @MedicineName" : string.Empty;

        string sql = $@"
UPDATE {table}
SET Generic_Name = @Generic_Name,
    Brand_Name = @Brand_Name,
    Dosage = @Dosage,
    Dosage_Form = @Dosage_Form,
    Unit = @Unit,
    Reorder_Level = @Reorder_Level{legacyName}
WHERE Med_ID = @Med_ID;";

        using SqlCommand command = new(sql, connection);
        AddParameters(command, medId, genericName, brandName, dosage, dosageForm, unit, reorderLevel, hasMedicineName);
        return command.ExecuteNonQuery() > 0;
    }

    public static bool DeleteMedicine(string medId)
    {
        EnsureSchema();
        using SqlConnection connection = new(DatabaseConnection.DatabaseConnectionString);
        connection.Open();
        string table = QuoteIdentifier(TableName);

        // Keep the medicine record inactive if it is referenced by stock movements, so
        // historical transactions remain valid and its ID is not accidentally reassigned.
        bool isReferenced = false;
        foreach (string referenceTable in new[] { "StockMovements", "Batches" })
        {
            if (!TableExists(connection, referenceTable) || !ColumnExists(connection, referenceTable, "Med_ID"))
                continue;
            using SqlCommand referenceCheck = new($"SELECT COUNT(*) FROM {QuoteIdentifier(referenceTable)} WHERE Med_ID = @Med_ID;", connection);
            referenceCheck.Parameters.Add("@Med_ID", SqlDbType.NVarChar, 20).Value = medId.Trim();
            if (Convert.ToInt32(referenceCheck.ExecuteScalar()) > 0)
            {
                isReferenced = true;
                break;
            }
        }

        string sql = isReferenced
            ? $"UPDATE {table} SET IsActive = 0 WHERE Med_ID = @Med_ID;"
            : $"DELETE FROM {table} WHERE Med_ID = @Med_ID;";
        using SqlCommand command = new(sql, connection);
        command.Parameters.Add("@Med_ID", SqlDbType.NVarChar, 20).Value = medId.Trim();
        return command.ExecuteNonQuery() > 0;
    }

    public static (int Total, int LowStock, int ExpiringSoon) GetDashboardMetrics()
    {
        EnsureSchema();
        using SqlConnection connection = new(DatabaseConnection.DatabaseConnectionString);
        connection.Open();

        string table = QuoteIdentifier(TableName);
        int total = ExecuteInt(connection, $"SELECT COUNT(*) FROM {table} WHERE IsActive = 1;");
        int lowStock = 0;

        if (ColumnExists(connection, TableName, "Current_Stock"))
        {
            lowStock = ExecuteInt(connection,
                $"SELECT COUNT(*) FROM {table} WHERE IsActive = 1 AND Current_Stock <= Reorder_Level;");
        }

        int expiringSoon = 0;
        if (TableExists(connection, "Batches") && ColumnExists(connection, "Batches", "ExpiryDate"))
        {
            expiringSoon = ExecuteInt(connection,
                "SELECT COUNT(*) FROM dbo.Batches WHERE ExpiryDate >= CAST(GETDATE() AS date) AND ExpiryDate < DATEADD(day, 31, CAST(GETDATE() AS date));");
        }

        return (total, lowStock, expiringSoon);
    }

    private static void AddParameters(SqlCommand command, string medId, string genericName,
        string brandName, string dosage, string dosageForm, string unit, int reorderLevel, bool includeMedicineName)
    {
        string cleanName = genericName.Trim();
        command.Parameters.Add("@Med_ID", SqlDbType.NVarChar, 20).Value = medId.Trim();
        if (includeMedicineName)
            command.Parameters.Add("@MedicineName", SqlDbType.NVarChar, 150).Value = cleanName;
        command.Parameters.Add("@Generic_Name", SqlDbType.NVarChar, 150).Value = cleanName;
        command.Parameters.Add("@Brand_Name", SqlDbType.NVarChar, 150).Value = string.IsNullOrWhiteSpace(brandName) ? DBNull.Value : brandName.Trim();
        command.Parameters.Add("@Dosage", SqlDbType.NVarChar, 50).Value = dosage.Trim();
        command.Parameters.Add("@Dosage_Form", SqlDbType.NVarChar, 50).Value = dosageForm.Trim();
        command.Parameters.Add("@Unit", SqlDbType.NVarChar, 50).Value = unit.Trim();
        command.Parameters.Add("@Reorder_Level", SqlDbType.Int).Value = reorderLevel;
    }

    private static int CountRows(SqlConnection connection, string tableName)
    {
        using SqlCommand command = new($"SELECT COUNT(*) FROM {QuoteIdentifier(tableName)};", connection);
        return Convert.ToInt32(command.ExecuteScalar());
    }

    private static bool TableExists(SqlConnection connection, string tableName)
    {
        using SqlCommand command = new(
            "SELECT CASE WHEN OBJECT_ID(@TableName, 'U') IS NULL THEN 0 ELSE 1 END", connection);
        command.Parameters.Add("@TableName", SqlDbType.NVarChar, 300).Value = "dbo." + tableName;
        return Convert.ToInt32(command.ExecuteScalar()) == 1;
    }

    private static bool ColumnExists(SqlConnection connection, string tableName, string columnName)
    {
        using SqlCommand command = new(
            "SELECT CASE WHEN COL_LENGTH(@TableName, @ColumnName) IS NULL THEN 0 ELSE 1 END", connection);
        command.Parameters.Add("@TableName", SqlDbType.NVarChar, 300).Value = "dbo." + tableName;
        command.Parameters.Add("@ColumnName", SqlDbType.NVarChar, 128).Value = columnName;
        return Convert.ToInt32(command.ExecuteScalar()) == 1;
    }

    private static int ExecuteInt(SqlConnection connection, string sql)
    {
        using SqlCommand command = new(sql, connection);
        return Convert.ToInt32(command.ExecuteScalar());
    }

    private static void ExecuteNonQuery(SqlConnection connection, string sql)
    {
        using SqlCommand command = new(sql, connection);
        command.ExecuteNonQuery();
    }

    private static string TableName
    {
        get
        {
            EnsureSchema();
            return medicineTableName!;
        }
    }
}
