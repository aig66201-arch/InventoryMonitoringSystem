using System.ComponentModel;
using InventoryMonitoringSystem.Services;

namespace InventoryMonitoringSystem
{
    public partial class LoginForm : Form
    {
        private FormZoomLayout? _zoomLayout;
        public LoginForm()
        {
            InitializeComponent();
            KeyPreview = true;
            ApplyResponsiveSize();
            txtUsername.KeyDown += TxtUsername_KeyDown;
            txtPassword.KeyDown += TxtPassword_KeyDown;
            btnTogglePassword.Image = CreateEyeIcon(false);
            btnTogglePassword.AccessibleName = "Show password";
        }


        private void TxtUsername_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;
            e.Handled = true;
            e.SuppressKeyPress = true;
            txtPassword.Focus();
            txtPassword.SelectionStart = txtPassword.TextLength;
        }

        private void TxtPassword_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;
            e.Handled = true;
            e.SuppressKeyPress = true;
            btnLogin.PerformClick();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrWhiteSpace(username))
            {
                MessageBox.Show(
                    "Please enter your username.",
                    "Login",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                txtUsername.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show(
                    "Please enter your password.",
                    "Login",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                txtPassword.Focus();
                return;
            }

            try
            {
                LoginResult result = AuthenticationService.Login(username, password);

                if (!result.Success)
                {
                    MessageBox.Show(
                        result.Message,
                        "Login Failed",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    txtPassword.Clear();
                    txtPassword.Focus();
                    return;
                }

                OpenMainForm(result);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "An error occurred while trying to log in.\n\n" + ex.Message,
                    "Login Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void OpenMainForm(LoginResult result)
        {
            // Create user session object from login result
            UserSession session = new UserSession(
                result.UserID.ToString(),
                result.Username,
                string.IsNullOrWhiteSpace(result.FullName) ? result.Username : result.FullName,
                result.Role
            );

            // Hide Login Form and open MainForm
            Hide();
            using (MainForm mainForm = new MainForm(session))
            {
                DialogResult dialogResult = mainForm.ShowDialog();

                // Clear input fields
                txtPassword.Clear();

                // If user signed out, show the login form again
                if (dialogResult == DialogResult.OK)
                {
                    Show();
                    txtUsername.Focus();
                }
                else
                {
                    // If user closed the window directly, terminate the application
                    Close();
                }
            }
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
        private void txtUsername_TextChanged(object sender, EventArgs e)
        {
        }
    }
}