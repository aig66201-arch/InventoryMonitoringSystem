namespace InventoryMonitoringSystem
{
    partial class FirstSetupForm
    {
        private System.ComponentModel.IContainer components = null;
        private Panel pnlBrand;
        private Panel pnlContent;
        private Label lblBrand;
        private Label lblBrandSub;
        private Label lblTitle;
        private Label lblSubtitle;
        private Label lblFirstName;
        private Label lblLastName;
        private Label lblEmail;
        private Label lblContactNumber;
        private Label lblUsername;
        private Label lblPassword;
        private Label lblConfirmPassword;
        private TextBox txtFirstName;
        private TextBox txtLastName;
        private TextBox txtEmail;
        private TextBox txtContactNumber;
        private TextBox txtUsername;
        private TextBox txtPassword;
        private TextBox txtConfirmPassword;
        private Panel pnlPassword;
        private Panel pnlConfirmPassword;
        private Button btnTogglePassword;
        private Button btnToggleConfirmPassword;
        private Button btnCreateAdministrator;

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
            lblFirstName = new Label();
            txtFirstName = new TextBox();
            lblLastName = new Label();
            txtLastName = new TextBox();
            lblEmail = new Label();
            txtEmail = new TextBox();
            lblContactNumber = new Label();
            txtContactNumber = new TextBox();
            lblUsername = new Label();
            txtUsername = new TextBox();
            lblPassword = new Label();
            pnlPassword = new Panel();
            txtPassword = new TextBox();
            btnTogglePassword = new Button();
            lblConfirmPassword = new Label();
            pnlConfirmPassword = new Panel();
            txtConfirmPassword = new TextBox();
            btnToggleConfirmPassword = new Button();
            btnCreateAdministrator = new Button();
            pnlBrand.SuspendLayout();
            pnlContent.SuspendLayout();
            pnlPassword.SuspendLayout();
            pnlConfirmPassword.SuspendLayout();
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
            pnlBrand.Size = new Size(326, 576);
            pnlBrand.TabIndex = 0;
            // 
            // lblBrandSub
            // 
            lblBrandSub.Font = new Font("Segoe UI", 9.5F);
            lblBrandSub.ForeColor = Color.FromArgb(220, 242, 230);
            lblBrandSub.Location = new Point(35, 286);
            lblBrandSub.Name = "lblBrandSub";
            lblBrandSub.Size = new Size(255, 58);
            lblBrandSub.TabIndex = 1;
            lblBrandSub.Text = "Create your first administrator\r\naccount to secure the system.";
            // 
            // lblBrand
            // 
            lblBrand.Font = new Font("Segoe UI", 22F, FontStyle.Bold);
            lblBrand.ForeColor = Color.White;
            lblBrand.Location = new Point(32, 112);
            lblBrand.Name = "lblBrand";
            lblBrand.Size = new Size(262, 166);
            lblBrand.TabIndex = 0;
            lblBrand.Text = "Inventory Monitoring\r\nSystem";
            lblBrand.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // pnlContent
            // 
            pnlContent.BackColor = Color.White;
            pnlContent.Controls.Add(lblTitle);
            pnlContent.Controls.Add(lblSubtitle);
            pnlContent.Controls.Add(lblFirstName);
            pnlContent.Controls.Add(txtFirstName);
            pnlContent.Controls.Add(lblLastName);
            pnlContent.Controls.Add(txtLastName);
            pnlContent.Controls.Add(lblEmail);
            pnlContent.Controls.Add(txtEmail);
            pnlContent.Controls.Add(lblContactNumber);
            pnlContent.Controls.Add(txtContactNumber);
            pnlContent.Controls.Add(lblUsername);
            pnlContent.Controls.Add(txtUsername);
            pnlContent.Controls.Add(lblPassword);
            pnlContent.Controls.Add(pnlPassword);
            pnlContent.Controls.Add(lblConfirmPassword);
            pnlContent.Controls.Add(pnlConfirmPassword);
            pnlContent.Controls.Add(btnCreateAdministrator);
            pnlContent.Dock = DockStyle.None;
            pnlContent.Location = new Point(326, 0);
            pnlContent.Name = "pnlContent";
            pnlContent.Size = new Size(634, 576);
            pnlContent.TabIndex = 1;
            // 
            // lblTitle
            // 
            lblTitle.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(18, 87, 55);
            lblTitle.Location = new Point(42, 28);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(550, 38);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Administrator Setup";
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblSubtitle
            // 
            lblSubtitle.Font = new Font("Segoe UI", 9.5F);
            lblSubtitle.ForeColor = Color.FromArgb(105, 118, 111);
            lblSubtitle.Location = new Point(44, 68);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(548, 30);
            lblSubtitle.TabIndex = 1;
            lblSubtitle.Text = "Create the first Administrator account for this system.";
            lblSubtitle.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblFirstName
            // 
            lblFirstName.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
            lblFirstName.ForeColor = Color.FromArgb(44, 58, 50);
            lblFirstName.Location = new Point(44, 112);
            lblFirstName.Name = "lblFirstName";
            lblFirstName.Size = new Size(258, 22);
            lblFirstName.TabIndex = 2;
            lblFirstName.Text = "First Name";
            lblFirstName.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtFirstName
            // 
            txtFirstName.BackColor = Color.FromArgb(248, 250, 249);
            txtFirstName.BorderStyle = BorderStyle.FixedSingle;
            txtFirstName.Font = new Font("Segoe UI", 10.5F);
            txtFirstName.ForeColor = Color.FromArgb(35, 45, 40);
            txtFirstName.Location = new Point(44, 137);
            txtFirstName.Name = "txtFirstName";
            txtFirstName.Size = new Size(258, 31);
            txtFirstName.TabIndex = 3;
            // 
            // lblLastName
            // 
            lblLastName.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
            lblLastName.ForeColor = Color.FromArgb(44, 58, 50);
            lblLastName.Location = new Point(320, 112);
            lblLastName.Name = "lblLastName";
            lblLastName.Size = new Size(258, 22);
            lblLastName.TabIndex = 4;
            lblLastName.Text = "Last Name";
            lblLastName.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtLastName
            // 
            txtLastName.BackColor = Color.FromArgb(248, 250, 249);
            txtLastName.BorderStyle = BorderStyle.FixedSingle;
            txtLastName.Font = new Font("Segoe UI", 10.5F);
            txtLastName.ForeColor = Color.FromArgb(35, 45, 40);
            txtLastName.Location = new Point(320, 137);
            txtLastName.Name = "txtLastName";
            txtLastName.Size = new Size(258, 31);
            txtLastName.TabIndex = 5;
            // 
            // lblEmail
            // 
            lblEmail.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
            lblEmail.ForeColor = Color.FromArgb(44, 58, 50);
            lblEmail.Location = new Point(44, 172);
            lblEmail.Name = "lblEmail";
            lblEmail.Size = new Size(258, 22);
            lblEmail.TabIndex = 6;
            lblEmail.Text = "Email (Optional)";
            lblEmail.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtEmail
            // 
            txtEmail.BackColor = Color.FromArgb(248, 250, 249);
            txtEmail.BorderStyle = BorderStyle.FixedSingle;
            txtEmail.Font = new Font("Segoe UI", 10.5F);
            txtEmail.ForeColor = Color.FromArgb(35, 45, 40);
            txtEmail.Location = new Point(44, 197);
            txtEmail.Name = "txtEmail";
            txtEmail.Size = new Size(258, 31);
            txtEmail.TabIndex = 7;
            // 
            // lblContactNumber
            // 
            lblContactNumber.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
            lblContactNumber.ForeColor = Color.FromArgb(44, 58, 50);
            lblContactNumber.Location = new Point(320, 172);
            lblContactNumber.Name = "lblContactNumber";
            lblContactNumber.Size = new Size(258, 22);
            lblContactNumber.TabIndex = 8;
            lblContactNumber.Text = "Contact No. (Optional)";
            lblContactNumber.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtContactNumber
            // 
            txtContactNumber.BackColor = Color.FromArgb(248, 250, 249);
            txtContactNumber.BorderStyle = BorderStyle.FixedSingle;
            txtContactNumber.Font = new Font("Segoe UI", 10.5F);
            txtContactNumber.ForeColor = Color.FromArgb(35, 45, 40);
            txtContactNumber.Location = new Point(320, 197);
            txtContactNumber.Name = "txtContactNumber";
            txtContactNumber.Size = new Size(258, 31);
            txtContactNumber.TabIndex = 9;
            // 
            // lblUsername
            // 
            lblUsername.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
            lblUsername.ForeColor = Color.FromArgb(44, 58, 50);
            lblUsername.Location = new Point(44, 232);
            lblUsername.Name = "lblUsername";
            lblUsername.Size = new Size(258, 22);
            lblUsername.TabIndex = 10;
            lblUsername.Text = "Username";
            lblUsername.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // txtUsername
            // 
            txtUsername.BackColor = Color.FromArgb(248, 250, 249);
            txtUsername.BorderStyle = BorderStyle.FixedSingle;
            txtUsername.Font = new Font("Segoe UI", 10.5F);
            txtUsername.ForeColor = Color.FromArgb(35, 45, 40);
            txtUsername.Location = new Point(44, 257);
            txtUsername.Name = "txtUsername";
            txtUsername.Size = new Size(258, 31);
            txtUsername.TabIndex = 11;
            // 
            // lblPassword
            // 
            lblPassword.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
            lblPassword.ForeColor = Color.FromArgb(44, 58, 50);
            lblPassword.Location = new Point(44, 292);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new Size(258, 22);
            lblPassword.TabIndex = 12;
            lblPassword.Text = "Password";
            lblPassword.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // pnlPassword
            // 
            pnlPassword.BackColor = Color.FromArgb(248, 250, 249);
            pnlPassword.BorderStyle = BorderStyle.FixedSingle;
            pnlPassword.Controls.Add(txtPassword);
            pnlPassword.Controls.Add(btnTogglePassword);
            pnlPassword.Location = new Point(44, 317);
            pnlPassword.Name = "pnlPassword";
            pnlPassword.Size = new Size(258, 36);
            pnlPassword.TabIndex = 8;
            // 
            // txtPassword
            // 
            txtPassword.BackColor = Color.FromArgb(248, 250, 249);
            txtPassword.BorderStyle = BorderStyle.None;
            txtPassword.Font = new Font("Segoe UI", 10.5F);
            txtPassword.ForeColor = Color.FromArgb(35, 45, 40);
            txtPassword.Location = new Point(10, 5);
            txtPassword.Name = "txtPassword";
            txtPassword.PasswordChar = '●';
            txtPassword.Size = new Size(204, 24);
            txtPassword.TabIndex = 5;
            // 
            // btnTogglePassword
            // 
            btnTogglePassword.BackColor = Color.Transparent;
            btnTogglePassword.Cursor = Cursors.Hand;
            btnTogglePassword.FlatAppearance.BorderSize = 0;
            btnTogglePassword.FlatAppearance.MouseDownBackColor = Color.FromArgb(220, 239, 228);
            btnTogglePassword.FlatAppearance.MouseOverBackColor = Color.FromArgb(232, 245, 237);
            btnTogglePassword.FlatStyle = FlatStyle.Flat;
            btnTogglePassword.Location = new Point(214, 1);
            btnTogglePassword.Name = "btnTogglePassword";
            btnTogglePassword.Size = new Size(38, 32);
            btnTogglePassword.TabIndex = 6;
            btnTogglePassword.TabStop = false;
            btnTogglePassword.UseVisualStyleBackColor = false;
            btnTogglePassword.Click += btnTogglePassword_Click;
            // 
            // lblConfirmPassword
            // 
            lblConfirmPassword.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
            lblConfirmPassword.ForeColor = Color.FromArgb(44, 58, 50);
            lblConfirmPassword.Location = new Point(320, 292);
            lblConfirmPassword.Name = "lblConfirmPassword";
            lblConfirmPassword.Size = new Size(258, 22);
            lblConfirmPassword.TabIndex = 13;
            lblConfirmPassword.Text = "Confirm Password";
            lblConfirmPassword.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // pnlConfirmPassword
            // 
            pnlConfirmPassword.BackColor = Color.FromArgb(248, 250, 249);
            pnlConfirmPassword.BorderStyle = BorderStyle.FixedSingle;
            pnlConfirmPassword.Controls.Add(txtConfirmPassword);
            pnlConfirmPassword.Controls.Add(btnToggleConfirmPassword);
            pnlConfirmPassword.Location = new Point(320, 317);
            pnlConfirmPassword.Name = "pnlConfirmPassword";
            pnlConfirmPassword.Size = new Size(258, 36);
            pnlConfirmPassword.TabIndex = 9;
            // 
            // txtConfirmPassword
            // 
            txtConfirmPassword.BackColor = Color.FromArgb(248, 250, 249);
            txtConfirmPassword.BorderStyle = BorderStyle.None;
            txtConfirmPassword.Font = new Font("Segoe UI", 10.5F);
            txtConfirmPassword.ForeColor = Color.FromArgb(35, 45, 40);
            txtConfirmPassword.Location = new Point(10, 5);
            txtConfirmPassword.Name = "txtConfirmPassword";
            txtConfirmPassword.PasswordChar = '●';
            txtConfirmPassword.Size = new Size(204, 24);
            txtConfirmPassword.TabIndex = 7;
            // 
            // btnToggleConfirmPassword
            // 
            btnToggleConfirmPassword.BackColor = Color.Transparent;
            btnToggleConfirmPassword.Cursor = Cursors.Hand;
            btnToggleConfirmPassword.FlatAppearance.BorderSize = 0;
            btnToggleConfirmPassword.FlatAppearance.MouseDownBackColor = Color.FromArgb(220, 239, 228);
            btnToggleConfirmPassword.FlatAppearance.MouseOverBackColor = Color.FromArgb(232, 245, 237);
            btnToggleConfirmPassword.FlatStyle = FlatStyle.Flat;
            btnToggleConfirmPassword.Location = new Point(214, 1);
            btnToggleConfirmPassword.Name = "btnToggleConfirmPassword";
            btnToggleConfirmPassword.Size = new Size(38, 32);
            btnToggleConfirmPassword.TabIndex = 8;
            btnToggleConfirmPassword.TabStop = false;
            btnToggleConfirmPassword.UseVisualStyleBackColor = false;
            btnToggleConfirmPassword.Click += btnToggleConfirmPassword_Click;
            // 
            // btnCreateAdministrator
            // 
            btnCreateAdministrator.BackColor = Color.FromArgb(22, 119, 72);
            btnCreateAdministrator.Cursor = Cursors.Hand;
            btnCreateAdministrator.FlatAppearance.BorderSize = 0;
            btnCreateAdministrator.FlatAppearance.MouseDownBackColor = Color.FromArgb(12, 79, 46);
            btnCreateAdministrator.FlatAppearance.MouseOverBackColor = Color.FromArgb(28, 139, 82);
            btnCreateAdministrator.FlatStyle = FlatStyle.Flat;
            btnCreateAdministrator.Font = new Font("Segoe UI Semibold", 10.5F, FontStyle.Bold);
            btnCreateAdministrator.ForeColor = Color.White;
            btnCreateAdministrator.Location = new Point(44, 388);
            btnCreateAdministrator.Name = "btnCreateAdministrator";
            btnCreateAdministrator.Size = new Size(534, 48);
            btnCreateAdministrator.TabIndex = 9;
            btnCreateAdministrator.Text = "CREATE ADMINISTRATOR";
            btnCreateAdministrator.UseVisualStyleBackColor = false;
            btnCreateAdministrator.Click += btnCreateAdministrator_Click;
            // 
            // FirstSetupForm
            // 
            AcceptButton = btnCreateAdministrator;
            AutoScaleMode = AutoScaleMode.None;
            BackColor = Color.White;
            ClientSize = new Size(960, 576);
            Controls.Add(pnlContent);
            Controls.Add(pnlBrand);
            Font = new Font("Segoe UI", 10F);
            FormBorderStyle = FormBorderStyle.None;
            MaximizeBox = false;
            MinimumSize = new Size(720, 432);
            Name = "FirstSetupForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Inventory Monitoring System - First Setup";
            pnlBrand.ResumeLayout(false);
            pnlContent.ResumeLayout(false);
            pnlContent.PerformLayout();
            pnlPassword.ResumeLayout(false);
            pnlPassword.PerformLayout();
            pnlConfirmPassword.ResumeLayout(false);
            pnlConfirmPassword.PerformLayout();
            ResumeLayout(false);
        }

    }
}
