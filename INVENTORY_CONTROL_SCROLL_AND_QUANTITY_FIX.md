Inventory Control layout update

- Moved vertical scrolling to one shared scroll container for the overall Inventory Control content.
- Removed nested vertical scrolling from Overview and Movement Analytics; charts are given fixed, usable heights and pages use the shared content scrollbar.
- Stock Register and Expiry & Risk grids keep their native row scrolling as needed.
- Stock Register columns Physical Stock, Usable Stock, Expired Qty, and Expiring Qty display whole numbers with no decimal places. Underlying database precision is not modified.

Open InventoryMonitoringSystem.sln in Visual Studio and rebuild to validate against the local SQL Server environment.
