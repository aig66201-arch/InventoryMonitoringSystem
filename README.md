# Inventory Monitoring System

WinForms inventory monitoring application with medicine catalog, dashboard, stock movement history, batch tracking, and setup/login screens.

For the latest Stock Movement changes, see `STOCK_MOVEMENT_SETUP.md`.

## Inventory Control adaptive layout

- Overview and Movement Analytics use vertically scrollable layouts with minimum chart heights to prevent charts from being compressed on short displays.
- Inventory tables use content-aware column sizing: numeric/date columns stay compact, medicine-name columns grow for longer names, and widths are recalculated when the grid is resized.
- Tables retain vertical scrolling for long datasets and horizontal scrolling when all columns cannot fit on narrow displays.
- The chart and table data still come from the existing Inventory Control services and database.

Build in Visual Studio 2026 with the .NET 10 Windows Desktop workload installed.
