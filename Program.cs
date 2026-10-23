using InventoryMonitoringSystem.Data;

namespace InventoryMonitoringSystem
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            try
            {
                DatabaseInitializer.Initialize();

                Application.Run(
                    StartupManager.GetStartupForm());
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "The application could not start.\n\n" +
                    ex.Message,
                    "Inventory Monitoring System",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
