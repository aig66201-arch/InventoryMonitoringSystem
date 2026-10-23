namespace InventoryMonitoringSystem
{
    partial class LoginForm
    {
        private System.ComponentModel.IContainer components = null;
        private Panel pnlBrand;
        private Panel pnlContent;
        private Label lblBrand;
        private Label lblBrandSub;
        private Label lblTitle;
        private Label lblSubtitle;
        private Label lblUsername;
        private Label lblPassword;
        private TextBox txtUsername;
        private TextBox txtPassword;
        private Panel pnlPassword;
        private Button btnTogglePassword;
        private Button btnLogin;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            pnlBrand = new Panel();
            lblBrandSub = new Label();
            lblBrand = new Label();
            pnlContent = new Panel();
            lblTitle = new Label();
            lblSubtitle = new Label();
            lblUsername = new Label();
            txtUsername = new TextBox();
            lblPassword = new Label();
            pnlPassword = new Panel();
            txtPassword = new TextBox();
            btnTogglePassword = new Button();
            btnLogin = new Button();
            pnlBrand.SuspendLayout();
            pnlContent.SuspendLayout();
            pnlPassword.SuspendLayout();
            SuspendLayout();
            // 
            // pnlBrand
            // 
            pnlBrand.BackColor = Color.FromArgb(18, 87, 55);
            pnlBrand.Controls.Add(lblBrandSub);
            pnlBrand.Controls.Add(lblBrand);
            pnlBrand.Dock = DockStyle.None;
            pnlBrand.Location = new Point(0, 0);
            pnlBrand.Name = "pnlBrand";
            pnlBrand.Size = new Size(374, 576);
            pnlBrand.TabIndex = 0;
            // 
            // lblBrandSub
            // 
            lblBrandSub.Font = new Font("Segoe UI", 10F);
            lblBrandSub.ForeColor = Color.FromArgb(220, 242, 230);
            lblBrandSub.Location = new Point(41, 285);
            lblBrandSub.Name = "lblBrandSub";
            lblBrandSub.Size = new Size(292, 54);
            lblBrandSub.TabIndex = 1;
            lblBrandSub.Text = "Secure inventory management\r\nfor your organization.";
            // 
            // lblBrand
            // 
            lblBrand.BackColor = Color.Transparent;
            lblBrand.Font = new Font("Segoe UI", 24F, FontStyle.Bold);
            lblBrand.ForeColor = Color.White;
            lblBrand.Location = new Point(41, 118);
            lblBrand.Name = "lblBrand";
            lblBrand.Size = new Size(298, 167);
            lblBrand.TabIndex = 0;
            lblBrand.Text = "Inventory Monitoring\r\nSystem";
            lblBrand.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // pnlContent
            // 
            pnlContent.BackColor = Color.White;
            pnlContent.Controls.Add(lblTitle);
            pnlContent.Controls.Add(lblSubtitle);
            pnlContent.Controls.Add(lblUsername);
            pnlContent.Controls.Add(txtUsername);
            pnlContent.Controls.Add(lblPassword);
            pnlContent.Controls.Add(pnlPassword);
            pnlContent.Controls.Add(btnLogin);
            pnlContent.Dock = DockStyle.None;
            pnlContent.Location = new Point(374, 0);
            pnlContent.Name = "pnlContent";
            pnlContent.Size = new Size(586, 576);
            pnlContent.TabIndex = 1;
            // 
            // lblTitle
            // 
            lblTitle.Font = new Font("Segoe UI", 24F, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(18, 87, 55);
            lblTitle.Location = new Point(58, 82);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(470, 44);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Welcome back";
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblSubtitle
            // 
            lblSubtitle.Font = new Font("Segoe UI", 10F);
            lblSubtitle.ForeColor = Color.FromArgb(105, 118, 111);
            lblSubtitle.Location = new Point(60, 128);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(468, 38);
            lblSubtitle.TabIndex = 1;
            lblSubtitle.Text = "Sign in to continue to your inventory dashboard.";
            lblSubtitle.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblUsername
            // 
            lblUsername.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
            lblUsername.ForeColor = Color.FromArgb(44, 58, 50);
            lblUsername.Location = new Point(60, 190);
            lblUsername.Name = "lblUsername";
            lblUsername.Size = new Size(468, 24);
            lblUsername.TabIndex = 2;
            lblUsername.Text = "Username";
            lblUsername.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtUsername
            // 
            txtUsername.BackColor = Color.FromArgb(248, 250, 249);
            txtUsername.BorderStyle = BorderStyle.FixedSingle;
            txtUsername.Font = new Font("Segoe UI", 11F);
            txtUsername.ForeColor = Color.FromArgb(35, 45, 40);
            txtUsername.Location = new Point(60, 218);
            txtUsername.Name = "txtUsername";
            txtUsername.Size = new Size(468, 32);
            txtUsername.TabIndex = 0;
            // 
            // lblPassword
            // 
            lblPassword.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
            lblPassword.ForeColor = Color.FromArgb(44, 58, 50);
            lblPassword.Location = new Point(60, 274);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new Size(468, 24);
            lblPassword.TabIndex = 3;
            lblPassword.Text = "Password";
            lblPassword.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // pnlPassword
            // 
            pnlPassword.BackColor = Color.FromArgb(248, 250, 249);
            pnlPassword.BorderStyle = BorderStyle.FixedSingle;
            pnlPassword.Controls.Add(txtPassword);
            pnlPassword.Controls.Add(btnTogglePassword);
            pnlPassword.Location = new Point(60, 302);
            pnlPassword.Name = "pnlPassword";
            pnlPassword.Size = new Size(468, 40);
            pnlPassword.TabIndex = 4;
            // 
            // txtPassword
            // 
            txtPassword.BackColor = Color.FromArgb(248, 250, 249);
            txtPassword.BorderStyle = BorderStyle.None;
            txtPassword.Font = new Font("Segoe UI", 11F);
            txtPassword.ForeColor = Color.FromArgb(35, 45, 40);
            txtPassword.Location = new Point(12, 7);
            txtPassword.Name = "txtPassword";
            txtPassword.PasswordChar = '●';
            txtPassword.Size = new Size(408, 25);
            txtPassword.TabIndex = 1;
            // 
            // btnTogglePassword
            // 
            btnTogglePassword.BackColor = Color.Transparent;
            btnTogglePassword.Cursor = Cursors.Hand;
            btnTogglePassword.FlatAppearance.BorderSize = 0;
            btnTogglePassword.FlatAppearance.MouseDownBackColor = Color.FromArgb(220, 239, 228);
            btnTogglePassword.FlatAppearance.MouseOverBackColor = Color.FromArgb(232, 245, 237);
            btnTogglePassword.FlatStyle = FlatStyle.Flat;
            btnTogglePassword.Location = new Point(426, 3);
            btnTogglePassword.Name = "btnTogglePassword";
            btnTogglePassword.Size = new Size(38, 32);
            btnTogglePassword.TabIndex = 2;
            btnTogglePassword.TabStop = false;
            btnTogglePassword.UseVisualStyleBackColor = false;
            btnTogglePassword.Click += btnTogglePassword_Click;
            // 
            // btnLogin
            // 
            btnLogin.BackColor = Color.FromArgb(22, 119, 72);
            btnLogin.Cursor = Cursors.Hand;
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.FlatAppearance.MouseDownBackColor = Color.FromArgb(12, 79, 46);
            btnLogin.FlatAppearance.MouseOverBackColor = Color.FromArgb(28, 139, 82);
            btnLogin.FlatStyle = FlatStyle.Flat;
            btnLogin.Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold);
            btnLogin.ForeColor = Color.White;
            btnLogin.Location = new Point(60, 374);
            btnLogin.Name = "btnLogin";
            btnLogin.Size = new Size(468, 48);
            btnLogin.TabIndex = 3;
            btnLogin.Text = "SIGN IN";
            btnLogin.UseVisualStyleBackColor = false;
            btnLogin.Click += btnLogin_Click;
            // 
            // LoginForm
            // 
            AcceptButton = btnLogin;
            AutoScaleMode = AutoScaleMode.None;
            BackColor = Color.White;
            ClientSize = new Size(960, 576);
            Controls.Add(pnlContent);
            Controls.Add(pnlBrand);
            Font = new Font("Segoe UI", 10F);
            FormBorderStyle = FormBorderStyle.None;
            MaximizeBox = false;
            MinimumSize = new Size(720, 432);
            Name = "LoginForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Inventory Monitoring System - Login";
            pnlBrand.ResumeLayout(false);
            pnlContent.ResumeLayout(false);
            pnlContent.PerformLayout();
            pnlPassword.ResumeLayout(false);
            pnlPassword.PerformLayout();
            ResumeLayout(false);
        }
    }
}
