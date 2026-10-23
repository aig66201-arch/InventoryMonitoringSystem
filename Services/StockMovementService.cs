using System.Data;
using Microsoft.Data.SqlClient;
using InventoryMonitoringSystem.Data;

namespace InventoryMonitoringSystem.Services;

/// <summary>
/// Stock ledger and batch inventory service. StockBatches is the authoritative
/// batch-level balance; Medicines.Current_Stock, when present, is maintained as
/// a derived aggregate for compatibility with the existing dashboard.
/// </summary>
public static class StockMovementService
{
    public const string StockIn = "Stock In";
    public const string StockOut = "Stock Out";
    public const string Expired = "Stock Out - Expired";
    public const string Disposal = "Stock Out - Disposal";
    public const string Damaged = "Stock Out - Damaged"; // Legacy value retained for historical records.
    public static readonly string[] TransactionTypes = [StockIn, StockOut, Expired, Disposal];

    public sealed record MedicineOption(string MedId, string GenericName, string BrandName, string Dosage, string DosageForm, string Unit)
    {
        public string Display => $"{MedId} | {GenericName}" +
            (string.IsNullOrWhiteSpace(BrandName) ? "" : $" | {BrandName}") +
            (string.IsNullOrWhiteSpace(Dosage) ? "" : $" | {Dosage}") +
            (string.IsNullOrWhiteSpace(DosageForm) ? "" : $" | {DosageForm}");
        public override string ToString() => Display;
    }

    public sealed record BatchOption(int BatchId, string BatchNo, DateTime? ExpiryDate, string Unit, decimal QuantityOnHand, decimal UnitCost)
    {
        public string Display => $"{BatchNo} | On hand: {QuantityOnHand:N0} {Unit}" +
            (ExpiryDate.HasValue ? $" | Exp: {ExpiryDate.Value:MMM-dd-yyyy}" : " | No expiry date");
        public override string ToString() => Display;
    }

    public sealed record SaveRequest(DateTime TransactionDate, string MedId, int? BatchId,
        string? NewBatchNo, DateTime? NewBatchExpiry, string TransactionType,
        decimal Quantity, decimal UnitCost, string Unit, string Remarks, string Username);

    public sealed record SaveResult(long MovementId, string TransactionId, decimal NewBalance);

    public static void EnsureSchema()
    {
        MedicineService.EnsureSchema();
        using SqlConnection connection = new(DatabaseConnection.DatabaseConnectionString);
        connection.Open();
        string medicineTable = ResolveMedicineTable(connection);
        bool hadCurrentStock = ColumnExists(connection, medicineTable, "Current_Stock");
        if (!hadCurrentStock)
            Execute(connection, $"ALTER TABLE {Q(medicineTable)} ADD Current_Stock DECIMAL(18,3) NOT NULL CONSTRAINT DF_{(medicineTable == "Medicine" ? "Medicine" : "Medicines")}_Current_Stock DEFAULT (0);");

        Execute(connection, @"
IF OBJECT_ID(N'dbo.StockBatches', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.StockBatches
    (
        BatchId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_StockBatches PRIMARY KEY,
        Med_ID NVARCHAR(20) NOT NULL,
        BatchNo NVARCHAR(60) NOT NULL,
        ExpiryDate DATE NULL,
        Unit NVARCHAR(50) NOT NULL,
        QuantityOnHand DECIMAL(18,3) NOT NULL CONSTRAINT DF_StockBatches_Quantity DEFAULT (0),
        UnitCost DECIMAL(18,4) NOT NULL CONSTRAINT DF_StockBatches_UnitCost DEFAULT (0),
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_StockBatches_CreatedAt DEFAULT SYSDATETIME(),
        CONSTRAINT CK_StockBatches_Quantity CHECK (QuantityOnHand >= 0),
        CONSTRAINT CK_StockBatches_Cost CHECK (UnitCost >= 0)
    );
    CREATE UNIQUE INDEX UX_StockBatches_Medicine_Batch ON dbo.StockBatches(Med_ID, BatchNo);
    CREATE INDEX IX_StockBatches_Medicine ON dbo.StockBatches(Med_ID);
END;
IF OBJECT_ID(N'dbo.StockMovements', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.StockMovements
    (
        MovementId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_StockMovements PRIMARY KEY,
        TransactionDate DATETIME2 NOT NULL,
        Med_ID NVARCHAR(20) NOT NULL,
        BatchId INT NOT NULL,
        TransactionType NVARCHAR(40) NOT NULL,
        Quantity DECIMAL(18,3) NOT NULL,
        Unit NVARCHAR(50) NOT NULL,
        UnitCost DECIMAL(18,4) NOT NULL,
        TotalCost DECIMAL(18,4) NOT NULL,
        Remarks NVARCHAR(1000) NULL,
        CreatedBy NVARCHAR(100) NOT NULL,
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_StockMovements_CreatedAt DEFAULT SYSDATETIME(),
        CONSTRAINT FK_StockMovements_StockBatches FOREIGN KEY (BatchId) REFERENCES dbo.StockBatches(BatchId),
        CONSTRAINT CK_StockMovements_Quantity CHECK (Quantity > 0),
        CONSTRAINT CK_StockMovements_Cost CHECK (UnitCost >= 0 AND TotalCost >= 0),
        CONSTRAINT CK_StockMovements_Type CHECK (TransactionType IN (N'Stock In', N'Stock Out', N'Stock Out - Expired', N'Stock Out - Damaged'))
    );
    CREATE INDEX IX_StockMovements_Date ON dbo.StockMovements(TransactionDate DESC);
    CREATE INDEX IX_StockMovements_Medicine ON dbo.StockMovements(Med_ID);
END;");

        // Migrate the movement-type constraint so new disposal transactions can be recorded while legacy damaged rows remain valid.
        Execute(connection, @"
IF OBJECT_ID(N'dbo.StockMovements', N'U') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE parent_object_id=OBJECT_ID(N'dbo.StockMovements') AND name=N'CK_StockMovements_Type' AND definition LIKE N'%Stock Out - Disposal%')
BEGIN
    IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE parent_object_id=OBJECT_ID(N'dbo.StockMovements') AND name=N'CK_StockMovements_Type')
        ALTER TABLE dbo.StockMovements DROP CONSTRAINT CK_StockMovements_Type;
    ALTER TABLE dbo.StockMovements ADD CONSTRAINT CK_StockMovements_Type CHECK (TransactionType IN (N'Stock In',N'Stock Out',N'Stock Out - Expired',N'Stock Out - Disposal',N'Stock Out - Damaged'));
END;");

        // Preserve pre-existing stock balances by importing them as opening batches exactly once.
        // After this migration, batch quantities are authoritative and Current_Stock is derived.
        if (hadCurrentStock)
        {
            string costColumn = new[] { "Cost_Per_Unit", "CostPerUnit", "Unit_Cost", "UnitCost" }
                .FirstOrDefault(c => ColumnExists(connection, medicineTable, c)) ?? string.Empty;
            string costExpression = costColumn.Length == 0 ? "CAST(0 AS DECIMAL(18,4))" : $"TRY_CONVERT(DECIMAL(18,4), m.[{costColumn}])";
            Execute(connection, $@"
INSERT INTO dbo.StockBatches(Med_ID,BatchNo,ExpiryDate,Unit,QuantityOnHand,UnitCost)
SELECT m.Med_ID, N'OPENING-' + m.Med_ID, NULL, COALESCE(NULLIF(m.Unit,N''),N'Unit'),
       m.Current_Stock, COALESCE({costExpression},0)
FROM {Q(medicineTable)} m
WHERE m.Current_Stock > 0
  AND NOT EXISTS (SELECT 1 FROM dbo.StockBatches b WHERE b.Med_ID=m.Med_ID);
INSERT INTO dbo.StockMovements(TransactionDate,Med_ID,BatchId,TransactionType,Quantity,Unit,UnitCost,TotalCost,Remarks,CreatedBy)
SELECT b.CreatedAt,b.Med_ID,b.BatchId,N'Stock In',b.QuantityOnHand,b.Unit,b.UnitCost,
       CONVERT(DECIMAL(18,2),b.QuantityOnHand*b.UnitCost),
       N'Opening balance imported from existing Medicine Catalog stock during Stock Movement setup.',N'System Migration'
FROM dbo.StockBatches b
WHERE b.BatchNo=N'OPENING-' + b.Med_ID AND b.QuantityOnHand > 0
  AND NOT EXISTS (SELECT 1 FROM dbo.StockMovements sm WHERE sm.BatchId=b.BatchId);");
        }
        Execute(connection, $@"UPDATE m SET Current_Stock = ISNULL(b.TotalQty, 0)
FROM {Q(medicineTable)} m
OUTER APPLY (SELECT SUM(QuantityOnHand) AS TotalQty FROM dbo.StockBatches WHERE Med_ID = m.Med_ID) b;");
    }

    public static List<MedicineOption> GetMedicines()
    {
        EnsureSchema();
        DataTable table = MedicineService.GetAllMedicines();
        List<MedicineOption> result = [];
        foreach (DataRow row in table.Rows)
        {
            result.Add(new MedicineOption(
                Convert.ToString(row["Med_ID"]) ?? "",
                Convert.ToString(row["Generic_Name"]) ?? "",
                Convert.ToString(row["Brand_Name"]) ?? "",
                Convert.ToString(row["Dosage"]) ?? "",
                Convert.ToString(row["Dosage_Form"]) ?? "",
                Convert.ToString(row["Unit"]) ?? ""));
        }
        return result;
    }

    public static List<BatchOption> GetBatches(string medId)
    {
        EnsureSchema();
        List<BatchOption> result = [];
        using SqlConnection connection = new(DatabaseConnection.DatabaseConnectionString);
        connection.Open();
        using SqlCommand command = new(@"SELECT BatchId, BatchNo, ExpiryDate, Unit, QuantityOnHand, UnitCost
FROM dbo.StockBatches WHERE Med_ID=@MedId AND QuantityOnHand>0 ORDER BY CASE WHEN ExpiryDate IS NULL THEN 1 ELSE 0 END, ExpiryDate, BatchNo;", connection);
        command.Parameters.Add("@MedId", SqlDbType.NVarChar, 20).Value = medId;
        using SqlDataReader reader = command.ExecuteReader();
        while (reader.Read())
            result.Add(new BatchOption(reader.GetInt32(0), reader.GetString(1), reader.IsDBNull(2) ? null : reader.GetDateTime(2),
                reader.GetString(3), reader.GetDecimal(4), reader.GetDecimal(5)));
        return result;
    }

    public static string GetNextBatchNo(string medId)
    {
        EnsureSchema();
        using SqlConnection connection = new(DatabaseConnection.DatabaseConnectionString);
        connection.Open();
        using SqlCommand command = new(@"SELECT ISNULL(MAX(TRY_CONVERT(INT, SUBSTRING(BatchNo, 3, 20))), 0) + 1
FROM dbo.StockBatches
WHERE Med_ID=@MedId AND BatchNo LIKE N'B-[0-9]%' AND TRY_CONVERT(INT, SUBSTRING(BatchNo, 3, 20)) IS NOT NULL;", connection);
        command.Parameters.Add("@MedId", SqlDbType.NVarChar, 20).Value = medId;
        return "B-" + Convert.ToInt32(command.ExecuteScalar()).ToString("D3", System.Globalization.CultureInfo.InvariantCulture);
    }

    public static decimal GetMedicineStock(string medId)
    {
        EnsureSchema();
        using SqlConnection connection = new(DatabaseConnection.DatabaseConnectionString);
        connection.Open();
        using SqlCommand command = new("SELECT COALESCE(SUM(QuantityOnHand),0) FROM dbo.StockBatches WHERE Med_ID=@MedId;", connection);
        command.Parameters.Add("@MedId", SqlDbType.NVarChar, 20).Value = medId;
        return Convert.ToDecimal(command.ExecuteScalar());
    }

    public static DataTable GetMovements(string search, DateTime? start, DateTime? end, string type)
    {
        EnsureSchema();
        DataTable table = new("StockMovements");
        using SqlConnection connection = new(DatabaseConnection.DatabaseConnectionString);
        connection.Open();
        const string sql = @"
SELECT sm.MovementId, sm.TransactionDate,
       N'T-' + CASE WHEN sm.MovementId < 1000 THEN RIGHT(N'000' + CONVERT(NVARCHAR(20), sm.MovementId), 3) ELSE CONVERT(NVARCHAR(20), sm.MovementId) END AS TransactionID,
       b.BatchNo AS BatchNo,
       sm.Med_ID,
       COALESCE(NULLIF(m.Generic_Name,N''), NULLIF(m.MedicineName,N''), N'') AS GenericName,
       COALESCE(m.Brand_Name,N'') AS BrandName, COALESCE(m.Dosage,N'') AS Dosage,
       sm.Quantity, sm.Unit, sm.TransactionType, sm.UnitCost, sm.TotalCost,
       COALESCE(sm.Remarks,N'') AS Remarks, COALESCE(sm.CreatedBy,N'') AS CreatedBy,
       b.ExpiryDate, b.QuantityOnHand AS BatchBalance
FROM dbo.StockMovements sm
INNER JOIN dbo.StockBatches b ON b.BatchId=sm.BatchId
LEFT JOIN dbo.Medicines m ON m.Med_ID=sm.Med_ID
WHERE (@Search=N'' OR (N'T-' + CASE WHEN sm.MovementId < 1000 THEN RIGHT(N'000' + CONVERT(NVARCHAR(20), sm.MovementId), 3) ELSE CONVERT(NVARCHAR(20), sm.MovementId) END) LIKE @Like OR CONVERT(NVARCHAR(20),sm.MovementId) LIKE @Like OR sm.Med_ID LIKE @Like
    OR b.BatchNo LIKE @Like OR m.Generic_Name LIKE @Like OR m.MedicineName LIKE @Like OR m.Brand_Name LIKE @Like)
  AND (@Start IS NULL OR sm.TransactionDate >= @Start)
  AND (@End IS NULL OR sm.TransactionDate < DATEADD(day,1,@End))
  AND (@Type=N'All Types' OR sm.TransactionType=@Type)
ORDER BY sm.TransactionDate DESC, sm.MovementId DESC;";
        // Resolve the actual catalog table name so legacy singular-table databases also work.
        string medicineTable = ResolveMedicineTable(connection);
        string actualSql = sql.Replace("dbo.Medicines m", Q(medicineTable) + " m", StringComparison.Ordinal);
        using SqlCommand command = new(actualSql, connection);
        command.Parameters.Add("@Search", SqlDbType.NVarChar, 200).Value = search.Trim();
        command.Parameters.Add("@Like", SqlDbType.NVarChar, 210).Value = "%" + search.Trim() + "%";
        command.Parameters.Add("@Start", SqlDbType.Date).Value = start.HasValue ? start.Value.Date : DBNull.Value;
        command.Parameters.Add("@End", SqlDbType.Date).Value = end.HasValue ? end.Value.Date : DBNull.Value;
        command.Parameters.Add("@Type", SqlDbType.NVarChar, 40).Value = string.IsNullOrWhiteSpace(type) ? "All Types" : type;
        using SqlDataAdapter adapter = new(command);
        adapter.Fill(table);
        return table;
    }

    public static (int Transactions, decimal StockIn, decimal StockOut, decimal ExpiredDamaged) GetSummary(DateTime? start, DateTime? end)
    {
        EnsureSchema();
        using SqlConnection connection = new(DatabaseConnection.DatabaseConnectionString);
        connection.Open();
        const string sql = @"SELECT COUNT(*) AS TxnCount,
COALESCE(SUM(CASE WHEN TransactionType=N'Stock In' THEN Quantity ELSE 0 END),0) AS StockIn,
COALESCE(SUM(CASE WHEN TransactionType IN (N'Stock Out',N'Stock Out - Expired',N'Stock Out - Disposal',N'Stock Out - Damaged') THEN Quantity ELSE 0 END),0) AS StockOut,
COALESCE(SUM(CASE WHEN TransactionType IN (N'Stock Out - Expired',N'Stock Out - Disposal',N'Stock Out - Damaged') THEN Quantity ELSE 0 END),0) AS ExpiredDamaged
FROM dbo.StockMovements WHERE (@Start IS NULL OR TransactionDate>=@Start) AND (@End IS NULL OR TransactionDate<DATEADD(day,1,@End));";
        using SqlCommand command = new(sql, connection);
        command.Parameters.Add("@Start", SqlDbType.Date).Value = start.HasValue ? start.Value.Date : DBNull.Value;
        command.Parameters.Add("@End", SqlDbType.Date).Value = end.HasValue ? end.Value.Date : DBNull.Value;
        using SqlDataReader reader = command.ExecuteReader();
        reader.Read();
        return (reader.GetInt32(0), reader.GetDecimal(1), reader.GetDecimal(2), reader.GetDecimal(3));
    }


    public static DataTable GetLowStockMedicines()
    {
        EnsureSchema();
        using SqlConnection connection = new(DatabaseConnection.DatabaseConnectionString);
        connection.Open();
        string medicineTable = ResolveMedicineTable(connection);
        string sql = $@"SELECT m.Med_ID AS [Med ID],
 COALESCE(NULLIF(m.Generic_Name,N''), NULLIF(m.MedicineName,N''), N'') AS [Medicine],
 COALESCE(NULLIF(m.Brand_Name,N''),N'') AS [Brand], COALESCE(NULLIF(m.Dosage,N''),N'') AS [Dosage],
 COALESCE(m.Reorder_Level,0) AS [Reorder Level],
 CAST(ISNULL(b.TotalQty,0) AS DECIMAL(18,0)) AS [Current Stock],
 COALESCE(NULLIF(m.Unit,N''),N'Unit') AS [Unit]
FROM {Q(medicineTable)} m
OUTER APPLY (SELECT SUM(QuantityOnHand) AS TotalQty FROM dbo.StockBatches WHERE Med_ID=m.Med_ID) b
WHERE m.IsActive=1 AND ISNULL(b.TotalQty,0)>0 AND ISNULL(b.TotalQty,0)<=m.Reorder_Level
ORDER BY ISNULL(b.TotalQty,0), [Medicine];";
        using SqlDataAdapter adapter = new(sql, connection);
        DataTable table = new("LowStock"); adapter.Fill(table); return table;
    }

    public static DataTable GetOutOfStockMedicines()
    {
        EnsureSchema();
        using SqlConnection connection = new(DatabaseConnection.DatabaseConnectionString);
        connection.Open();
        string medicineTable = ResolveMedicineTable(connection);
        string sql = $@"SELECT m.Med_ID AS [Med ID],
 COALESCE(NULLIF(m.Generic_Name,N''), NULLIF(m.MedicineName,N''), N'') AS [Medicine],
 COALESCE(NULLIF(m.Brand_Name,N''),N'') AS [Brand], COALESCE(NULLIF(m.Dosage,N''),N'') AS [Dosage],
 CAST(ISNULL(b.TotalQty,0) AS DECIMAL(18,0)) AS [Current Stock],
 COALESCE(NULLIF(m.Unit,N''),N'Unit') AS [Unit]
FROM {Q(medicineTable)} m
OUTER APPLY (SELECT SUM(QuantityOnHand) AS TotalQty FROM dbo.StockBatches WHERE Med_ID=m.Med_ID) b
WHERE m.IsActive=1 AND ISNULL(b.TotalQty,0)<=0
ORDER BY [Medicine];";
        using SqlDataAdapter adapter = new(sql, connection);
        DataTable table = new("OutOfStock"); adapter.Fill(table); return table;
    }

    public static DataTable GetExpiredBatches()
    {
        EnsureSchema();
        using SqlConnection connection = new(DatabaseConnection.DatabaseConnectionString);
        connection.Open();
        string medicineTable = ResolveMedicineTable(connection);
        string sql = $@"SELECT b.BatchNo AS [Batch No.], m.Med_ID AS [Med ID],
 COALESCE(NULLIF(m.Generic_Name,N''), NULLIF(m.MedicineName,N''), N'') AS [Medicine],
 COALESCE(NULLIF(m.Brand_Name,N''),N'') AS [Brand], COALESCE(NULLIF(m.Dosage,N''),N'') AS [Dosage],
 b.ExpiryDate AS [Expiry Date], CAST(b.QuantityOnHand AS DECIMAL(18,0)) AS [Current Stock],
 COALESCE(NULLIF(b.Unit,N''),N'Unit') AS [Unit]
FROM dbo.StockBatches b
INNER JOIN {Q(medicineTable)} m ON m.Med_ID=b.Med_ID
WHERE m.IsActive=1 AND b.QuantityOnHand>0 AND b.ExpiryDate<CONVERT(date,GETDATE())
ORDER BY b.ExpiryDate, [Medicine];";
        using SqlDataAdapter adapter = new(sql, connection);
        DataTable table = new("ExpiredBatches");
        adapter.Fill(table);
        return table;
    }

    public static DataTable GetExpiringBatches(int days)
    {
        if (days < 1 || days > 3650) throw new ArgumentOutOfRangeException(nameof(days), "Expiry window must be between 1 and 3,650 days.");
        EnsureSchema();
        using SqlConnection connection = new(DatabaseConnection.DatabaseConnectionString);
        connection.Open();
        string medicineTable = ResolveMedicineTable(connection);
        string sql = $@"SELECT b.BatchNo AS [Batch No.], m.Med_ID AS [Med ID],
 COALESCE(NULLIF(m.Generic_Name,N''), NULLIF(m.MedicineName,N''), N'') AS [Medicine],
 COALESCE(NULLIF(m.Brand_Name,N''),N'') AS [Brand], COALESCE(NULLIF(m.Dosage,N''),N'') AS [Dosage],
 b.ExpiryDate AS [Expiry Date], CAST(b.QuantityOnHand AS DECIMAL(18,0)) AS [Current Stock],
 COALESCE(NULLIF(b.Unit,N''),N'Unit') AS [Unit]
FROM dbo.StockBatches b
INNER JOIN {Q(medicineTable)} m ON m.Med_ID=b.Med_ID
WHERE m.IsActive=1 AND b.QuantityOnHand>0 AND b.ExpiryDate>=CONVERT(date,GETDATE())
 AND b.ExpiryDate<=DATEADD(day,@Days,CONVERT(date,GETDATE()))
ORDER BY b.ExpiryDate, [Medicine];";
        using SqlCommand command = new(sql, connection);
        command.Parameters.Add("@Days", SqlDbType.Int).Value = days;
        using SqlDataAdapter adapter = new(command);
        DataTable table = new("ExpiringBatches"); adapter.Fill(table); return table;
    }

    public static SaveResult Save(SaveRequest request)
    {
        EnsureSchema();
        if (request.TransactionDate > DateTime.Now.AddMinutes(2)) throw new InvalidOperationException("Transaction date and time cannot be in the future.");
        if (string.IsNullOrWhiteSpace(request.MedId)) throw new InvalidOperationException("Select a medicine.");
        if (string.IsNullOrWhiteSpace(request.Unit)) throw new InvalidOperationException("The selected medicine must have a configured unit of measure.");
        if (request.Quantity <= 0) throw new InvalidOperationException("Quantity must be greater than zero.");
        if (request.UnitCost < 0) throw new InvalidOperationException("Cost per unit cannot be negative.");
        if (!TransactionTypes.Contains(request.TransactionType, StringComparer.Ordinal)) throw new InvalidOperationException("Select a valid transaction type.");
        using SqlConnection connection = new(DatabaseConnection.DatabaseConnectionString);
        connection.Open();
        using SqlTransaction transaction = connection.BeginTransaction(IsolationLevel.Serializable);
        try
        {
            string medicineTable = ResolveMedicineTable(connection, transaction);
            using (SqlCommand medicineCheck = new($"SELECT Unit FROM {Q(medicineTable)} WHERE Med_ID=@MedId AND IsActive=1;", connection, transaction))
            {
                medicineCheck.Parameters.Add("@MedId", SqlDbType.NVarChar, 20).Value = request.MedId;
                object? unitValue = medicineCheck.ExecuteScalar();
                if (unitValue == null || unitValue == DBNull.Value) throw new InvalidOperationException("The selected medicine is missing or inactive in the Medicine Catalog.");
            }

            int batchId;
            decimal actualCost;
            string actualUnit;
            DateTime? expiry;
            decimal newBalance;
            bool stockIn = request.TransactionType == StockIn;

            if (stockIn && !request.BatchId.HasValue)
            {
                if (string.IsNullOrWhiteSpace(request.NewBatchNo)) throw new InvalidOperationException("Enter a batch number for the new batch.");
                if (!request.NewBatchExpiry.HasValue) throw new InvalidOperationException("Select the batch expiration date.");
                if (request.NewBatchExpiry.Value.Date < request.TransactionDate.Date) throw new InvalidOperationException("The new batch expiration date cannot be earlier than the transaction date.");
                using SqlCommand insertBatch = new(@"INSERT INTO dbo.StockBatches(Med_ID,BatchNo,ExpiryDate,Unit,QuantityOnHand,UnitCost)
OUTPUT INSERTED.BatchId VALUES(@MedId,@BatchNo,@Expiry,@Unit,0,@Cost);", connection, transaction);
                insertBatch.Parameters.Add("@MedId", SqlDbType.NVarChar, 20).Value = request.MedId;
                insertBatch.Parameters.Add("@BatchNo", SqlDbType.NVarChar, 60).Value = request.NewBatchNo.Trim();
                insertBatch.Parameters.Add("@Expiry", SqlDbType.Date).Value = request.NewBatchExpiry.Value.Date;
                insertBatch.Parameters.Add("@Unit", SqlDbType.NVarChar, 50).Value = request.Unit;
                insertBatch.Parameters.Add("@Cost", SqlDbType.Decimal).Value = request.UnitCost;
                insertBatch.Parameters["@Cost"].Precision = 18; insertBatch.Parameters["@Cost"].Scale = 4;
                try { batchId = Convert.ToInt32(insertBatch.ExecuteScalar()); }
                catch (SqlException ex) when (ex.Number is 2601 or 2627) { throw new InvalidOperationException("That batch number already exists for this medicine. Select the existing batch instead.", ex); }
                actualCost = request.UnitCost; actualUnit = request.Unit; expiry = request.NewBatchExpiry.Value.Date;
            }
            else
            {
                if (!request.BatchId.HasValue) throw new InvalidOperationException("Select an existing batch.");
                using SqlCommand batch = new(@"SELECT BatchId,BatchNo,ExpiryDate,Unit,QuantityOnHand,UnitCost FROM dbo.StockBatches WITH (UPDLOCK,HOLDLOCK) WHERE BatchId=@BatchId AND Med_ID=@MedId;", connection, transaction);
                batch.Parameters.Add("@BatchId", SqlDbType.Int).Value = request.BatchId.Value;
                batch.Parameters.Add("@MedId", SqlDbType.NVarChar, 20).Value = request.MedId;
                using SqlDataReader reader = batch.ExecuteReader();
                if (!reader.Read()) throw new InvalidOperationException("The selected batch does not belong to this medicine or no longer exists.");
                batchId = reader.GetInt32(0); expiry = reader.IsDBNull(2) ? null : reader.GetDateTime(2);
                actualUnit = reader.GetString(3); decimal onHand = reader.GetDecimal(4); actualCost = reader.GetDecimal(5);
                reader.Close();
                if (stockIn && expiry.HasValue && expiry.Value.Date < request.TransactionDate.Date)
                    throw new InvalidOperationException("Cannot receive stock into an expired batch. Create or select a valid batch.");
                if (!stockIn)
                {
                    if (request.TransactionType == Expired && (!expiry.HasValue || expiry.Value.Date >= request.TransactionDate.Date))
                        throw new InvalidOperationException("Stock Out - Expired requires a batch whose expiration date is earlier than the transaction date.");
                    if (request.TransactionType == StockOut && expiry.HasValue && expiry.Value.Date < request.TransactionDate.Date)
                        throw new InvalidOperationException("This batch is expired. Use Stock Out - Expired and provide remarks.");
                    if (request.Quantity > onHand) throw new InvalidOperationException($"Insufficient stock. This batch has only {onHand:N0} {actualUnit} available.");
                }
            }

            if (stockIn)
            {
                // Keep the selected batch's saved unit cost unchanged when receiving into an existing batch.
                if (request.BatchId.HasValue)
                {
                    using SqlCommand get = new("SELECT Unit,UnitCost,ExpiryDate FROM dbo.StockBatches WHERE BatchId=@Id;", connection, transaction);
                    get.Parameters.Add("@Id", SqlDbType.Int).Value = batchId;
                    using SqlDataReader r = get.ExecuteReader(); r.Read(); actualUnit = r.GetString(0); actualCost = r.GetDecimal(1); expiry = r.IsDBNull(2) ? null : r.GetDateTime(2); r.Close();
                }
                using SqlCommand update = new("UPDATE dbo.StockBatches SET QuantityOnHand=QuantityOnHand+@Qty WHERE BatchId=@Id;", connection, transaction);
                update.Parameters.Add("@Qty", SqlDbType.Decimal).Value = request.Quantity; update.Parameters["@Qty"].Precision = 18; update.Parameters["@Qty"].Scale = 3;
                update.Parameters.Add("@Id", SqlDbType.Int).Value = batchId; update.ExecuteNonQuery();
            }
            else
            {
                using SqlCommand update = new("UPDATE dbo.StockBatches SET QuantityOnHand=QuantityOnHand-@Qty WHERE BatchId=@Id AND QuantityOnHand>=@Qty;", connection, transaction);
                update.Parameters.Add("@Qty", SqlDbType.Decimal).Value = request.Quantity; update.Parameters["@Qty"].Precision = 18; update.Parameters["@Qty"].Scale = 3;
                update.Parameters.Add("@Id", SqlDbType.Int).Value = batchId;
                if (update.ExecuteNonQuery() != 1) throw new InvalidOperationException("Stock changed while saving. Refresh the batch list and try again.");
            }

            if (!stockIn) actualCost = decimal.Round(actualCost, 2, MidpointRounding.AwayFromZero);
            else actualCost = decimal.Round(actualCost, 2, MidpointRounding.AwayFromZero);
            decimal total = decimal.Round(request.Quantity * actualCost, 2, MidpointRounding.AwayFromZero);
            long movementId;
            using (SqlCommand insert = new(@"INSERT INTO dbo.StockMovements(TransactionDate,Med_ID,BatchId,TransactionType,Quantity,Unit,UnitCost,TotalCost,Remarks,CreatedBy)
OUTPUT INSERTED.MovementId VALUES(@Date,@MedId,@BatchId,@Type,@Qty,@Unit,@Cost,@Total,@Remarks,@User);", connection, transaction))
            {
                insert.Parameters.Add("@Date", SqlDbType.DateTime2).Value = request.TransactionDate;
                insert.Parameters.Add("@MedId", SqlDbType.NVarChar, 20).Value = request.MedId;
                insert.Parameters.Add("@BatchId", SqlDbType.Int).Value = batchId;
                insert.Parameters.Add("@Type", SqlDbType.NVarChar, 40).Value = request.TransactionType;
                AddDecimal(insert, "@Qty", request.Quantity, 18, 3); insert.Parameters.Add("@Unit", SqlDbType.NVarChar, 50).Value = actualUnit;
                AddDecimal(insert, "@Cost", actualCost, 18, 2); AddDecimal(insert, "@Total", total, 18, 2);
                insert.Parameters.Add("@Remarks", SqlDbType.NVarChar, 1000).Value = string.IsNullOrWhiteSpace(request.Remarks) ? DBNull.Value : request.Remarks.Trim();
                insert.Parameters.Add("@User", SqlDbType.NVarChar, 100).Value = string.IsNullOrWhiteSpace(request.Username) ? "Unknown" : request.Username.Trim();
                movementId = Convert.ToInt64(insert.ExecuteScalar());
            }
            using (SqlCommand aggregate = new($@"UPDATE {Q(medicineTable)} SET Current_Stock=ISNULL((SELECT SUM(QuantityOnHand) FROM dbo.StockBatches WHERE Med_ID=@MedId),0) WHERE Med_ID=@MedId;", connection, transaction))
            {
                aggregate.Parameters.Add("@MedId", SqlDbType.NVarChar, 20).Value = request.MedId; aggregate.ExecuteNonQuery();
            }
            using (SqlCommand balance = new("SELECT QuantityOnHand FROM dbo.StockBatches WHERE BatchId=@Id;", connection, transaction))
            {
                balance.Parameters.Add("@Id", SqlDbType.Int).Value = batchId; newBalance = Convert.ToDecimal(balance.ExecuteScalar());
            }
            transaction.Commit();
            return new SaveResult(movementId, $"T-{movementId:D3}", newBalance);
        }
        catch { try { transaction.Rollback(); } catch { } throw; }
    }

    private static void AddDecimal(SqlCommand command, string name, decimal value, byte precision, byte scale)
    {
        SqlParameter p = command.Parameters.Add(name, SqlDbType.Decimal); p.Precision = precision; p.Scale = scale; p.Value = value;
    }

    private static string ResolveMedicineTable(SqlConnection connection, SqlTransaction? transaction = null)
    {
        bool hasMedicines;
        bool hasMedicine;
        using (SqlCommand command = new("SELECT CASE WHEN OBJECT_ID(N'dbo.Medicines','U') IS NULL THEN 0 ELSE 1 END;", connection, transaction))
            hasMedicines = Convert.ToInt32(command.ExecuteScalar()) == 1;
        using (SqlCommand command = new("SELECT CASE WHEN OBJECT_ID(N'dbo.Medicine','U') IS NULL THEN 0 ELSE 1 END;", connection, transaction))
            hasMedicine = Convert.ToInt32(command.ExecuteScalar()) == 1;
        if (hasMedicines && !hasMedicine) return "Medicines";
        if (hasMedicine && !hasMedicines) return "Medicine";
        if (!hasMedicine && !hasMedicines) throw new InvalidOperationException("The Medicine Catalog table was not found.");
        int pluralCount;
        int singularCount;
        using (SqlCommand command = new("SELECT COUNT(*) FROM dbo.Medicines;", connection, transaction)) pluralCount = Convert.ToInt32(command.ExecuteScalar());
        using (SqlCommand command = new("SELECT COUNT(*) FROM dbo.Medicine;", connection, transaction)) singularCount = Convert.ToInt32(command.ExecuteScalar());
        return singularCount > 0 && pluralCount == 0 ? "Medicine" : "Medicines";
    }
    private static bool ColumnExists(SqlConnection connection, string table, string column)
    {
        using SqlCommand command = new("SELECT CASE WHEN COL_LENGTH(@Table,@Column) IS NULL THEN 0 ELSE 1 END;", connection);
        command.Parameters.Add("@Table", SqlDbType.NVarChar, 300).Value = "dbo." + table;
        command.Parameters.Add("@Column", SqlDbType.NVarChar, 128).Value = column;
        return Convert.ToInt32(command.ExecuteScalar()) == 1;
    }
    private static string Q(string name) => "dbo.[" + name.Replace("]", "]]", StringComparison.Ordinal) + "]";
    private static void Execute(SqlConnection connection, string sql)
    { using SqlCommand command = new(sql, connection); command.ExecuteNonQuery(); }
}
