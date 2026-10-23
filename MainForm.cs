using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
namespace InventoryMonitoringSystem
{
public partial class MainForm : Form
{
private Button? activeButton;
private bool sidebarCollapsed;
private const int ExpandedWidth = 280;
private const int CollapsedWidth = 72;
private UserSession currentUserSession = new("0", "guest", "Guest User", "User");
private MedicineCatalogControl? medicineCatalogView;
private StockMovementControl? stockMovementView;
private InventoryControlControl? inventoryControlView;
private Panel? dashboardWelcomePanel;
private Label? lblDashboardWelcome;
private Label? lblDashboardSummary;
private int dashboardOutOfStockCount;
private bool kioskMode = true;
public MainForm()
{
InitializeComponent();
KeyPreview = true;
// Keep the content area beside the navigation drawer instead of underneath it.
mainArea.Dock = DockStyle.None;
LayoutMainArea();
Resize += MainForm_Resize;
ConfigureDashboardWelcome();
LoadNavigationImages();
ConfigureNavigationEvents();
btnToggle.Click += BtnToggle_Click;
sidebarTimer.Tick += SidebarTimer_Tick;
picAvatar.Paint += PicAvatar_Paint;
Shown += MainForm_Shown;
EnterKioskMode();
}
public MainForm(UserSession session) : this()
{
ApplyUserSession(session);
}
private void EnterKioskMode()
{
kioskMode = true;
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
}
private void ExitKioskMode()
{
kioskMode = false;
SuspendLayout();
TopMost = false;
WindowState = FormWindowState.Normal;
FormBorderStyle = FormBorderStyle.Sizable;
MaximizeBox = true;
MinimizeBox = true;
Size = new Size(1200, 760);
Rectangle area = Screen.FromControl(this).WorkingArea;
Location = new Point(area.Left + Math.Max(0, (area.Width - Width) / 2), area.Top + Math.Max(0, (area.Height - Height) / 2));
ResumeLayout(true);
}
protected override void OnKeyDown(KeyEventArgs e)
{
if (e.KeyCode == Keys.Escape)
{ ExitKioskMode(); e.Handled = true; e.SuppressKeyPress = true; }
else if (e.KeyCode == Keys.F4)
{ EnterKioskMode(); e.Handled = true; e.SuppressKeyPress = true; }
base.OnKeyDown(e);
}
public void ApplyUserSession(UserSession session)
{
currentUserSession = session ?? new("0", "guest", "Guest User", "User");
lblName.Text = string.IsNullOrWhiteSpace(currentUserSession.FullName) ? currentUserSession.Username : currentUserSession.FullName;
UpdateDashboardWelcome();
lblRole.Text = currentUserSession.Role;
SetSidebarContentState(sidebarCollapsed);
activeButton = btnDashboard;
SetActive(btnDashboard);
}
private void ConfigureDashboardWelcome()
{
    dashboardWelcomePanel = new Panel
    {
        BackColor = Color.FromArgb(236, 253, 245),
        BorderStyle = BorderStyle.FixedSingle,
        Location = new Point(0, 0),
        Size = new Size(Math.Max(320, contentPanel.ClientSize.Width - contentPanel.Padding.Horizontal), 96),
        Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
    };
    lblDashboardWelcome = new Label
    {
        AutoSize = false, Location = new Point(18, 13), Size = new Size(800, 31),
        Font = new Font("Segoe UI", 16F, FontStyle.Bold), ForeColor = Color.FromArgb(6, 78, 59),
        TextAlign = ContentAlignment.MiddleLeft, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
    };
    lblDashboardSummary = new Label
    {
        AutoSize = false, Location = new Point(20, 49), Size = new Size(1000, 32),
        Font = new Font("Segoe UI", 9F), ForeColor = Color.FromArgb(55, 80, 68),
        TextAlign = ContentAlignment.MiddleLeft, AutoEllipsis = true, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
    };
    dashboardWelcomePanel.Controls.Add(lblDashboardWelcome);
    dashboardWelcomePanel.Controls.Add(lblDashboardSummary);
    contentPanel.Controls.Add(dashboardWelcomePanel);
    int initialCardY = dashboardWelcomePanel.Bottom + 18;
    cardTotalMed.Location = new Point(0, initialCardY);
    cardLowStock.Location = new Point(250, initialCardY);
    cardExpiring.Location = new Point(500, initialCardY);
    contentPanel.Resize += (_, _) =>
    {
        if (dashboardWelcomePanel == null) return;
        dashboardWelcomePanel.Width = Math.Max(320, contentPanel.ClientSize.Width - contentPanel.Padding.Horizontal);
        int cardY = dashboardWelcomePanel.Bottom + 18;
        cardTotalMed.Location = new Point(0, cardY);
        cardLowStock.Location = new Point(250, cardY);
        cardExpiring.Location = new Point(500, cardY);
    };
    UpdateDashboardWelcome();
}

private void UpdateDashboardWelcome()
{
    if (lblDashboardWelcome == null || lblDashboardSummary == null) return;
    int hour = DateTime.Now.Hour;
    string greeting = hour < 12 ? "Good morning" : hour < 18 ? "Good afternoon" : "Good evening";
    string name = string.IsNullOrWhiteSpace(currentUserSession.FullName) ? currentUserSession.Username : currentUserSession.FullName;
    lblDashboardWelcome.Text = $"{greeting}, {name}";
    int days = InventoryMonitoringSystem.StockMovementControl.CurrentExpiryWindowDays;
    lblDashboardSummary.Text = $"Inventory overview: {lblCard1Val.Text} medicine types, {lblCard2Val.Text} needing low-stock attention, {dashboardOutOfStockCount:N0} out of stock, and {lblCard3Val.Text} expiring within {days} days. Check Stock Movement for batch-level details.";
}

private void RefreshExpiringMetric()
{
    int days = InventoryMonitoringSystem.StockMovementControl.CurrentExpiryWindowDays;
    try
    {
        int count = InventoryMonitoringSystem.Services.StockMovementService.GetExpiringBatches(days).Rows.Count;
        lblCard3Val.Text = $"{count:N0} Batches";
        lblCard3Title.Text = "EXPIRING MEDICINE";
    }
    catch
    {
        lblCard3Val.Text = "0 Batches";
        lblCard3Title.Text = "EXPIRING MEDICINE";
    }
    UpdateDashboardWelcome();
}

private void LoadNavigationImages()
{
    SetButtonImage(btnToggle, "menu.png");
    SetButtonImage(btnDashboard, "dashboard.png");
    SetButtonImage(btnMedicineCatalog, "catalog.png");
    SetButtonImage(btnStockMovement, "movement.png");
    SetButtonImage(btnInventoryControl, "inventory.png");
    SetButtonImage(btnAnalyticsReports, "analytics.png");
    SetButtonImage(btnMyProfile, "profile.png");
    SetButtonImage(btnUserAdministration, "users.png");
    SetButtonImage(btnActivityLog, "log.png");
    SetButtonImage(btnSystemSetting, "settings.png");
    SetButtonImage(btnSignOut, "signout.png");

    Bitmap avatar = LoadAssetImage("avatar.png", 52, 52);
    picAvatar.Image?.Dispose();
    picAvatar.Image = avatar;
}

private static void SetButtonImage(Button button, string fileName)
{
    Bitmap image = LoadAssetImage(fileName, 20, 20);
    button.Image?.Dispose();
    button.Image = image;
}

private static Bitmap LoadAssetImage(string fileName, int width, int height)
{
    try
    {
        Assembly assembly = typeof(MainForm).Assembly;
        string? assemblyName = assembly.GetName().Name;
        if (!string.IsNullOrWhiteSpace(assemblyName))
        {
            string resourceName = $"{assemblyName}.Assets.{fileName}";
            using Stream? stream = assembly.GetManifestResourceStream(resourceName);
            if (stream != null)
            {
                using Bitmap source = new(stream);
                return new Bitmap(source);
            }
        }

        string baseDirectory = AppContext.BaseDirectory;
        string[] possiblePaths =
        [
            Path.Combine(baseDirectory, "Assets", fileName),
            Path.Combine(Directory.GetCurrentDirectory(), "Assets", fileName),
            Path.GetFullPath(Path.Combine(baseDirectory, "..", "..", "..", "Assets", fileName))
        ];

        foreach (string filePath in possiblePaths)
        {
            if (!File.Exists(filePath))
                continue;

            using Bitmap source = new(filePath);
            return new Bitmap(source);
        }
    }
    catch (Exception)
    {
        // Missing/corrupt optional navigation artwork must never prevent the form from opening.
    }

    return CreatePlaceholderBitmap(width, height);
}

private static Bitmap CreatePlaceholderBitmap(int width, int height)
{
    Bitmap bmp = new(Math.Max(16, width), Math.Max(16, height));
    using Graphics graphics = Graphics.FromImage(bmp);
    graphics.Clear(Color.Transparent);
    using Pen pen = new(Color.FromArgb(167, 243, 208), Math.Max(1, Math.Min(width, height) / 10));
    graphics.DrawRectangle(pen, 2, 2, bmp.Width - 5, bmp.Height - 5);
    return bmp;
}

private void PicAvatar_Paint(object? sender, PaintEventArgs e)
{
    if (picAvatar.Image == null) return;

    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
    using GraphicsPath path = new();
    path.AddEllipse(0, 0, picAvatar.Width - 1, picAvatar.Height - 1);
    e.Graphics.SetClip(path);
    e.Graphics.DrawImage(picAvatar.Image, 0, 0, picAvatar.Width, picAvatar.Height);

    using Pen borderPen = new(Color.FromArgb(167, 243, 208), 2);
    e.Graphics.DrawEllipse(borderPen, 1, 1, picAvatar.Width - 3, picAvatar.Height - 3);
}
private IEnumerable<Button> NavigationButtons()
{
yield return btnDashboard;
yield return btnMedicineCatalog;
yield return btnStockMovement;
yield return btnInventoryControl;
yield return btnAnalyticsReports;
yield return btnMyProfile;
yield return btnUserAdministration;
yield return btnActivityLog;
yield return btnSystemSetting;
yield return btnSignOut;
}
private void ConfigureNavigationEvents()
{
foreach (Button button in NavigationButtons())
{
button.Click += Navigation_Click;
button.MouseEnter += Navigation_MouseEnter;
button.MouseLeave += Navigation_MouseLeave;
}
btnSignOut.Click -= Navigation_Click;
btnSignOut.Click += BtnSignOut_Click;
}
private void MainForm_Shown(object? sender, EventArgs e)
{
if (kioskMode) EnterKioskMode();
SetSidebarContentState(sidebarCollapsed);
RefreshDashboardMetrics();
}
private void BtnToggle_Click(object? sender, EventArgs e)
{
    // Reverse smoothly from the current width if the user clicks during animation.
    if (sidebarTimer.Enabled)
    {
        sidebarCollapsed = !sidebarCollapsed;
        sidebarAnimationTarget = sidebarCollapsed ? CollapsedWidth : ExpandedWidth;
        return;
    }

    sidebarCollapsed = !sidebarCollapsed;
    sidebarAnimationTarget = sidebarCollapsed ? CollapsedWidth : ExpandedWidth;

    // Keep the content at its last completed layout while the drawer animates.
    // Reflow it once at the end to avoid expensive per-frame child layout.
    SuspendLayout();
    sidebar.SuspendLayout();
    sidebar.Dock = DockStyle.None;
    mainArea.Dock = DockStyle.None;
    sidebar.SetBounds(0, 0, Math.Max(1, sidebar.Width), ClientSize.Height);
    mainArea.SendToBack();
    sidebar.BringToFront();
    sidebar.ResumeLayout(false);
    ResumeLayout(false);

    sidebarTimer.Start();
}

private int sidebarAnimationTarget = ExpandedWidth;

private void MainForm_Resize(object? sender, EventArgs e)
{
    if (sidebar != null && sidebar.Height != ClientSize.Height)
        sidebar.Height = ClientSize.Height;
    LayoutMainArea();
    UpdateToggleLocation();
}

private void LayoutMainArea()
{
    if (mainArea == null || sidebar == null) return;
    int left = Math.Clamp(sidebar.Width, 0, Math.Max(0, ClientSize.Width - 1));
    mainArea.Dock = DockStyle.None;
    mainArea.SetBounds(left, 0, Math.Max(1, ClientSize.Width - left), ClientSize.Height);
}

private void SidebarTimer_Tick(object? sender, EventArgs e)
{
    int current = sidebar.Width;
    int distance = sidebarAnimationTarget - current;

    // Ease-out motion: large steps at first, smaller steps near the target.
    // The drawer remains visible above the main area for every animation frame.
    int step = Math.Max(2, (int)Math.Ceiling(Math.Abs(distance) * 0.28));
    int next = Math.Abs(distance) <= step ? sidebarAnimationTarget : current + Math.Sign(distance) * step;
    sidebar.SetBounds(0, 0, next, ClientSize.Height, BoundsSpecified.X | BoundsSpecified.Y | BoundsSpecified.Width | BoundsSpecified.Height);
    // Keep the content stable during the animation; update only the hamburger position.
    UpdateToggleLocation();
    sidebar.BringToFront();

    if (next == sidebarAnimationTarget)
        FinishSidebarAnimation();
}

private void FinishSidebarAnimation()
{
    sidebarTimer.Stop();
    int target = sidebarCollapsed ? CollapsedWidth : ExpandedWidth;

    // Only now allow the main content to resize and the navigation to reflow.
    SuspendLayout();
    sidebar.SuspendLayout();
    mainArea.SuspendLayout();
    sidebar.SetBounds(0, 0, target, ClientSize.Height);
    sidebar.Dock = DockStyle.None;
    mainArea.Dock = DockStyle.None;
    LayoutMainArea();
    SetSidebarContentState(collapsed: sidebarCollapsed);
    mainArea.ResumeLayout(true);
    sidebar.ResumeLayout(true);
    ResumeLayout(true);

    // WinForms docking order can put a Fill panel above a Left panel.
    // Explicitly keep the navigation drawer in front after layout completes.
    mainArea.SendToBack();
    sidebar.BringToFront();
}
private void SetSidebarContentState(bool collapsed)
{
sidebar.SuspendLayout();
int buttonWidth = collapsed ? CollapsedWidth - 16 : ExpandedWidth - 28;
foreach (Button button in NavigationButtons())
{
button.Width = buttonWidth;
button.Text = collapsed ? string.Empty : GetButtonText(button);
button.Padding = collapsed ? Padding.Empty : new Padding(14, 0, 8, 0);
button.ImageAlign = collapsed ? ContentAlignment.MiddleCenter : ContentAlignment.MiddleLeft;
button.TextAlign = collapsed ? ContentAlignment.MiddleCenter : ContentAlignment.MiddleLeft;
button.TextImageRelation = collapsed ? TextImageRelation.Overlay : TextImageRelation.ImageBeforeText;
}
lblMenuHeader.Visible = !collapsed;
profilePanel.Visible = !collapsed;
lblWorkspace.Visible = !collapsed;
bool isAdmin = currentUserSession.IsAdministration;
lblAdministration.Visible = !collapsed && isAdmin;
administrationMenu.Visible = isAdmin && !collapsed;
sidebar.Padding = collapsed ? new Padding(8, 12, 8, 12) : new Padding(14, 14, 14, 14);
UpdateToggleLocation();
sidebar.ResumeLayout(true);
}

private void UpdateToggleLocation()
{
    if (btnToggle == null || headerPanel == null) return;

    int headerWidth = headerPanel.ClientSize.Width;
    int margin = 10;
    int x;
    if ((sidebarCollapsed && !sidebarTimer.Enabled) ||
        (sidebarTimer.Enabled && sidebar.Width <= CollapsedWidth + 28))
    {
        x = (headerWidth - btnToggle.Width) / 2;
    }
    else
    {
        x = headerWidth - btnToggle.Width - margin;
    }

    x = Math.Max(0, Math.Min(x, Math.Max(0, headerWidth - btnToggle.Width)));
    int y = Math.Max(0, (headerPanel.ClientSize.Height - btnToggle.Height) / 2);
    btnToggle.Location = new Point(x, y);
}

private string GetButtonText(Button button)
{
if (button == btnDashboard) return "Dashboard";
if (button == btnMedicineCatalog) return "Medicine Catalog";
if (button == btnStockMovement) return "Stock Movement";
if (button == btnInventoryControl) return "Inventory Control";
if (button == btnAnalyticsReports) return "Analytics Reports";
if (button == btnMyProfile) return "My Profile";
if (button == btnUserAdministration) return "User Administration";
if (button == btnActivityLog) return "Activity Log";
if (button == btnSystemSetting) return "System Setting";
return "Sign Out";
}
private void Navigation_Click(object? sender, EventArgs e)
{
if (sender is Button button)
{
SetActive(button);
lblPageTitle.Text = GetButtonText(button);
NavigateToView(button);
}
}
private void NavigateToView(Button button)
{
contentPanel.SuspendLayout();
contentPanel.Controls.Clear();
if (button == btnMedicineCatalog)
{
if (medicineCatalogView == null)
{
medicineCatalogView = new();
    medicineCatalogView.DataChanged += MedicineCatalogView_DataChanged;
}
else
{
    medicineCatalogView.RefreshCatalog();
}
contentPanel.Controls.Add(medicineCatalogView);
}
else if (button == btnStockMovement)
{
    if (stockMovementView == null)
    {
        stockMovementView = new StockMovementControl(currentUserSession);
        stockMovementView.DataChanged += StockMovementView_DataChanged;
    }
    else
    {
        stockMovementView.RefreshData();
    }
    contentPanel.Controls.Add(stockMovementView);
}
else if (button == btnInventoryControl)
{
    if (inventoryControlView == null)
    {
        inventoryControlView = new InventoryControlControl(currentUserSession);
        inventoryControlView.RequestStockMovement += (_, _) =>
        {
            SetActive(btnStockMovement);
            lblPageTitle.Text = GetButtonText(btnStockMovement);
            NavigateToView(btnStockMovement);
        };
    }
    inventoryControlView.RefreshAll();
    contentPanel.Controls.Add(inventoryControlView);
}
else if (button == btnDashboard)
{
RefreshDashboardMetrics();
contentPanel.Controls.Add(dashboardWelcomePanel!);
contentPanel.Controls.Add(cardExpiring);
contentPanel.Controls.Add(cardLowStock);
contentPanel.Controls.Add(cardTotalMed);
int cardY = (dashboardWelcomePanel?.Bottom ?? 96) + 18;
cardTotalMed.Location = new Point(0, cardY);
cardLowStock.Location = new Point(250, cardY);
cardExpiring.Location = new Point(500, cardY);
}
contentPanel.ResumeLayout(true);
}
private void MedicineCatalogView_DataChanged(object? sender, EventArgs e)
{
    RefreshDashboardMetrics();
    if (inventoryControlView != null && !inventoryControlView.IsDisposed) inventoryControlView.RefreshAll();
}

private void StockMovementView_DataChanged(object? sender, EventArgs e)
{
    RefreshDashboardMetrics();
    if (inventoryControlView != null && !inventoryControlView.IsDisposed) inventoryControlView.RefreshAll();
}

private void RefreshDashboardMetrics()
{
    try
    {
        var metrics = InventoryMonitoringSystem.Services.MedicineService.GetDashboardMetrics();
        lblCard1Val.Text = metrics.Total.ToString("N0");
        int lowStockOnly = InventoryMonitoringSystem.Services.StockMovementService.GetLowStockMedicines().Rows.Count;
        lblCard2Val.Text = $"{lowStockOnly:N0} Items";
        dashboardOutOfStockCount = InventoryMonitoringSystem.Services.StockMovementService.GetOutOfStockMedicines().Rows.Count;
        RefreshExpiringMetric();
        UpdateDashboardWelcome();
    }
    catch
    {
        // Database initialization already ran before MainForm is opened. If a metric
        // query is unavailable because an optional table is absent, keep safe values.
        lblCard1Val.Text = "0";
        lblCard2Val.Text = "0 Items";
        dashboardOutOfStockCount = 0;
        RefreshExpiringMetric();
        UpdateDashboardWelcome();
    }
}

private void SetActive(Button button)
{
if (activeButton != null)
{
activeButton.BackColor = Color.FromArgb(6, 78, 59);
activeButton.ForeColor = Color.FromArgb(209, 250, 229);
}
activeButton = button;
activeButton.BackColor = Color.FromArgb(5, 150, 105);
activeButton.ForeColor = Color.White;
}
private void Navigation_MouseEnter(object? sender, EventArgs e)
{
if (sender is Button button && button != activeButton)
{
button.BackColor = button == btnSignOut
? Color.FromArgb(153, 27, 27)
: Color.FromArgb(4, 120, 87);
}
}
private void Navigation_MouseLeave(object? sender, EventArgs e)
{
if (sender is Button button && button != activeButton)
{
button.BackColor = Color.FromArgb(6, 78, 59);
}
}
private void BtnSignOut_Click(object? sender, EventArgs e)
{
DialogResult result = MessageBox.Show(
"Are you sure you want to sign out of the Health Office Inventory System?",
"Confirm Sign Out",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question);
if (result == DialogResult.Yes)
{
DialogResult = DialogResult.OK;
Close();
}
}
}
}