using System.ComponentModel;
using System.Security.Cryptography;
using InventoryMonitoringSystem.Data;
using Microsoft.Data.SqlClient;

namespace InventoryMonitoringSystem
{
    public partial class FirstSetupForm : Form
    {
        private FormZoomLayout? _zoomLayout;
        public FirstSetupForm()
        {
            InitializeComponent();
            KeyPreview = true;
            ApplyResponsiveSize();
            btnTogglePassword.Image = CreateEyeIcon(false);
            btnTogglePassword.AccessibleName = "Show password";
            btnToggleConfirmPassword.Image = CreateEyeIcon(false);
            btnToggleConfirmPassword.AccessibleName = "Show password";
        }

        private void btnCreateAdministrator_Click(
            object sender,
            EventArgs e)
        {
            string firstName = txtFirstName.Text.Trim();
            string lastName = txtLastName.Text.Trim();
            string email = txtEmail.Text.Trim();
            string contactNumber = txtContactNumber.Text.Trim();
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;
            string confirmPassword = txtConfirmPassword.Text;

            if (string.IsNullOrWhiteSpace(firstName))
            {
                MessageBox.Show("Please enter the First Name.", "First Setup",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtFirstName.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(lastName))
            {
                MessageBox.Show("Please enter the Last Name.", "First Setup",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtLastName.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(username))
            {
                MessageBox.Show("Please enter a username.", "First Setup",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUsername.Focus();
                return;
            }

            if (username.Length < 3)
            {
                MessageBox.Show("Username must contain at least 3 characters.", "First Setup",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUsername.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter a password.", "First Setup",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPassword.Focus();
                return;
            }

            if (password.Length < 6)
            {
                MessageBox.Show("Password must contain at least 6 characters.", "First Setup",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPassword.Focus();
                return;
            }

            if (password != confirmPassword)
            {
                MessageBox.Show("The passwords do not match.", "First Setup",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtConfirmPassword.Focus();
                return;
            }

            try
            {
                CreateAdministrator(
                    firstName,
                    lastName,
                    email,
                    contactNumber,
                    username,
                    password);

                MessageBox.Show(
                    "Administrator account created successfully.\n\n" +
                    "You can now log in using your Administrator account.",
                    "First Setup Complete",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                Hide();

                LoginForm loginForm = new LoginForm();
                loginForm.FormClosed += (s, args) => Close();
                loginForm.Show();
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2601 || ex.Number == 2627)
                {
                    MessageBox.Show(
                        "That username already exists.\n\nPlease choose another username.",
                        "Username Already Exists",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    txtUsername.Focus();
                }
                else
                {
                    MessageBox.Show(
                        "The Administrator account could not be created.\n\n" + ex.Message,
                        "First Setup Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "The Administrator account could not be created.\n\n" + ex.Message,
                    "First Setup Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private static void CreateAdministrator(
            string firstName,
            string lastName,
            string email,
            string contactNumber,
            string username,
            string password)
        {
            string passwordHash = HashPassword(password);

            using SqlConnection connection =
                new SqlConnection(DatabaseConnection.DatabaseConnectionString);

            connection.Open();

            const string sql = @"
INSERT INTO dbo.Users
(
    Username,
    PasswordHash,
    Role,
    FirstName,
    LastName,
    Email,
    ContactNumber,
    IsActive
)
VALUES
(
    @Username,
    @PasswordHash,
    'Administrator',
    @FirstName,
    @LastName,
    NULLIF(@Email, ''),
    NULLIF(@ContactNumber, ''),
    1
);";

            using SqlCommand command = new SqlCommand(sql, connection);

            command.Parameters.Add("@Username", System.Data.SqlDbType.NVarChar, 50).Value = username;
            command.Parameters.Add("@PasswordHash", System.Data.SqlDbType.NVarChar, 255).Value = passwordHash;
            command.Parameters.Add("@FirstName", System.Data.SqlDbType.NVarChar, 100).Value = firstName;
            command.Parameters.Add("@LastName", System.Data.SqlDbType.NVarChar, 100).Value = lastName;
            command.Parameters.Add("@Email", System.Data.SqlDbType.NVarChar, 150).Value = email;
            command.Parameters.Add("@ContactNumber", System.Data.SqlDbType.NVarChar, 50).Value = contactNumber;

            command.ExecuteNonQuery();
        }

        private static string HashPassword(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(16);

            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                100_000,
                HashAlgorithmName.SHA256,
                32);

            return Convert.ToBase64String(salt) + ":" +
                   Convert.ToBase64String(hash);
        }
        private void btnTogglePassword_Click(object? sender, EventArgs e)
        {
            bool show = txtPassword.PasswordChar != '\0';
            txtPassword.PasswordChar = show ? '\0' : '●';
            btnTogglePassword.Image = CreateEyeIcon(show);
            btnTogglePassword.AccessibleName = show ? "Hide password" : "Show password";
            txtPassword.Focus();
            txtPassword.SelectionStart = txtPassword.TextLength;
        }

        private void btnToggleConfirmPassword_Click(object? sender, EventArgs e)
        {
            bool show = txtConfirmPassword.PasswordChar != '\0';
            txtConfirmPassword.PasswordChar = show ? '\0' : '●';
            btnToggleConfirmPassword.Image = CreateEyeIcon(show);
            btnToggleConfirmPassword.AccessibleName = show ? "Hide password" : "Show password";
            txtConfirmPassword.Focus();
            txtConfirmPassword.SelectionStart = txtConfirmPassword.TextLength;
        }

        private static Bitmap CreateEyeIcon(bool visible)
        {
            Bitmap bitmap = new Bitmap(22, 18);
            using Graphics g = Graphics.FromImage(bitmap);
            g.Clear(Color.Transparent);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using Pen pen = new Pen(Color.FromArgb(18, 87, 55), 1.7F);
            g.DrawEllipse(pen, new Rectangle(2, 4, 18, 10));
            using Brush brush = new SolidBrush(Color.FromArgb(18, 87, 55));
            g.FillEllipse(brush, new Rectangle(8, 7, 6, 6));
            if (!visible)
            {
                using Pen slash = new Pen(Color.FromArgb(18, 87, 55), 1.8F);
                g.DrawLine(slash, 3, 2, 19, 16);
            }
            return bitmap;
        }

        private void ApplyResponsiveSize()
        {
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                return;

            // Uniformly zoom the original 960x576 composition for fullscreen and windowed sizes.
            _zoomLayout = new FormZoomLayout(this, new Size(960, 576));
            EnterKioskMode();
        }

        private void EnterKioskMode()
        {
            Rectangle screenBounds = Screen.FromControl(this).Bounds;
            SuspendLayout();
            FormBorderStyle = FormBorderStyle.None;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.Manual;
            TopMost = true;
            Bounds = screenBounds;
            WindowState = FormWindowState.Maximized;
            ResumeLayout(true);
            _zoomLayout?.Apply();
        }

        private void ExitKioskMode()
        {
            SuspendLayout();
            TopMost = false;
            WindowState = FormWindowState.Normal;
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = true;
            MinimizeBox = true;
            Size = new Size(960, 576);
            StartPosition = FormStartPosition.Manual;
            Rectangle area = Screen.FromControl(this).WorkingArea;
            Location = new Point(area.Left + Math.Max(0, (area.Width - Width) / 2),
                                 area.Top + Math.Max(0, (area.Height - Height) / 2));
            ResumeLayout(true);
            _zoomLayout?.Apply();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                ExitKioskMode();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.F4)
            {
                EnterKioskMode();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            base.OnKeyDown(e);
        }
    }
}
