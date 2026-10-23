using System.Data;
using Microsoft.Data.SqlClient;
using InventoryMonitoringSystem.Data;

namespace InventoryMonitoringSystem.Services;

/// <summary>Read-only inventory summaries and analytics backed by the existing batch ledger.</summary>
public static class InventoryControlService
{
    private static string Prepare()
    {
        StockMovementService.EnsureSchema();
        using SqlConnection connection = new(DatabaseConnection.DatabaseConnectionString);
        connection.Open();
        bool hasMedicines, hasMedicine;
        using (SqlCommand cmd = new("SELECT CASE WHEN OBJECT_ID(N'dbo.Medicines',N'U') IS NULL THEN 0 ELSE 1 END, CASE WHEN OBJECT_ID(N'dbo.Medicine',N'U') IS NULL THEN 0 ELSE 1 END;", connection))
        using (SqlDataReader reader = cmd.ExecuteReader())
        {
            reader.Read(); hasMedicines = reader.GetInt32(0) == 1; hasMedicine = reader.GetInt32(1) == 1;
        }
        string table = hasMedicines ? "Medicines" : "Medicine";
        if (hasMedicines && hasMedicine)
        {
            int medicinesRows, medicineRows;
            using (SqlCommand cmd = new("SELECT (SELECT COUNT_BIG(*) FROM dbo.Medicines), (SELECT COUNT_BIG(*) FROM dbo.Medicine);", connection))
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                reader.Read(); medicinesRows = Convert.ToInt32(reader.GetInt64(0)); medicineRows = Convert.ToInt32(reader.GetInt64(1));
            }
            if (medicineRows > 0 && medicinesRows == 0) table = "Medicine";
        }
        return table == "Medicine" ? "dbo.[Medicine]" : "dbo.[Medicines]";
    }

    private static DataTable Query(string sql, Action<SqlCommand>? parameters = null)
    {
        string table = Prepare();
        sql = sql.Replace("{MEDICINE_TABLE}", table, StringComparison.Ordinal);
        using SqlConnection connection = new(DatabaseConnection.DatabaseConnectionString);
        connection.Open();
        using SqlCommand command = new(sql, connection);
        parameters?.Invoke(command);
        using SqlDataAdapter adapter = new(command);
        DataTable result = new();
        adapter.Fill(result);
        return result;
    }

    public static DataTable GetStockRegister(int expiryDays = 30, string search = "", string status = "All") => Query(@"
SELECT m.Med_ID AS [Medicine ID], COALESCE(NULLIF(m.Generic_Name,N''),NULLIF(m.MedicineName,N''),N'') AS [Medicine],
       COALESCE(m.Brand_Name,N'') AS [Brand], COALESCE(m.Dosage,N'') AS [Dosage], COALESCE(m.Dosage_Form,N'') AS [Form],
       COALESCE(NULLIF(m.Unit,N''),N'Unit') AS [Unit], COALESCE(m.Reorder_Level,0) AS [Reorder Level],
       CAST(ISNULL(q.TotalQty,0) AS DECIMAL(18,3)) AS [Physical Stock],
       CAST(ISNULL(q.UsableQty,0) AS DECIMAL(18,3)) AS [Usable Stock],
       CAST(ISNULL(q.ExpiredQty,0) AS DECIMAL(18,3)) AS [Expired Qty],
       CAST(ISNULL(q.ExpiringQty,0) AS DECIMAL(18,3)) AS [Expiring Qty],
       CAST(ISNULL(q.StockValue,0) AS DECIMAL(18,2)) AS [Stock Value],
       CASE WHEN ISNULL(q.UsableQty,0)<=0 THEN N'Out of Stock'
            WHEN ISNULL(q.ExpiredQty,0)>0 THEN N'Expired Batch'
            WHEN ISNULL(q.UsableQty,0)<=COALESCE(m.Reorder_Level,0) THEN N'Low Stock'
            WHEN ISNULL(q.ExpiringQty,0)>0 THEN N'Expiring'
            ELSE N'Healthy' END AS [Status],
       q.LastMovement AS [Last Movement]
FROM {MEDICINE_TABLE} m
OUTER APPLY (SELECT SUM(b.QuantityOnHand) TotalQty,
    SUM(CASE WHEN b.ExpiryDate IS NULL OR b.ExpiryDate>=CONVERT(date,GETDATE()) THEN b.QuantityOnHand ELSE 0 END) UsableQty,
    SUM(CASE WHEN b.ExpiryDate<CONVERT(date,GETDATE()) THEN b.QuantityOnHand ELSE 0 END) ExpiredQty,
    SUM(CASE WHEN b.ExpiryDate>=CONVERT(date,GETDATE()) AND b.ExpiryDate<=DATEADD(day,@Days,CONVERT(date,GETDATE())) THEN b.QuantityOnHand ELSE 0 END) ExpiringQty,
    SUM(CASE WHEN b.ExpiryDate IS NULL OR b.ExpiryDate>=CONVERT(date,GETDATE()) THEN b.QuantityOnHand*b.UnitCost ELSE 0 END) StockValue
    FROM dbo.StockBatches b WHERE b.Med_ID=m.Med_ID) q
OUTER APPLY (SELECT MAX(sm.TransactionDate) LastMovement FROM dbo.StockMovements sm WHERE sm.Med_ID=m.Med_ID) lm
WHERE m.IsActive=1
 AND (@Search=N'' OR m.Med_ID LIKE @Like OR m.Generic_Name LIKE @Like OR m.MedicineName LIKE @Like OR m.Brand_Name LIKE @Like OR m.Dosage LIKE @Like)
 AND (@Status=N'All' OR (@Status=N'Out of Stock' AND ISNULL(q.UsableQty,0)<=0)
 OR (@Status=N'Low Stock' AND ISNULL(q.UsableQty,0)>0 AND ISNULL(q.UsableQty,0)<=COALESCE(m.Reorder_Level,0))
 OR (@Status=N'Expiring' AND ISNULL(q.ExpiringQty,0)>0)
 OR (@Status=N'Expired Batch' AND ISNULL(q.ExpiredQty,0)>0)
 OR (@Status=N'Healthy' AND ISNULL(q.UsableQty,0)>COALESCE(m.Reorder_Level,0) AND ISNULL(q.ExpiringQty,0)=0 AND ISNULL(q.ExpiredQty,0)=0))
ORDER BY CASE WHEN ISNULL(q.UsableQty,0)<=0 THEN 0 WHEN ISNULL(q.ExpiredQty,0)>0 THEN 1 WHEN ISNULL(q.UsableQty,0)<=COALESCE(m.Reorder_Level,0) THEN 2 WHEN ISNULL(q.ExpiringQty,0)>0 THEN 3 ELSE 4 END, [Medicine];
".Replace("q.LastMovement", "lm.LastMovement"), cmd =>
    {
        cmd.Parameters.Add("@Days", SqlDbType.Int).Value = Math.Clamp(expiryDays, 1, 3650);
        cmd.Parameters.Add("@Search", SqlDbType.NVarChar, 200).Value = search.Trim();
        cmd.Parameters.Add("@Like", SqlDbType.NVarChar, 210).Value = "%" + search.Trim() + "%";
        cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 30).Value = string.IsNullOrWhiteSpace(status) ? "All" : status;
    });

    public static DataTable GetExpiryRisk(int expiryDays = 30, string risk = "All") => Query(@"
SELECT b.BatchNo AS [Batch No.], m.Med_ID AS [Medicine ID], COALESCE(NULLIF(m.Generic_Name,N''),NULLIF(m.MedicineName,N''),N'') AS [Medicine],
       COALESCE(m.Brand_Name,N'') AS [Brand], COALESCE(m.Dosage,N'') AS [Dosage], b.ExpiryDate AS [Expiry Date],
       DATEDIFF(day,CONVERT(date,GETDATE()),b.ExpiryDate) AS [Days Remaining], CAST(b.QuantityOnHand AS DECIMAL(18,3)) AS [Quantity],
       COALESCE(NULLIF(b.Unit,N''),N'Unit') AS [Unit], CAST(b.QuantityOnHand*b.UnitCost AS DECIMAL(18,2)) AS [Value at Risk],
       CASE WHEN b.ExpiryDate<CONVERT(date,GETDATE()) THEN N'Expired' ELSE N'Expiring' END AS [Risk]
FROM dbo.StockBatches b INNER JOIN {MEDICINE_TABLE} m ON m.Med_ID=b.Med_ID
WHERE m.IsActive=1 AND b.QuantityOnHand>0 AND b.ExpiryDate IS NOT NULL
 AND (b.ExpiryDate<CONVERT(date,GETDATE()) OR b.ExpiryDate<=DATEADD(day,@Days,CONVERT(date,GETDATE())))
 AND (@Risk=N'All' OR (@Risk=N'Expired' AND b.ExpiryDate<CONVERT(date,GETDATE())) OR (@Risk=N'Expiring' AND b.ExpiryDate>=CONVERT(date,GETDATE())))
ORDER BY b.ExpiryDate, [Medicine];", cmd =>
    {
        cmd.Parameters.Add("@Days", SqlDbType.Int).Value = Math.Clamp(expiryDays, 1, 3650);
        cmd.Parameters.Add("@Risk", SqlDbType.NVarChar, 20).Value = risk;
    });

    public static DataTable GetBatchDetails(string medicineId) => Query(@"
SELECT b.BatchNo AS [Batch No.], b.ExpiryDate AS [Expiry Date], CAST(b.QuantityOnHand AS DECIMAL(18,3)) AS [Quantity On Hand],
       COALESCE(NULLIF(b.Unit,N''),N'Unit') AS [Unit], CAST(b.UnitCost AS DECIMAL(18,4)) AS [Unit Cost],
       CAST(b.QuantityOnHand*b.UnitCost AS DECIMAL(18,2)) AS [Batch Value],
       CASE WHEN b.ExpiryDate<CONVERT(date,GETDATE()) THEN N'Expired'
            WHEN b.ExpiryDate<=DATEADD(day,30,CONVERT(date,GETDATE())) THEN N'Expiring Soon' ELSE N'Usable' END AS [Batch Status]
FROM dbo.StockBatches b WHERE b.Med_ID=@MedId ORDER BY CASE WHEN b.ExpiryDate IS NULL THEN 1 ELSE 0 END,b.ExpiryDate,b.BatchNo;", cmd => cmd.Parameters.Add("@MedId", SqlDbType.NVarChar, 20).Value = medicineId);

    public static DataTable GetSummary(int expiryDays = 30) => Query(@"
SELECT COUNT(*) TotalMedicines,
 SUM(CASE WHEN ISNULL(q.UsableQty,0)>0 AND ISNULL(q.UsableQty,0)<=COALESCE(m.Reorder_Level,0) THEN 1 ELSE 0 END) LowStock,
 SUM(CASE WHEN ISNULL(q.UsableQty,0)<=0 THEN 1 ELSE 0 END) OutOfStock,
 SUM(CASE WHEN ISNULL(q.ExpiredQty,0)>0 THEN 1 ELSE 0 END) WithExpired,
 SUM(CASE WHEN ISNULL(q.ExpiringQty,0)>0 THEN 1 ELSE 0 END) WithExpiring,
 CAST(SUM(ISNULL(q.UsableQty,0)) AS DECIMAL(18,3)) UsableUnits,
 CAST(SUM(ISNULL(q.StockValue,0)) AS DECIMAL(18,2)) InventoryValue,
 CAST(SUM(ISNULL(q.ExpiredQty,0)) AS DECIMAL(18,3)) ExpiredUnits
FROM {MEDICINE_TABLE} m
OUTER APPLY (SELECT SUM(CASE WHEN b.ExpiryDate IS NULL OR b.ExpiryDate>=CONVERT(date,GETDATE()) THEN b.QuantityOnHand ELSE 0 END) UsableQty,
 SUM(CASE WHEN b.ExpiryDate<CONVERT(date,GETDATE()) THEN b.QuantityOnHand ELSE 0 END) ExpiredQty,
 SUM(CASE WHEN b.ExpiryDate IS NULL OR b.ExpiryDate>=CONVERT(date,GETDATE()) THEN b.QuantityOnHand*b.UnitCost ELSE 0 END) StockValue,
 SUM(CASE WHEN b.ExpiryDate>=CONVERT(date,GETDATE()) AND b.ExpiryDate<=DATEADD(day,@Days,CONVERT(date,GETDATE())) THEN b.QuantityOnHand ELSE 0 END) ExpiringQty
 FROM dbo.StockBatches b WHERE b.Med_ID=m.Med_ID) q WHERE m.IsActive=1;", cmd => cmd.Parameters.Add("@Days", SqlDbType.Int).Value = Math.Clamp(expiryDays, 1, 3650));

    public static DataTable GetMovementTrend(DateTime start, DateTime end) => Query(@"
SELECT CONVERT(date,TransactionDate) [Movement Date],
 SUM(CASE WHEN TransactionType=N'Stock In' THEN Quantity ELSE 0 END) [Stock In],
 SUM(CASE WHEN TransactionType IN (N'Stock Out',N'Stock Out - Expired',N'Stock Out - Disposal',N'Stock Out - Damaged') THEN Quantity ELSE 0 END) [Stock Out],
 SUM(CASE WHEN TransactionType=N'Stock In' THEN 1 ELSE 0 END) [Stock In Transactions],
 SUM(CASE WHEN TransactionType IN (N'Stock Out',N'Stock Out - Expired',N'Stock Out - Disposal',N'Stock Out - Damaged') THEN 1 ELSE 0 END) [Stock Out Transactions]
FROM dbo.StockMovements WHERE TransactionDate>=@Start AND TransactionDate<DATEADD(day,1,@End)
GROUP BY CONVERT(date,TransactionDate) ORDER BY [Movement Date];", cmd =>
    {
        cmd.Parameters.Add("@Start", SqlDbType.Date).Value = start.Date;
        cmd.Parameters.Add("@End", SqlDbType.Date).Value = end.Date;
    });

    public static DataTable GetTopMovers(DateTime start, DateTime end, string direction = "In", string ranking = "Frequency") => Query(@"
SELECT TOP (10) sm.Med_ID AS [Medicine ID], COALESCE(NULLIF(m.Generic_Name,N''),NULLIF(m.MedicineName,N''),N'') AS [Medicine],
 COUNT(*) AS [Transactions], CAST(SUM(sm.Quantity) AS DECIMAL(18,3)) AS [Total Quantity], COALESCE(MAX(sm.Unit),N'Unit') AS [Unit]
FROM dbo.StockMovements sm INNER JOIN {MEDICINE_TABLE} m ON m.Med_ID=sm.Med_ID
WHERE sm.TransactionDate>=@Start AND sm.TransactionDate<DATEADD(day,1,@End)
 AND ((@Direction=N'In' AND sm.TransactionType=N'Stock In') OR (@Direction=N'Out' AND sm.TransactionType IN (N'Stock Out',N'Stock Out - Expired',N'Stock Out - Disposal',N'Stock Out - Damaged')))
GROUP BY sm.Med_ID,m.Generic_Name,m.MedicineName
ORDER BY CASE WHEN @Ranking=N'Frequency' THEN COUNT(*) ELSE SUM(sm.Quantity) END DESC, [Medicine];", cmd =>
    {
        cmd.Parameters.Add("@Start", SqlDbType.Date).Value = start.Date;
        cmd.Parameters.Add("@End", SqlDbType.Date).Value = end.Date;
        cmd.Parameters.Add("@Direction", SqlDbType.NVarChar, 10).Value = direction;
        cmd.Parameters.Add("@Ranking", SqlDbType.NVarChar, 20).Value = ranking;
    });
}
