#nullable enable
namespace InventoryMonitoringSystem;

partial class MainForm
{
    private System.ComponentModel.IContainer? components = null;
    private Panel sidebar = null!;
    private Panel headerPanel = null!;
    private Label lblMenuHeader = null!;
    private Panel profilePanel = null!;
    private PictureBox picAvatar = null!;
    private Label lblName = null!;
    private Label lblRole = null!;
    private Label lblWorkspace = null!;
    private Label lblAdministration = null!;
    private FlowLayoutPanel workspaceMenu = null!;
    private FlowLayoutPanel administrationMenu = null!;
    private Button btnToggle = null!;
    private Button btnDashboard = null!;
    private Button btnMedicineCatalog = null!;
    private Button btnStockMovement = null!;
    private Button btnInventoryControl = null!;
    private Button btnAnalyticsReports = null!;
    private Button btnMyProfile = null!;
    private Button btnUserAdministration = null!;
    private Button btnActivityLog = null!;
    private Button btnSystemSetting = null!;
    private Button btnSignOut = null!;
    
    // Main Area Components
    private Panel mainArea = null!;
    private Panel topAppBar = null!;
    private Label lblPageTitle = null!;
    private Label lblSystemSubtitle = null!;
    private Panel contentPanel = null!;
    private Panel cardTotalMed = null!;
    private Label lblCard1Val = null!;
    private Label lblCard1Title = null!;
    private Panel cardLowStock = null!;
    private Label lblCard2Val = null!;
    private Label lblCard2Title = null!;
    private Panel cardExpiring = null!;
    private Label lblCard3Val = null!;
    private Label lblCard3Title = null!;

    private System.Windows.Forms.Timer sidebarTimer = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing) components?.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        sidebar = new Panel();
        headerPanel = new Panel();
        lblMenuHeader = new Label();
        profilePanel = new Panel();
        picAvatar = new PictureBox();
        lblName = new Label();
        lblRole = new Label();
        btnToggle = new Button();
        lblWorkspace = new Label();
        lblAdministration = new Label();
        workspaceMenu = new FlowLayoutPanel();
        administrationMenu = new FlowLayoutPanel();
        
        mainArea = new Panel();
        topAppBar = new Panel();
        lblPageTitle = new Label();
        lblSystemSubtitle = new Label();
        contentPanel = new Panel();

        // Cards
        cardTotalMed = new Panel();
        lblCard1Val = new Label();
        lblCard1Title = new Label();
        cardLowStock = new Panel();
        lblCard2Val = new Label();
        lblCard2Title = new Label();
        cardExpiring = new Panel();
        lblCard3Val = new Label();
        lblCard3Title = new Label();

        sidebarTimer = new(components);

        btnDashboard = CreateNavButton("Dashboard");
        btnMedicineCatalog = CreateNavButton("Medicine Catalog");
        btnStockMovement = CreateNavButton("Stock Movement");
        btnInventoryControl = CreateNavButton("Inventory Control");
        btnAnalyticsReports = CreateNavButton("Analytics Reports");
        btnMyProfile = CreateNavButton("My Profile");
        btnUserAdministration = CreateNavButton("User Administration");
        btnActivityLog = CreateNavButton("Activity Log");
        btnSystemSetting = CreateNavButton("System Setting");
        btnSignOut = CreateNavButton("Sign Out");

        SuspendLayout();
        sidebar.SuspendLayout();
        profilePanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)picAvatar).BeginInit();
        workspaceMenu.SuspendLayout();
        administrationMenu.SuspendLayout();
        mainArea.SuspendLayout();
        topAppBar.SuspendLayout();
        contentPanel.SuspendLayout();
        cardTotalMed.SuspendLayout();
        cardLowStock.SuspendLayout();
        cardExpiring.SuspendLayout();

        // Form settings
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1380, 850);
        MinimumSize = new Size(1024, 600);
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Barangay & Municipal Health Office - Inventory Monitoring System";
        BackColor = Color.FromArgb(243, 244, 246);
        Font = new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point);

        // Sidebar Main Panel (Deep Forest/Emerald Green)
        sidebar.BackColor = Color.FromArgb(6, 78, 59);
        sidebar.Dock = DockStyle.Left;
        sidebar.Width = 280;
        sidebar.Padding = new Padding(14, 14, 14, 14);

        // Header Panel inside Sidebar
        headerPanel.BackColor = Color.FromArgb(2, 44, 34);
        headerPanel.Dock = DockStyle.Top;
        headerPanel.Height = 60;

        lblMenuHeader.AutoSize = false;
        lblMenuHeader.Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold);
        lblMenuHeader.ForeColor = Color.FromArgb(236, 253, 245);
        lblMenuHeader.Location = new Point(14, 0);
        lblMenuHeader.Size = new Size(170, 60);
        lblMenuHeader.Text = "MHO Health System";
        lblMenuHeader.TextAlign = ContentAlignment.MiddleLeft;

        btnToggle.FlatAppearance.BorderSize = 0;
        btnToggle.FlatStyle = FlatStyle.Flat;
        btnToggle.BackColor = Color.FromArgb(4, 120, 87);
        btnToggle.ForeColor = Color.White;
        btnToggle.Size = new Size(38, 38);
        btnToggle.Location = new Point(228, 11);
        btnToggle.Cursor = Cursors.Hand;

        headerPanel.Controls.Add(lblMenuHeader);
        headerPanel.Controls.Add(btnToggle);

        // Profile Panel
        profilePanel.BackColor = Color.FromArgb(4, 108, 78);
        profilePanel.Dock = DockStyle.Top;
        profilePanel.Height = 84;
        profilePanel.Margin = new Padding(0, 10, 0, 10);

        picAvatar.Location = new Point(12, 16);
        picAvatar.Size = new Size(52, 52);
        picAvatar.SizeMode = PictureBoxSizeMode.Zoom;
        picAvatar.TabStop = false;

        lblName.AutoSize = false;
        lblName.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
        lblName.ForeColor = Color.White;
        lblName.Location = new Point(72, 18);
        lblName.Size = new Size(140, 22);
        lblName.Text = "Dr. Maria Santos";
        lblName.TextAlign = ContentAlignment.MiddleLeft;

        lblRole.AutoSize = false;
        lblRole.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular);
        lblRole.ForeColor = Color.FromArgb(167, 243, 208);
        lblRole.Location = new Point(72, 40);
        lblRole.Size = new Size(140, 18);
        lblRole.Text = "Health Officer / Admin";
        lblRole.TextAlign = ContentAlignment.MiddleLeft;

        profilePanel.Controls.Add(picAvatar);
        profilePanel.Controls.Add(lblName);
        profilePanel.Controls.Add(lblRole);

        // Workspace Menu Title
        lblWorkspace.Dock = DockStyle.Top;
        lblWorkspace.Height = 28;
        lblWorkspace.Padding = new Padding(4, 10, 0, 0);
        lblWorkspace.Font = new Font("Segoe UI Bold", 7.5F, FontStyle.Bold);
        lblWorkspace.ForeColor = Color.FromArgb(167, 243, 208);
        lblWorkspace.Text = "WORKSPACE";

        // Workspace Menu Items
        workspaceMenu.Dock = DockStyle.Top;
        workspaceMenu.AutoSize = true;
        workspaceMenu.FlowDirection = FlowDirection.TopDown;
        workspaceMenu.WrapContents = false;
        workspaceMenu.Padding = new Padding(0, 4, 0, 0);
        workspaceMenu.Controls.Add(btnDashboard);
        workspaceMenu.Controls.Add(btnMedicineCatalog);
        workspaceMenu.Controls.Add(btnStockMovement);
        workspaceMenu.Controls.Add(btnInventoryControl);
        workspaceMenu.Controls.Add(btnAnalyticsReports);
        workspaceMenu.Controls.Add(btnMyProfile);

        // Administration Menu Title
        lblAdministration.Dock = DockStyle.Top;
        lblAdministration.Height = 28;
        lblAdministration.Padding = new Padding(4, 10, 0, 0);
        lblAdministration.Font = new Font("Segoe UI Bold", 7.5F, FontStyle.Bold);
        lblAdministration.ForeColor = Color.FromArgb(167, 243, 208);
        lblAdministration.Text = "ADMINISTRATION";

        // Administration Menu Items
        administrationMenu.Dock = DockStyle.Top;
        administrationMenu.AutoSize = true;
        administrationMenu.FlowDirection = FlowDirection.TopDown;
        administrationMenu.WrapContents = false;
        administrationMenu.Padding = new Padding(0, 4, 0, 0);
        administrationMenu.Controls.Add(btnUserAdministration);
        administrationMenu.Controls.Add(btnActivityLog);
        administrationMenu.Controls.Add(btnSystemSetting);

        // Sign Out Button
        btnSignOut.Dock = DockStyle.Bottom;
        btnSignOut.Height = 42;
        btnSignOut.BackColor = Color.FromArgb(6, 78, 59);

        // Assemble Sidebar
        sidebar.Controls.Add(btnSignOut);
        sidebar.Controls.Add(administrationMenu);
        sidebar.Controls.Add(lblAdministration);
        sidebar.Controls.Add(workspaceMenu);
        sidebar.Controls.Add(lblWorkspace);
        sidebar.Controls.Add(profilePanel);
        sidebar.Controls.Add(headerPanel);

        // Main Area Layout
        mainArea.Dock = DockStyle.Fill;
        mainArea.BackColor = Color.FromArgb(243, 244, 246);

        // Top App Header Bar
        topAppBar.Dock = DockStyle.Top;
        topAppBar.Height = 70;
        topAppBar.BackColor = Color.White;
        topAppBar.Padding = new Padding(24, 12, 24, 12);

        lblPageTitle.AutoSize = true;
        lblPageTitle.Font = new Font("Segoe UI Bold", 14F, FontStyle.Bold);
        lblPageTitle.ForeColor = Color.FromArgb(17, 24, 39);
        lblPageTitle.Location = new Point(24, 10);
        lblPageTitle.Text = "Dashboard";

        lblSystemSubtitle.AutoSize = true;
        lblSystemSubtitle.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
        lblSystemSubtitle.ForeColor = Color.FromArgb(107, 114, 128);
        lblSystemSubtitle.Location = new Point(25, 36);
        lblSystemSubtitle.Text = "";
        lblSystemSubtitle.Visible = false;

        topAppBar.Controls.Add(lblPageTitle);
        topAppBar.Controls.Add(lblSystemSubtitle);

        // Content Area Container
        contentPanel.Dock = DockStyle.Fill;
        contentPanel.Padding = new Padding(24);

        // Card 1: Total Medicines
        SetupMetricCard(cardTotalMed, lblCard1Title, lblCard1Val, "TOTAL MEDICINE TYPES", "1,248", Color.FromArgb(16, 185, 129), 0);
        // Card 2: Low Stock Alert
        SetupMetricCard(cardLowStock, lblCard2Title, lblCard2Val, "LOW STOCK ALERTS", "14 Items", Color.FromArgb(245, 158, 11), 250);
        // Card 3: Expiring Soon
        SetupMetricCard(cardExpiring, lblCard3Title, lblCard3Val, "EXPIRING MEDICINE", "6 Batches", Color.FromArgb(239, 68, 68), 500);

        contentPanel.Controls.Add(cardExpiring);
        contentPanel.Controls.Add(cardLowStock);
        contentPanel.Controls.Add(cardTotalMed);

        mainArea.Controls.Add(contentPanel);
        mainArea.Controls.Add(topAppBar);

        // Animation Timer Config (15ms tick interval)
        sidebarTimer.Interval = 15;

        Controls.Add(mainArea);
        Controls.Add(sidebar);

        cardExpiring.ResumeLayout(false);
        cardExpiring.PerformLayout();
        cardLowStock.ResumeLayout(false);
        cardLowStock.PerformLayout();
        cardTotalMed.ResumeLayout(false);
        cardTotalMed.PerformLayout();
        contentPanel.ResumeLayout(false);
        topAppBar.ResumeLayout(false);
        topAppBar.PerformLayout();
        mainArea.ResumeLayout(false);
        administrationMenu.ResumeLayout(false);
        workspaceMenu.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)picAvatar).EndInit();
        profilePanel.ResumeLayout(false);
        sidebar.ResumeLayout(false);
        sidebar.PerformLayout();
        ResumeLayout(false);
    }

    private Button CreateNavButton(string text)
    {
        Button btn = new Button
        {
            BackColor = Color.FromArgb(6, 78, 59),
            FlatAppearance = { BorderSize = 0 },
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point),
            ForeColor = Color.FromArgb(209, 250, 229),
            Height = 40,
            ImageAlign = ContentAlignment.MiddleLeft,
            Margin = new Padding(0, 0, 0, 3),
            Padding = new Padding(14, 0, 8, 0),
            Size = new Size(252, 40),
            Text = text,
            TextAlign = ContentAlignment.MiddleLeft,
            TextImageRelation = TextImageRelation.ImageBeforeText,
            UseVisualStyleBackColor = false,
            Cursor = Cursors.Hand
        };
        return btn;
    }

    private void SetupMetricCard(Panel card, Label title, Label value, string titleText, string valText, Color accentColor, int leftMargin)
    {
        card.BackColor = Color.White;
        card.Size = new Size(230, 100);
        card.Location = new Point(leftMargin, 0);

        // Accent indicator bar on top of the card
        Panel accentBar = new Panel
        {
            BackColor = accentColor,
            Dock = DockStyle.Top,
            Height = 4
        };

        title.AutoSize = true;
        title.Font = new Font("Segoe UI Bold", 7.5F, FontStyle.Bold);
        title.ForeColor = Color.FromArgb(107, 114, 128);
        title.Location = new Point(16, 20);
        title.Text = titleText;

        value.AutoSize = true;
        value.Font = new Font("Segoe UI Bold", 18F, FontStyle.Bold);
        value.ForeColor = Color.FromArgb(17, 24, 39);
        value.Location = new Point(14, 42);
        value.Text = valText;

        card.Controls.Add(accentBar);
        card.Controls.Add(title);
        card.Controls.Add(value);
    }
}