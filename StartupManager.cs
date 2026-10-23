using InventoryMonitoringSystem.Services;

namespace InventoryMonitoringSystem
{
    public static class StartupManager
    {
        public static Form GetStartupForm()
        {
            if (AuthenticationService.AdministratorExists())
            {
                return new LoginForm();
            }

            return new FirstSetupForm();
        }
    }
}
