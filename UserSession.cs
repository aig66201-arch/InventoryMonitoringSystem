using System;

namespace InventoryMonitoringSystem
{
    public class UserSession(string userId, string username, string fullName, string role)
    {
        public string UserId { get; set; } = userId;
        public string Username { get; set; } = username;
        public string FullName { get; set; } =
            string.IsNullOrWhiteSpace(fullName) ? username : fullName;
        public string Role { get; set; } = role;

        public bool IsAdministration =>
            string.Equals(Role, "Administrator", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(Role, "Administration", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(Role, "Admin", StringComparison.OrdinalIgnoreCase);
    }
}
