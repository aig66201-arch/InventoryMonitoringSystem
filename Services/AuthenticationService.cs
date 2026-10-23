using System.Security.Cryptography;
using InventoryMonitoringSystem.Data;
using Microsoft.Data.SqlClient;

namespace InventoryMonitoringSystem.Services
{
    public static class AuthenticationService
    {
        public static bool AdministratorExists()
        {
            using SqlConnection connection =
                new SqlConnection(DatabaseConnection.DatabaseConnectionString);

            connection.Open();

            const string sql = @"
SELECT COUNT(*)
FROM dbo.Users
WHERE Role = 'Administrator'
  AND IsActive = 1;";

            using SqlCommand command = new SqlCommand(sql, connection);
            int count = Convert.ToInt32(command.ExecuteScalar());

            return count > 0;
        }

        public static LoginResult Login(
            string username,
            string password)
        {
            using SqlConnection connection =
                new SqlConnection(DatabaseConnection.DatabaseConnectionString);

            connection.Open();

            const string sql = @"
SELECT UserID, Username, PasswordHash, Role, FirstName, LastName
FROM dbo.Users
WHERE Username = @Username
  AND IsActive = 1;";

            using SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.Add("@Username", System.Data.SqlDbType.NVarChar, 50).Value = username;

            using SqlDataReader reader = command.ExecuteReader();

            if (!reader.Read())
            {
                return new LoginResult
                {
                    Success = false,
                    Message = "Invalid username or password."
                };
            }

            int userID = Convert.ToInt32(reader["UserID"]);
            string storedUsername = Convert.ToString(reader["Username"]) ?? string.Empty;
            string storedPasswordHash = Convert.ToString(reader["PasswordHash"]) ?? string.Empty;
            string role = Convert.ToString(reader["Role"]) ?? string.Empty;
            string firstName = Convert.ToString(reader["FirstName"]) ?? string.Empty;
            string lastName = Convert.ToString(reader["LastName"]) ?? string.Empty;
            string fullName = (firstName + " " + lastName).Trim();
            if (string.IsNullOrWhiteSpace(fullName)) fullName = storedUsername;

            if (!VerifyPassword(password, storedPasswordHash))
            {
                return new LoginResult
                {
                    Success = false,
                    Message = "Invalid username or password."
                };
            }

            return new LoginResult
            {
                Success = true,
                UserID = userID,
                Username = storedUsername,
                FullName = fullName,
                Role = role,
                Message = "Login successful."
            };
        }

        private static bool VerifyPassword(
            string password,
            string storedPassword)
        {
            try
            {
                string[] parts = storedPassword.Split(':');

                if (parts.Length != 2)
                    return false;

                byte[] salt = Convert.FromBase64String(parts[0]);
                byte[] storedHash = Convert.FromBase64String(parts[1]);

                byte[] calculatedHash =
                    Rfc2898DeriveBytes.Pbkdf2(
                        password,
                        salt,
                        100_000,
                        HashAlgorithmName.SHA256,
                        32);

                return CryptographicOperations.FixedTimeEquals(
                    calculatedHash,
                    storedHash);
            }
            catch
            {
                return false;
            }
        }
    }
}
