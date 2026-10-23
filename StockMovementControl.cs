using System.Data;
using System.Drawing;
using System.Globalization;
using InventoryMonitoringSystem.Services;

namespace InventoryMonitoringSystem;

[System.ComponentModel.DesignerCategory("UserControl")]
public partial class StockMovementControl : UserControl
{
    private static readonly Color Green = Color.FromArgb(5, 150, 105);
    private static readonly Color DarkGreen = Color.FromArgb(6, 78, 59);
    private static readonly Color PaleGreen = Color.FromArgb(236, 253, 245);
    private static readonly Color BorderColor = Color.FromArgb(220, 232, 226);
    private static readonly Color TextColor = Color.FromArgb(31, 55, 46);
    private readonly UserSession session;
    private readonly ListBox gridLowStock = CreateAlertList();
    private readonly ListBox gridOutOfStock = CreateAlertList();
    private readonly ListBox gridExpiring = CreateAlertList();
    private readonly ListBox gridExpired = CreateAlertList();
    private readonly ComboBox cmbExpiryWindow = new();
    private readonly DataGridView grid = new();
    private readonly TextBox txtSearch = new();
    private readonly DateTimePicker dtStart = new();
    private readonly DateTimePicker dtEnd = new();
    private readonly ComboBox cmbType = new();
    private readonly Label lblPeriod = new();
    private readonly Label lblCount = new();
    private bool loading;
    private bool suppressFilterRefresh;
    private int customExpiryDays = 120;
    private int lastExpirySelection;
    public static int CurrentExpiryWindowDays { get; private set; } = 30;
    private DataTable? currentData;

    public event EventHandler? DataChanged;

    private bool IsDesignTime => DesignMode || System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime;

    public StockMovementControl() : this(new UserSession("0", "guest", "Guest User", "User")) { }

    public StockMovementControl(UserSession userSession)
    {
        session = userSession ?? new UserSession("0", "guest", "Guest User", "User");
        InitializeComponent();
        if (!IsDesignTime)
        {
            BuildInterface();
            try { StockMovementService.EnsureSchema(); RefreshData(); }
            catch (Exception ex) { ShowDatabaseError(ex); }
        }
    }

    public void RefreshData()
    {
        if (IsDesignTime || loading || grid.IsDisposed) return;
        loading = true;
        try
        {
            DateTime? start = dtStart.Value.Date;
            DateTime? end = dtEnd.Value.Date;
            if (start.Value > end.Value)
                throw new InvalidOperationException("Start date cannot be later than end date.");
            RefreshInventoryLists();
            lblPeriod.Text = $"Transactions: {start.Value:MMM-dd-yyyy} – {end.Value:MMM-dd-yyyy}";
            currentData = StockMovementService.GetMovements(txtSearch.Text, start, end, Convert.ToString(cmbType.SelectedItem) ?? "All Types");
            // Keep the display-only medicine summary inside the bound table so it
            // follows its transaction when the user sorts any column.
            if (!currentData.Columns.Contains("MedicineDetails"))
                currentData.Columns.Add("MedicineDetails", typeof(string));
            foreach (DataRow row in currentData.Rows)
            {
                string med = currentData.Columns.Contains("Med_ID") ? Convert.ToString(row["Med_ID"]) ?? "" : "";
                string generic = currentData.Columns.Contains("GenericName") ? Convert.ToString(row["GenericName"]) ?? "" : "";
                string brand = currentData.Columns.Contains("BrandName") ? Convert.ToString(row["BrandName"]) ?? "" : "";
                string dosage = currentData.Columns.Contains("Dosage") ? Convert.ToString(row["Dosage"]) ?? "" : "";
                row["MedicineDetails"] = med + " • " + generic + (brand.Length > 0 ? " • " + brand : "") + (dosage.Length > 0 ? " • " + dosage : "");
            }
            grid.DataSource = currentData;
            ConfigureGrid();
            lblCount.Text = $"{currentData.Rows.Count:N0} transaction(s)";
        }
        catch (Exception ex)
        {
            MessageBox.Show("Stock Movement could not be refreshed.\n\n" + ex.Message, "Stock Movement", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally { loading = false; }
    }

    private void BuildInterface()
    {
        SuspendLayout();
        Controls.Clear();
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, Padding = new Padding(14, 12, 14, 12), BackColor = BackColor, GrowStyle = TableLayoutPanelGrowStyle.FixedSize };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 66));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 166));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 106));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        Controls.Add(root);

        var header = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2, BackColor = Color.Transparent, Margin = new Padding(0) };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        header.RowStyles.Add(new RowStyle(SizeType.Absolute, 34)); header.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        var title = new Label { Text = "Stock Movement", Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = DarkGreen, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, AutoEllipsis = true };
        var subtitle = new Label { Text = "Monitor and record inventory stock transactions.", Font = new Font("Segoe UI", 9f), ForeColor = Color.FromArgb(100, 116, 108), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, AutoEllipsis = true };
        var headerButtons = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = false, FlowDirection = FlowDirection.LeftToRight, Anchor = AnchorStyles.Right, Margin = new Padding(0, 4, 0, 0) };
        var btnRefresh = MakeButton("↻ Refresh", false, 92); btnRefresh.Click += (_, _) => RefreshData();
        var btnAdd = MakeButton("＋ Add Movement", true, 155); btnAdd.Click += (_, _) => OpenAddDialog();
        headerButtons.Controls.Add(btnRefresh); headerButtons.Controls.Add(btnAdd);
        header.Controls.Add(title, 0, 0); header.Controls.Add(subtitle, 0, 1); header.Controls.Add(headerButtons, 1, 0); header.SetRowSpan(headerButtons, 2);
        root.Controls.Add(header, 0, 0);

        var filters = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 6, RowCount = 3, BackColor = Color.White, Padding = new Padding(10, 7, 10, 7), Margin = new Padding(0, 3, 0, 7) };
        filters.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28)); filters.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 17)); filters.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 17)); filters.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 18)); filters.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 84)); filters.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 84));
        filters.RowStyles.Add(new RowStyle(SizeType.Absolute, 17)); filters.RowStyles.Add(new RowStyle(SizeType.Absolute, 32)); filters.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        AddFilterLabel(filters, "Search transactions", 0); AddFilterLabel(filters, "Start date", 1); AddFilterLabel(filters, "End date", 2); AddFilterLabel(filters, "Transaction type", 3);
        StyleTextBox(txtSearch); txtSearch.PlaceholderText = "Search ID, batch, medicine..."; txtSearch.Dock = DockStyle.Fill; txtSearch.Margin = new Padding(0, 2, 6, 0);
        StyleDatePicker(dtStart); StyleDatePicker(dtEnd); dtStart.Value = DateTime.Today; dtEnd.Value = DateTime.Today;
        var startPanel = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0, 2, 6, 0) }; startPanel.Controls.Add(dtStart);
        var endPanel = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0, 2, 6, 0) }; endPanel.Controls.Add(dtEnd);
        cmbType.DropDownStyle = ComboBoxStyle.DropDownList; cmbType.Items.Add("All Types"); cmbType.Items.AddRange(StockMovementService.TransactionTypes.Cast<object>().ToArray()); cmbType.SelectedIndex = 0; StyleCombo(cmbType); cmbType.Dock = DockStyle.Fill;
        filters.Controls.Add(txtSearch, 0, 1); filters.Controls.Add(startPanel, 1, 1); filters.Controls.Add(endPanel, 2, 1); filters.Controls.Add(cmbType, 3, 1);
        var btnClear = MakeButton("Clear", false, 72); btnClear.Dock = DockStyle.Fill;
        btnClear.Click += (_, _) =>
        {
            suppressFilterRefresh = true;
            try { txtSearch.Clear(); dtStart.Value = DateTime.Today; dtEnd.Value = DateTime.Today; cmbType.SelectedIndex = 0; }
            finally { suppressFilterRefresh = false; }
            RefreshData();
        };
        filters.Controls.Add(btnClear, 4, 1); filters.SetColumnSpan(btnClear, 2);
        filters.Controls.Add(lblPeriod, 0, 2); filters.SetColumnSpan(lblPeriod, 6);
        lblPeriod.Dock = DockStyle.Fill; lblPeriod.ForeColor = Color.FromArgb(100, 116, 108); lblPeriod.Font = new Font("Segoe UI", 8f); lblPeriod.TextAlign = ContentAlignment.MiddleLeft; lblPeriod.AutoEllipsis = true;
        txtSearch.TextChanged += (_, _) => ApplyFiltersImmediately();
        dtStart.ValueChanged += (_, _) => ApplyFiltersImmediately();
        dtEnd.ValueChanged += (_, _) => ApplyFiltersImmediately();
        cmbType.SelectedIndexChanged += (_, _) => ApplyFiltersImmediately();
        root.Controls.Add(filters, 0, 2);

        var alerts = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 1, Margin = new Padding(0, 0, 0, 8), BackColor = Color.Transparent, Padding = new Padding(0) };
        for (int i = 0; i < 4; i++) alerts.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
        alerts.Controls.Add(MakeListPanel("LOW ON STOCK", gridLowStock, Color.FromArgb(161, 98, 7)), 0, 0);
        alerts.Controls.Add(MakeListPanel("OUT OF STOCK", gridOutOfStock, Color.FromArgb(185, 28, 28)), 1, 0);
        var expiringPanel = MakeListPanel("EXPIRING MEDICINE", gridExpiring, Color.FromArgb(161, 98, 7));
        var expiryHeader = expiringPanel.Controls.OfType<Panel>().FirstOrDefault(p => p.Name == "alertHeader");
        if (expiryHeader != null)
        {
            cmbExpiryWindow.DropDownStyle = ComboBoxStyle.DropDownList; cmbExpiryWindow.Items.AddRange(new object[] { "30 days", "60 days", "90 days", $"Custom ({customExpiryDays} days)" }); cmbExpiryWindow.SelectedIndex = 0; StyleCombo(cmbExpiryWindow); cmbExpiryWindow.Width = 112; cmbExpiryWindow.Height = 26; cmbExpiryWindow.Anchor = AnchorStyles.Right | AnchorStyles.Top; cmbExpiryWindow.Location = new Point(Math.Max(0, expiryHeader.ClientSize.Width - cmbExpiryWindow.Width - 42), 6);
            expiryHeader.Controls.Add(cmbExpiryWindow);
            expiryHeader.Resize += (_, _) => cmbExpiryWindow.Location = new Point(Math.Max(0, expiryHeader.ClientSize.Width - cmbExpiryWindow.Width - 42), 6);
            cmbExpiryWindow.SelectedIndexChanged += (_, _) => HandleExpiryWindowChanged();
        }
        alerts.Controls.Add(expiringPanel, 2, 0);
        alerts.Controls.Add(MakeListPanel("EXPIRED STOCK", gridExpired, Color.FromArgb(185, 28, 28)), 3, 0);
        root.Controls.Add(alerts, 0, 1);

        var tablePanel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = Color.White, Margin = new Padding(0), Padding = new Padding(0) };
        tablePanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34)); tablePanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        var tableHeader = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(10, 0, 10, 0) };
        var tableTitle = new Label { Text = "Transaction History", Font = new Font("Segoe UI", 10, FontStyle.Bold), ForeColor = DarkGreen, Dock = DockStyle.Left, AutoSize = true, TextAlign = ContentAlignment.MiddleLeft };
        lblCount.Dock = DockStyle.Right; lblCount.Font = new Font("Segoe UI", 8f); lblCount.ForeColor = Color.Gray; lblCount.TextAlign = ContentAlignment.MiddleRight;
        tableHeader.Controls.Add(lblCount); tableHeader.Controls.Add(tableTitle);
        ConfigureGridBase(); grid.Dock = DockStyle.Fill; grid.Margin = new Padding(0);
        grid.CellDoubleClick += (_, e) => { if (e.RowIndex >= 0) ViewSelectedDetails(); };
        grid.SizeChanged += (_, _) => { if (!grid.IsDisposed && grid.Columns.Count > 0) { grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; grid.PerformLayout(); } };
        var context = new ContextMenuStrip(); context.Items.Add("View Details", null, (_, _) => ViewSelectedDetails()); grid.ContextMenuStrip = context;
        tablePanel.Controls.Add(tableHeader, 0, 0); tablePanel.Controls.Add(grid, 0, 1); root.Controls.Add(tablePanel, 0, 3);
        ResumeLayout(true);
    }

    private static ListBox CreateAlertList() => new ListBox
    {
        DrawMode = DrawMode.OwnerDrawFixed,
        IntegralHeight = false,
        ItemHeight = 34,
        BorderStyle = BorderStyle.None,
        BackColor = Color.White,
        ForeColor = TextColor,
        Font = new Font("Segoe UI", 8.25f),
        HorizontalScrollbar = false
    };

    private Control MakeListPanel(string heading, ListBox dataList, Color accent)
    {
        bool isRedAlert = heading == "OUT OF STOCK" || heading == "EXPIRED STOCK";
        Color tint = isRedAlert ? Color.FromArgb(254, 242, 242) : Color.FromArgb(255, 251, 235);
        Color accentSoft = isRedAlert ? Color.FromArgb(254, 226, 226) : Color.FromArgb(254, 243, 199);
        var section = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Margin = new Padding(0, 0, 9, 0),
            Padding = new Padding(0),
        };
        section.Paint += (_, e) =>
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using var border = new Pen(BorderColor);
            e.Graphics.DrawRectangle(border, 0, 0, Math.Max(0, section.Width - 1), Math.Max(0, section.Height - 1));
            using var divider = new Pen(Color.FromArgb(235, 241, 237));
            e.Graphics.DrawLine(divider, 1, 42, Math.Max(1, section.Width - 2), 42);
        };

        var header = new Panel
        {
            Name = "alertHeader",
            Dock = DockStyle.Top,
            Height = 43,
            BackColor = Color.White,
            Padding = new Padding(12, 0, 10, 0)
        };
        var title = new Label
        {
            Text = heading,
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
            ForeColor = Color.FromArgb(42, 59, 50),
            TextAlign = ContentAlignment.MiddleLeft,
            AutoEllipsis = true,
            Padding = new Padding(0),
            Margin = new Padding(0)
        };
        var count = new Label
        {
            Text = "0",
            Dock = DockStyle.Right,
            Width = 34,
            BackColor = accentSoft,
            ForeColor = accent,
            Font = new Font("Segoe UI", 8.25f, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter,
            Margin = new Padding(0, 8, 0, 8)
        };
        header.Controls.Add(title);
        header.Controls.Add(count);

        var listHost = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(7, 4, 7, 5),
            Margin = new Padding(0)
        };
        dataList.Dock = DockStyle.Fill;
        dataList.Margin = new Padding(0);
        dataList.Tag = count;
        dataList.DrawItem += AlertList_DrawItem;
        dataList.BorderStyle = BorderStyle.None;
        dataList.BackColor = Color.White;
        dataList.ForeColor = TextColor;
        dataList.Font = new Font("Segoe UI", 8.25f);
        dataList.IntegralHeight = false;
        dataList.HorizontalScrollbar = false;
        dataList.ItemHeight = 38;
        dataList.SelectionMode = SelectionMode.One;
        listHost.Controls.Add(dataList);
        section.Controls.Add(listHost);
        section.Controls.Add(header);
        return section;
    }

    private void ApplyFiltersImmediately()
    {
        if (!suppressFilterRefresh && !loading)
            RefreshData();
    }

    private static void ConfigureSmallGrid(DataGridView dataGrid)
    {
        dataGrid.AutoGenerateColumns = true; dataGrid.AllowUserToAddRows = false; dataGrid.AllowUserToDeleteRows = false; dataGrid.ReadOnly = true;
        dataGrid.MultiSelect = false; dataGrid.RowHeadersVisible = false; dataGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dataGrid.BackgroundColor = Color.White; dataGrid.BorderStyle = BorderStyle.None; dataGrid.GridColor = Color.FromArgb(232, 239, 235);
        dataGrid.EnableHeadersVisualStyles = false; dataGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(240, 249, 244);
        dataGrid.ColumnHeadersDefaultCellStyle.ForeColor = DarkGreen; dataGrid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 7.5f, FontStyle.Bold);
        dataGrid.ColumnHeadersHeight = 32; dataGrid.RowTemplate.Height = 29; dataGrid.DefaultCellStyle.Font = new Font("Segoe UI", 8.5f); dataGrid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal; dataGrid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None; dataGrid.ColumnHeadersDefaultCellStyle.Padding = new Padding(6, 0, 4, 0); dataGrid.DefaultCellStyle.Padding = new Padding(6, 0, 4, 0); dataGrid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(249, 252, 250);
        dataGrid.DefaultCellStyle.ForeColor = TextColor; dataGrid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(209, 250, 229); dataGrid.DefaultCellStyle.SelectionForeColor = DarkGreen;
        dataGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; dataGrid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
        dataGrid.ScrollBars = ScrollBars.Vertical;
    }

    private int GetExpiryWindowDays() => cmbExpiryWindow.SelectedIndex switch
    {
        1 => 60,
        2 => 90,
        3 => customExpiryDays,
        _ => 30
    };

    private void HandleExpiryWindowChanged()
    {
        if (cmbExpiryWindow.SelectedIndex == 3)
        {
            int? selectedDays = ExpiryDaysPrompt.Show(FindForm(), customExpiryDays);
            if (selectedDays.HasValue)
            {
                customExpiryDays = selectedDays.Value;
                cmbExpiryWindow.Items[3] = $"Custom ({customExpiryDays} days)";
                lastExpirySelection = 3;
                RefreshInventoryLists();
                DataChanged?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                cmbExpiryWindow.SelectedIndex = lastExpirySelection == 3 ? 0 : lastExpirySelection;
            }
            return;
        }

        lastExpirySelection = Math.Max(0, cmbExpiryWindow.SelectedIndex);
        CurrentExpiryWindowDays = GetExpiryWindowDays();
        RefreshInventoryLists();
        DataChanged?.Invoke(this, EventArgs.Empty);
    }

    private static void BindAlertList(ListBox list, DataTable table, string mode)
    {
        list.BeginUpdate();
        try
        {
            list.Items.Clear();
            foreach (DataRow row in table.Rows)
            {
                string id = Convert.ToString(row["Med ID"]) ?? "";
                string medicine = Convert.ToString(row["Medicine"]) ?? "Medicine";
                string brand = table.Columns.Contains("Brand") ? Convert.ToString(row["Brand"]) ?? "" : "";
                string dosage = table.Columns.Contains("Dosage") ? Convert.ToString(row["Dosage"]) ?? "" : "";
                string stock = table.Columns.Contains("Current Stock") ? Convert.ToDecimal(row["Current Stock"]).ToString("N0") : "0";
                string unit = table.Columns.Contains("Unit") ? Convert.ToString(row["Unit"]) ?? "" : "";
                string batch = table.Columns.Contains("Batch No.") ? Convert.ToString(row["Batch No."]) ?? "" : "";
                string expiry = table.Columns.Contains("Expiry Date") && row["Expiry Date"] != DBNull.Value ? Convert.ToDateTime(row["Expiry Date"]).ToString("MMM-dd-yyyy") : "Not recorded";
                string details = medicine + (brand.Length > 0 ? " · " + brand : "") + (dosage.Length > 0 ? " · " + dosage : "");
                string subtitle = mode == "expiry"
                    ? $"{(batch.Length > 0 ? batch + " · " : "")}Expiration: {expiry}".Trim()
                    : $"Stock: {stock} {unit}".Trim();
                list.Items.Add(new AlertListItem(id + "  " + details, subtitle));
            }
        }
        finally { list.EndUpdate(); }
        if (list.Tag is Label count)
        {
            count.Text = list.Items.Count.ToString("N0");
            bool redAlert = list.BackColor == Color.FromArgb(254, 242, 242);
            count.BackColor = redAlert ? Color.FromArgb(254, 226, 226) : Color.FromArgb(254, 243, 199);
        }
        list.Invalidate();
    }

    private static void AlertList_DrawItem(object? sender, DrawItemEventArgs e)
    {
        if (sender is not ListBox list || e.Index < 0 || e.Index >= list.Items.Count) return;
        var item = list.Items[e.Index] as AlertListItem;
        if (item == null) return;

        bool isRedAlert = list.Tag is Label count && count.ForeColor == Color.FromArgb(185, 28, 28);
        Color titleColor = Color.FromArgb(42, 59, 50);
        Color detailColor = Color.FromArgb(104, 119, 111);
        bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
        using var background = new SolidBrush(selected ? Color.FromArgb(236, 253, 245) : Color.White);
        e.Graphics.FillRectangle(background, e.Bounds);

        int dotSize = 7;
        using var dotBrush = new SolidBrush(isRedAlert ? Color.FromArgb(220, 38, 38) : Color.FromArgb(217, 146, 23));
        e.Graphics.FillEllipse(dotBrush, e.Bounds.X + 4, e.Bounds.Y + 9, dotSize, dotSize);
        using var titleFont = new Font("Segoe UI", 8.15f, FontStyle.Bold);
        using var detailFont = new Font("Segoe UI", 7.25f, FontStyle.Regular);
        var titleBounds = new Rectangle(e.Bounds.X + 17, e.Bounds.Y + 3, Math.Max(0, e.Bounds.Width - 21), 16);
        var detailBounds = new Rectangle(e.Bounds.X + 17, e.Bounds.Y + 19, Math.Max(0, e.Bounds.Width - 21), 15);
        TextRenderer.DrawText(e.Graphics, item.Title, titleFont, titleBounds, titleColor, TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine | TextFormatFlags.VerticalCenter);
        TextRenderer.DrawText(e.Graphics, item.Detail, detailFont, detailBounds, detailColor, TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine | TextFormatFlags.VerticalCenter);
        using var line = new Pen(Color.FromArgb(239, 244, 241));
        e.Graphics.DrawLine(line, e.Bounds.X + 4, e.Bounds.Bottom - 1, e.Bounds.Right - 4, e.Bounds.Bottom - 1);
        if ((e.State & DrawItemState.Focus) == DrawItemState.Focus) e.DrawFocusRectangle();
    }

    private sealed record AlertListItem(string Title, string Detail)
    {
        public override string ToString() => Title + " — " + Detail;
    }

    private void RefreshInventoryLists()
    {
        try
        {
            BindAlertList(gridLowStock, StockMovementService.GetLowStockMedicines(), "stock");
            BindAlertList(gridOutOfStock, StockMovementService.GetOutOfStockMedicines(), "stock");
            int days = GetExpiryWindowDays();
            CurrentExpiryWindowDays = days;
            BindAlertList(gridExpiring, StockMovementService.GetExpiringBatches(days), "expiry");
            BindAlertList(gridExpired, StockMovementService.GetExpiredBatches(), "expiry");
        }
        catch (Exception ex)
        {
            // Keep transaction history usable even if an alert query fails.
            System.Diagnostics.Debug.WriteLine("Inventory alerts could not refresh: " + ex.Message);
        }
    }

    private static Button MakeButton(string text, bool primary, int width)
    {
        var button = new Button { Text = text, Width = width, Height = 34, Margin = new Padding(4, 0, 0, 0), FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9f, FontStyle.Bold), Cursor = Cursors.Hand, UseVisualStyleBackColor = false, BackColor = primary ? Green : Color.White, ForeColor = primary ? Color.White : DarkGreen, Padding = new Padding(5, 0, 5, 0) };
        button.FlatAppearance.BorderSize = primary ? 0 : 1; button.FlatAppearance.BorderColor = BorderColor; button.FlatAppearance.MouseOverBackColor = primary ? Color.FromArgb(4, 120, 87) : PaleGreen;
        return button;
    }
    private static void StyleTextBox(TextBox box) { box.Font = new Font("Segoe UI", 9f); box.BorderStyle = BorderStyle.FixedSingle; box.BackColor = Color.White; box.ForeColor = TextColor; }
    private static void StyleDatePicker(DateTimePicker picker) { picker.Format = DateTimePickerFormat.Custom; picker.CustomFormat = "MMM-dd-yyyy"; picker.Font = new Font("Segoe UI", 8.5f); picker.Dock = DockStyle.Fill; picker.Margin = new Padding(0); }
    private static void StyleCombo(ComboBox combo) { combo.Font = new Font("Segoe UI", 8.5f); combo.DropDownStyle = ComboBoxStyle.DropDownList; combo.FlatStyle = FlatStyle.Flat; }
    private static void AddFilterLabel(TableLayoutPanel panel, string text, int column) => panel.Controls.Add(new Label { Text = text, Font = new Font("Segoe UI", 8f, FontStyle.Bold), ForeColor = Color.FromArgb(100, 116, 108), Dock = DockStyle.Fill, TextAlign = ContentAlignment.BottomLeft, Margin = new Padding(0, 0, 0, 2) }, column, 0);

    private void ConfigureGridBase()
    {
        grid.AutoGenerateColumns = true; grid.AllowUserToAddRows = false; grid.AllowUserToDeleteRows = false; grid.ReadOnly = true;
        grid.MultiSelect = false; grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect; grid.RowHeadersVisible = false;
        grid.BackgroundColor = Color.White; grid.BorderStyle = BorderStyle.None; grid.GridColor = Color.FromArgb(232, 239, 235);
        grid.EnableHeadersVisualStyles = false; grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(240, 249, 244);
        grid.ColumnHeadersDefaultCellStyle.ForeColor = DarkGreen; grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
        grid.ColumnHeadersHeight = 40; grid.RowTemplate.Height = 39; grid.DefaultCellStyle.Font = new Font("Segoe UI", 8.5f);
        grid.DefaultCellStyle.ForeColor = TextColor; grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(209, 250, 229); grid.DefaultCellStyle.SelectionForeColor = DarkGreen;
        grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 253, 251); grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None; grid.ScrollBars = ScrollBars.Vertical;
    }

    private void ConfigureGrid()
    {
        if (grid.Columns.Count == 0) return;
        void Col(string name, string header, int width)
        {
            // Only configure columns that are currently attached to this grid.
            // Avoid setting Width/MinimumWidth here: with Fill sizing, setting the
            // band thickness during DataSource binding can throw in some layouts.
            if (!grid.Columns.Contains(name)) return;
            DataGridViewColumn? column = grid.Columns[name];
            if (column == null || !ReferenceEquals(column.DataGridView, grid)) return;

            column.HeaderText = header;
            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            column.FillWeight = Math.Max(1, width);
            column.SortMode = DataGridViewColumnSortMode.Automatic;
        }
        Col("TransactionDate", "Date & Time", 130); Col("TransactionID", "Transaction ID", 95); Col("BatchNo", "Batch No.", 80);
        if (grid.Columns["MedicineDetails"] is DataGridViewColumn medicineDetailsBound)
        {
            medicineDetailsBound.HeaderText = "Medicine Details";
            medicineDetailsBound.ReadOnly = true;
            medicineDetailsBound.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            medicineDetailsBound.FillWeight = 360;
            medicineDetailsBound.SortMode = DataGridViewColumnSortMode.Automatic;
        }
        Col("Quantity", "Quantity", 85); Col("Unit", "Unit of Measure", 105); Col("TransactionType", "Transaction Type", 150); Col("UnitCost", "Cost/Unit", 90); Col("TotalCost", "Total Cost", 100);
        foreach (string hidden in new[] { "MovementId", "Med_ID", "GenericName", "BrandName", "Dosage", "CreatedBy", "ExpiryDate", "BatchBalance" })
        {
            if (grid.Columns[hidden] is DataGridViewColumn column)
                column.Visible = false;
        }
        SetColumnFormat("TransactionDate", "MMM-dd-yy h:mm tt");
        SetColumnFormat("Quantity", "N0");
        SetColumnFormat("UnitCost", "₱#,##0.00");
        SetColumnFormat("TotalCost", "₱#,##0.00");
        if (grid.Columns["TransactionType"] is DataGridViewColumn typeColumn)
        {
            typeColumn.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
            typeColumn.HeaderCell.Style.WrapMode = DataGridViewTriState.False;
            typeColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            typeColumn.FillWeight = 150;
            foreach (DataGridViewRow row in grid.Rows)
            {
                string type = Convert.ToString(row.Cells["TransactionType"].Value) ?? "";
                row.Cells["TransactionType"].Style.ForeColor = type == StockMovementService.StockIn ? Color.FromArgb(4, 120, 87) : type == StockMovementService.StockOut ? Color.FromArgb(71, 85, 105) : Color.FromArgb(180, 83, 9);
                row.Cells["TransactionType"].Style.Font = new Font("Segoe UI", 8.25f, FontStyle.Bold);
            }
        }
        if (grid.Columns["Remarks"] is DataGridViewColumn remarksColumn) remarksColumn.Visible = false;
        string[] visibleOrder = ["TransactionDate", "TransactionID", "BatchNo", "MedicineDetails", "Quantity", "Unit", "TransactionType", "UnitCost", "TotalCost"];
        for (int i = 0; i < visibleOrder.Length; i++)
            if (grid.Columns[visibleOrder[i]] is DataGridViewColumn visibleColumn) visibleColumn.DisplayIndex = i;
        // Keep all visible columns adaptive when the navigation drawer expands or collapses.
        if (grid.Columns["MedicineDetails"] is DataGridViewColumn medicineDetailsColumn)
        {
            medicineDetailsColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            medicineDetailsColumn.FillWeight = 360;
        }
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
        grid.AllowUserToResizeRows = false;
        grid.RowTemplate.Height = 39;
        foreach (DataGridViewRow row in grid.Rows)
            row.Height = 39;
        grid.PerformLayout();
    }

    private void OpenAddDialog()
    {
        using var dialog = new StockMovementEntryForm(session);
        // In kiosk mode the owner is TopMost. Keep the owned modal dialog in the
        // same z-order band so it cannot open behind the fullscreen MainForm.
        Form? owner = FindForm();
        if (owner != null)
        {
            dialog.TopMost = owner.TopMost;
            dialog.StartPosition = FormStartPosition.CenterParent;
            dialog.Shown += (_, _) =>
            {
                dialog.BringToFront();
                dialog.Activate();
            };
        }

        DialogResult result = owner != null ? dialog.ShowDialog(owner) : dialog.ShowDialog();
        if (result == DialogResult.OK)
        {
            RefreshData(); DataChanged?.Invoke(this, EventArgs.Empty);
            MessageBox.Show($"Transaction {dialog.SavedTransactionId} was saved successfully.\nUpdated batch balance: {dialog.SavedBalance:N0} {dialog.SavedUnit}", "Transaction Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private void SetColumnFormat(string columnName, string format)
    {
        if (grid.Columns[columnName] is DataGridViewColumn column)
            column.DefaultCellStyle.Format = format;
    }

    private void ViewSelectedDetails()
    {
        if (grid.CurrentRow == null) return;
        string Get(string column) => grid.Columns.Contains(column) ? Convert.ToString(grid.CurrentRow.Cells[column].Value) ?? "" : "";
        string transactionDateText = DateTime.TryParse(Get("TransactionDate"), out DateTime date)
            ? date.ToString("MMM-dd-yy h:mm tt")
            : Get("TransactionDate");
        decimal medicineStock = StockMovementService.GetMedicineStock(Get("Med_ID"));
        string expiryText = DateTime.TryParse(Get("ExpiryDate"), out DateTime expiry) ? expiry.ToString("MMM-dd-yyyy") : Get("ExpiryDate");
        string details = $"Transaction ID: {Get("TransactionID")}\nDate & Time: {transactionDateText}\nBatch No.: {Get("BatchNo")}\nMedicine: {Get("Med_ID")} — {Get("GenericName")} {Get("BrandName")} {Get("Dosage")}\nTransaction Type: {Get("TransactionType")}\nQuantity: {Get("Quantity")} {Get("Unit")}\nCost/Unit: ₱{Convert.ToDecimal(Get("UnitCost")):N2}\nTotal Cost: ₱{Convert.ToDecimal(Get("TotalCost")):N2}\nBatch Expiry: {expiryText}\nBatch Balance (current): {Convert.ToDecimal(Get("BatchBalance")):N0}\nMedicine Stock: {medicineStock:N0} {Get("Unit")}\nRemarks: {Get("Remarks")}\nRecorded By: {Get("CreatedBy")}";
        MessageBox.Show(details, "Stock Movement Details", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void ShowDatabaseError(Exception ex) => MessageBox.Show("Stock Movement could not connect to the inventory database. Verify that SQL Server LocalDB is installed and the database is available.\n\n" + ex.Message, "Stock Movement", MessageBoxButtons.OK, MessageBoxIcon.Error);
}

internal sealed class StockMovementEntryForm : Form
{
    private static readonly Color Green = Color.FromArgb(5, 150, 105);
    private static readonly Color DarkGreen = Color.FromArgb(6, 78, 59);
    private static readonly Color Muted = Color.FromArgb(100, 116, 139);
    private readonly UserSession session;
    private readonly ComboBox cmbMedicine = new();
    private readonly ComboBox cmbBatch = new();
    private readonly ComboBox cmbType = new();
    private readonly DateTimePicker dtExpiry = new();
    private readonly NumericUpDown numQuantity = new();
    private readonly NumericUpDown numCost = new();
    private readonly TextBox txtUnit = new();
    private readonly TextBox txtTotal = new();
    private readonly TextBox txtRemarks = new();
    private readonly Label lblMedicineDetails = new();
    private readonly Label lblBatchHelp = new();
    private readonly List<StockMovementService.MedicineOption> allMedicines = [];
    private readonly List<StockMovementService.MedicineOption> eligibleMedicines = [];
    private bool changing;
    private bool updatingAmounts;
    private bool totalWasEdited;
    public string SavedTransactionId { get; private set; } = "";
    public decimal SavedBalance { get; private set; }
    public string SavedUnit { get; private set; } = "";

    private sealed class NewBatchChoice(string batchNo)
    {
        public string BatchNo { get; } = batchNo;
        public override string ToString() => $"＋ New batch · {BatchNo}";
    }

    public StockMovementEntryForm(UserSession userSession)
    {
        session = userSession;
        Text = "Add Stock Movement";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false; MinimizeBox = false; ShowInTaskbar = false;
        ClientSize = new Size(790, 690);
        BackColor = Color.FromArgb(246, 250, 248);
        Font = new Font("Segoe UI", 9f);
        BuildForm();
        LoadMedicines();
    }

    private void BuildForm()
    {
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(24, 18, 24, 18), ColumnCount = 2, RowCount = 12, BackColor = BackColor };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 164));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        Controls.Add(root);

        var titlePanel = new Panel { Dock = DockStyle.Fill, BackColor = BackColor, Padding = new Padding(0, 0, 0, 4) };
        var title = new Label { Text = "Record Stock Movement", Font = new Font("Segoe UI", 17, FontStyle.Bold), ForeColor = DarkGreen, Dock = DockStyle.Top, Height = 30 };
        var subtitle = new Label { Text = "Enter the movement details below. Date and time are recorded automatically on save.", Font = new Font("Segoe UI", 8.5f), ForeColor = Muted, Dock = DockStyle.Bottom, Height = 20 };
        titlePanel.Controls.Add(title); titlePanel.Controls.Add(subtitle);
        root.Controls.Add(titlePanel, 0, 0); root.SetColumnSpan(titlePanel, 2);

        cmbMedicine.DropDownStyle = ComboBoxStyle.DropDown;
        cmbMedicine.AutoCompleteMode = AutoCompleteMode.None;
        cmbMedicine.Dock = DockStyle.Fill;
        cmbMedicine.IntegralHeight = false;
        cmbMedicine.MaxDropDownItems = 10;
        cmbMedicine.TextUpdate += MedicineTextUpdated;
        cmbMedicine.SelectedIndexChanged += (_, _) => MedicineChanged();
        AddRow(root, 1, "Medicine ID", cmbMedicine);

        lblMedicineDetails.Text = "Choose a medicine to view its catalog details.";
        lblMedicineDetails.Dock = DockStyle.Fill;
        lblMedicineDetails.TextAlign = ContentAlignment.MiddleLeft;
        lblMedicineDetails.ForeColor = Muted;
        lblMedicineDetails.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
        lblMedicineDetails.Padding = new Padding(10, 0, 8, 0);
        lblMedicineDetails.BackColor = Color.FromArgb(232, 245, 239);
        AddRow(root, 2, "Medicine Details", lblMedicineDetails);

        cmbType.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbType.Items.AddRange(StockMovementService.TransactionTypes.Cast<object>().ToArray());
        cmbType.SelectedItem = StockMovementService.StockIn;
        cmbType.Dock = DockStyle.Fill;
        cmbType.SelectedIndexChanged += (_, _) => TypeChanged();
        AddRow(root, 3, "Transaction Type", cmbType);

        cmbBatch.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbBatch.Dock = DockStyle.Top;
        cmbBatch.Height = 30;
        cmbBatch.SelectedIndexChanged += (_, _) => BatchChanged();
        lblBatchHelp.Dock = DockStyle.Bottom;
        lblBatchHelp.Height = 19;
        lblBatchHelp.Font = new Font("Segoe UI", 7.5f);
        lblBatchHelp.ForeColor = Muted;
        lblBatchHelp.TextAlign = ContentAlignment.MiddleLeft;
        var batchPanel = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0) };
        batchPanel.Controls.Add(cmbBatch); batchPanel.Controls.Add(lblBatchHelp);
        AddRow(root, 4, "Batch No.", batchPanel);

        dtExpiry.Format = DateTimePickerFormat.Custom;
        dtExpiry.CustomFormat = "MMM-dd-yyyy";
        dtExpiry.Value = DateTime.Today.AddYears(1);
        dtExpiry.Dock = DockStyle.Fill;
        dtExpiry.Enabled = false;
        AddRow(root, 5, "Expiration Date", dtExpiry);

        numQuantity.DecimalPlaces = 0;
        numQuantity.Minimum = 1;
        numQuantity.Maximum = 999999999M;
        numQuantity.Increment = 1;
        numQuantity.ThousandsSeparator = true;
        numQuantity.Dock = DockStyle.Fill;
        numQuantity.ValueChanged += (_, _) =>
        {
            if (updatingAmounts) return;
            if (totalWasEdited) UpdateCostFromTotal();
            else UpdateTotalFromCost();
        };
        AddRow(root, 6, "Quantity", numQuantity);

        txtUnit.Dock = DockStyle.Fill; txtUnit.ReadOnly = true; StyleEntryText(txtUnit); AddRow(root, 7, "Unit of Measurement", txtUnit);

        numCost.DecimalPlaces = 2; numCost.Minimum = 0; numCost.Maximum = 999999999M; numCost.Increment = 0.01M; numCost.ThousandsSeparator = true; numCost.Dock = DockStyle.Fill;
        numCost.ValueChanged += (_, _) =>
        {
            if (!updatingAmounts) totalWasEdited = false;
            UpdateTotalFromCost();
        };
        AddRow(root, 8, "Cost per Unit (₱)", numCost);

        txtTotal.Dock = DockStyle.Fill; StyleEntryText(txtTotal); txtTotal.ReadOnly = false;
        txtTotal.MaxLength = 18;
        txtTotal.KeyPress += (_, e) =>
        {
            if (char.IsControl(e.KeyChar)) return;
            if (char.IsDigit(e.KeyChar)) return;
            if (e.KeyChar == '.' && !txtTotal.Text.Contains('.')) return;
            e.Handled = true;
        };
        txtTotal.TextChanged += (_, _) =>
        {
            if (updatingAmounts) return;
            string raw = txtTotal.Text;
            var cleaned = new System.Text.StringBuilder(raw.Length);
            bool hasDecimal = false;
            foreach (char ch in raw)
            {
                if (char.IsDigit(ch)) cleaned.Append(ch);
                else if (ch == '.' && !hasDecimal) { cleaned.Append(ch); hasDecimal = true; }
            }
            string sanitized = cleaned.ToString();
            if (!string.Equals(raw, sanitized, StringComparison.Ordinal))
            {
                int caret = txtTotal.SelectionStart;
                updatingAmounts = true;
                txtTotal.Text = sanitized;
                txtTotal.SelectionStart = Math.Min(caret, txtTotal.Text.Length);
                updatingAmounts = false;
            }
            totalWasEdited = true;
            UpdateCostFromTotal();
        };
        AddRow(root, 9, "Total Cost (₱)", txtTotal);

        txtRemarks.Multiline = true; txtRemarks.ScrollBars = ScrollBars.Vertical; txtRemarks.Dock = DockStyle.Fill; StyleEntryText(txtRemarks); txtRemarks.BackColor = Color.White;
        AddRow(root, 10, "Remarks (Optional)", txtRemarks);

        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, WrapContents = false, Padding = new Padding(0, 7, 0, 0) };
        var save = EntryButton("Save Transaction", true); save.Click += (_, _) => Save();
        var clear = EntryButton("Clear Form", false); clear.Click += (_, _) => Clear();
        var cancel = EntryButton("Cancel", false); cancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
        buttons.Controls.Add(save); buttons.Controls.Add(clear); buttons.Controls.Add(cancel);
        root.Controls.Add(buttons, 0, 11); root.SetColumnSpan(buttons, 2);
        AcceptButton = save; CancelButton = cancel;
    }

    private void LoadMedicines()
    {
        try
        {
            allMedicines.Clear(); allMedicines.AddRange(StockMovementService.GetMedicines());
            UpdateMedicineOptions(false);
            if (eligibleMedicines.Count > 0)
            {
                cmbMedicine.SelectedIndex = 0;
                MedicineChanged();
            }
            else lblMedicineDetails.Text = "No active medicines were found in the Medicine Catalog.";
        }
        catch (Exception ex) { MessageBox.Show("Could not load medicines from the catalog.\n\n" + ex.Message, "Stock Movement", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private string CurrentType => Convert.ToString(cmbType.SelectedItem) ?? StockMovementService.StockIn;

    private bool MedicineEligible(StockMovementService.MedicineOption medicine, string type)
    {
        if (type == StockMovementService.StockIn) return true;
        var batches = StockMovementService.GetBatches(medicine.MedId).Where(b => b.QuantityOnHand > 0).ToList();
        if (type == StockMovementService.StockOut)
            return batches.Any(b => !b.ExpiryDate.HasValue || b.ExpiryDate.Value.Date >= DateTime.Today);
        if (type == StockMovementService.Expired)
            return batches.Any(b => b.ExpiryDate.HasValue && b.ExpiryDate.Value.Date < DateTime.Today);
        if (type == StockMovementService.Disposal)
            return batches.Count > 0;
        return false;
    }

    private void UpdateMedicineOptions(bool showEmptyMessage)
    {
        string previousId = (cmbMedicine.SelectedItem as StockMovementService.MedicineOption)?.MedId ?? "";
        string typed = cmbMedicine.Focused ? cmbMedicine.Text : "";
        string type = CurrentType;
        eligibleMedicines.Clear();
        foreach (var medicine in allMedicines)
        {
            try { if (MedicineEligible(medicine, type)) eligibleMedicines.Add(medicine); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine("Medicine eligibility check failed: " + ex.Message); }
        }
        changing = true;
        try
        {
            cmbMedicine.BeginUpdate(); cmbMedicine.Items.Clear();
            foreach (var medicine in eligibleMedicines) cmbMedicine.Items.Add(medicine);
            cmbMedicine.EndUpdate();
            var previous = eligibleMedicines.FirstOrDefault(m => m.MedId == previousId);
            if (previous != null) { cmbMedicine.SelectedItem = previous; }
            else if (eligibleMedicines.Count == 1) { cmbMedicine.SelectedItem = eligibleMedicines[0]; }
            else { cmbMedicine.SelectedIndex = -1; cmbMedicine.Text = typed; }
        }
        finally { changing = false; }
        if (eligibleMedicines.Count == 0)
        {
            cmbBatch.Items.Clear(); lblMedicineDetails.Text = "No eligible medicine/stock for this transaction type.";
            BatchChanged();
            if (showEmptyMessage)
            {
                string message = type == StockMovementService.Expired
                    ? "There is no expired medicine in stock. No medicine is available for Stock Out - Expired."
                    : "There is no available stock for this transaction type.";
                MessageBox.Show(message, "No Eligible Stock", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        else if (previousId.Length == 0 || !eligibleMedicines.Any(m => m.MedId == previousId))
        {
            cmbMedicine.Text = "";
            lblMedicineDetails.Text = "Search and select a medicine ID from the suggestions.";
            cmbBatch.Items.Clear(); BatchChanged();
        }
    }

    private void MedicineTextUpdated(object? sender, EventArgs e)
    {
        if (changing) return;
        string typed = cmbMedicine.Text;
        var matches = eligibleMedicines.Where(m => ($"{m.MedId} {m.GenericName} {m.BrandName} {m.Dosage} {m.DosageForm}").Contains(typed, StringComparison.OrdinalIgnoreCase)).ToList();
        changing = true;
        try
        {
            cmbMedicine.BeginUpdate(); cmbMedicine.Items.Clear(); foreach (var item in matches) cmbMedicine.Items.Add(item); cmbMedicine.Text = typed; cmbMedicine.SelectionStart = typed.Length; cmbMedicine.SelectionLength = 0; cmbMedicine.EndUpdate();
            if (matches.Count > 1 && typed.Length > 0) cmbMedicine.DroppedDown = true;
            if (matches.Count == 1 && typed.Trim().Length > 0)
            {
                cmbMedicine.SelectedItem = matches[0]; cmbMedicine.Text = matches[0].ToString(); cmbMedicine.SelectionStart = cmbMedicine.Text.Length;
            }
            else if (matches.Count == 0 && typed.Length > 0) cmbMedicine.DroppedDown = false;
        }
        finally { changing = false; }
        if (matches.Count == 1 && typed.Trim().Length > 0) MedicineChanged();
    }

    private void MedicineChanged()
    {
        if (changing || cmbMedicine.SelectedItem is not StockMovementService.MedicineOption med) return;
        lblMedicineDetails.Text = $"{med.MedId}  ·  {med.GenericName}  ·  {med.BrandName}  ·  {med.Dosage}  ·  {med.DosageForm}";
        txtUnit.Text = med.Unit;
        try
        {
            changing = true;
            cmbBatch.Items.Clear();
            var batches = StockMovementService.GetBatches(med.MedId).Where(b => b.QuantityOnHand > 0).ToList();
            string type = CurrentType;
            if (type == StockMovementService.StockIn)
            {
                // Stock In always creates a new batch; existing batches are shown only for Stock Out types.
                var newBatch = new NewBatchChoice(StockMovementService.GetNextBatchNo(med.MedId));
                cmbBatch.Items.Add(newBatch);
                cmbBatch.SelectedItem = newBatch;
            }
            else
            {
                IEnumerable<StockMovementService.BatchOption> eligible = type switch
                {
                    var t when t == StockMovementService.StockOut => batches.Where(b => !b.ExpiryDate.HasValue || b.ExpiryDate.Value.Date >= DateTime.Today),
                    var t when t == StockMovementService.Expired => batches.Where(b => b.ExpiryDate.HasValue && b.ExpiryDate.Value.Date < DateTime.Today),
                    var t when t == StockMovementService.Disposal => batches,
                    _ => Enumerable.Empty<StockMovementService.BatchOption>()
                };
                foreach (var batch in eligible.OrderBy(b => b.ExpiryDate ?? DateTime.MaxValue).ThenBy(b => b.BatchNo, StringComparer.OrdinalIgnoreCase)) cmbBatch.Items.Add(batch);
                SelectAutomaticBatch();
            }
        }
        catch (Exception ex) { MessageBox.Show("Could not load medicine batches.\n\n" + ex.Message, "Stock Movement", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        finally { changing = false; }
        BatchChanged();
    }

    private void TypeChanged()
    {
        if (changing) return;
        UpdateMedicineOptions(true);
        if (cmbMedicine.SelectedItem is StockMovementService.MedicineOption) MedicineChanged();
    }

    private void SelectAutomaticBatch()
    {
        string type = CurrentType;
        var batches = cmbBatch.Items.Cast<object>().OfType<StockMovementService.BatchOption>().Where(b => b.QuantityOnHand > 0);
        StockMovementService.BatchOption? selected = type switch
        {
            var t when t == StockMovementService.Expired => batches.Where(b => b.ExpiryDate.HasValue && b.ExpiryDate.Value.Date < DateTime.Today).OrderBy(b => b.ExpiryDate).FirstOrDefault(),
            var t when t == StockMovementService.StockOut => batches.Where(b => !b.ExpiryDate.HasValue || b.ExpiryDate.Value.Date >= DateTime.Today).OrderBy(b => b.ExpiryDate ?? DateTime.MaxValue).FirstOrDefault(),
            var t when t == StockMovementService.Disposal => batches.OrderBy(b => b.ExpiryDate ?? DateTime.MaxValue).FirstOrDefault(),
            _ => batches.FirstOrDefault()
        };
        cmbBatch.SelectedItem = selected;
    }

    private void BatchChanged()
    {
        if (changing) return;
        bool stockIn = CurrentType == StockMovementService.StockIn;
        bool isNew = cmbBatch.SelectedItem is NewBatchChoice;
        totalWasEdited = false;
        bool stockOut = !stockIn;
        dtExpiry.Enabled = stockIn && isNew;
        txtTotal.ReadOnly = stockOut;
        txtTotal.BackColor = stockOut ? Color.FromArgb(240, 247, 243) : Color.White;
        if (isNew && cmbBatch.SelectedItem is NewBatchChoice newBatch)
        {
            numCost.Enabled = true;
            lblBatchHelp.Text = $"New batch number generated for this medicine: {newBatch.BatchNo}";
            if (cmbMedicine.SelectedItem is StockMovementService.MedicineOption m) txtUnit.Text = m.Unit;
            numQuantity.Maximum = 999999999M;
        }
        else if (cmbBatch.SelectedItem is StockMovementService.BatchOption batch)
        {
            txtUnit.Text = batch.Unit;
            updatingAmounts = true;
            numCost.Value = Math.Clamp(decimal.Round(batch.UnitCost, 2, MidpointRounding.AwayFromZero), numCost.Minimum, numCost.Maximum);
            updatingAmounts = false;
            numCost.Enabled = !stockOut;
            lblBatchHelp.Text = $"Available: {batch.QuantityOnHand:N0} {batch.Unit}" + (batch.ExpiryDate.HasValue ? $"  ·  Expires {batch.ExpiryDate.Value:MMM-dd-yyyy}" : "  ·  No expiration recorded");
            numQuantity.Maximum = 999999999M;
        }
        else
        {
            numCost.Enabled = stockIn;
            lblBatchHelp.Text = "No eligible batch is available for this transaction type.";
            numQuantity.Maximum = 999999999M;
        }
        UpdateTotalFromCost();
    }

    private void UpdateTotalFromCost()
    {
        if (updatingAmounts) return;
        updatingAmounts = true;
        txtTotal.Text = decimal.Round(numQuantity.Value * numCost.Value, 2, MidpointRounding.AwayFromZero).ToString("0.00", CultureInfo.InvariantCulture);
        updatingAmounts = false;
    }

    private void UpdateCostFromTotal()
    {
        if (updatingAmounts || !decimal.TryParse(txtTotal.Text.Trim().Replace("₱", "").Replace(",", ""), NumberStyles.Number, CultureInfo.InvariantCulture, out decimal total) || total < 0 || numQuantity.Value <= 0) return;
        updatingAmounts = true;
        decimal cost = decimal.Round(total / numQuantity.Value, 2, MidpointRounding.AwayFromZero);
        numCost.Value = Math.Clamp(cost, numCost.Minimum, numCost.Maximum);
        updatingAmounts = false;
    }

    private void Save()
    {
        var med = cmbMedicine.SelectedItem as StockMovementService.MedicineOption;
        if (med == null)
        {
            string typedId = cmbMedicine.Text.Trim();
            med = eligibleMedicines.FirstOrDefault(m => string.Equals(m.MedId, typedId, StringComparison.OrdinalIgnoreCase))
                ?? eligibleMedicines.FirstOrDefault(m => string.Equals(m.ToString(), typedId, StringComparison.OrdinalIgnoreCase));
            if (med != null) { changing = true; cmbMedicine.SelectedItem = med; changing = false; MedicineChanged(); }
        }
        if (med == null) { Warn("Select a medicine from the suggestions.", cmbMedicine); return; }
        string type = Convert.ToString(cmbType.SelectedItem) ?? "";
        if (string.IsNullOrWhiteSpace(type)) { Warn("Select a transaction type.", cmbType); return; }
        if (numQuantity.Value < 1 || numQuantity.Value != decimal.Truncate(numQuantity.Value)) { Warn("Quantity must be a whole number greater than zero.", numQuantity); return; }
        bool stockIn = type == StockMovementService.StockIn;
        bool isNew = cmbBatch.SelectedItem is NewBatchChoice;
        if (!stockIn && cmbBatch.SelectedItem is not StockMovementService.BatchOption) { Warn(type == StockMovementService.Expired ? "There is no expired medicine available in stock." : "No eligible batch is available. Check the medicine stock and expiration date.", cmbBatch); return; }
        if (stockIn && !isNew) { Warn("For Stock In, choose the automatically generated new batch number.", cmbBatch); return; }
        if (stockIn && dtExpiry.Value.Date < DateTime.Today) { Warn("Expiration date cannot be earlier than today.", dtExpiry); return; }
        if (numCost.Value < 0) { Warn("Cost per unit cannot be negative.", numCost); return; }
        var batch = cmbBatch.SelectedItem as StockMovementService.BatchOption;
        if (!stockIn && batch != null && numQuantity.Value > decimal.Truncate(batch.QuantityOnHand))
        {
            decimal availableWhole = decimal.Truncate(batch.QuantityOnHand);
            if (availableWhole < 1) { Warn("The selected batch has less than one whole unit available. Choose another batch or restock it.", cmbBatch); return; }
            var answer = MessageBox.Show($"The selected batch has only {availableWhole:N0} {batch.Unit} available. Use the current stock ({availableWhole:N0}) instead of {numQuantity.Value:N0}?", "Quantity Exceeds Stock", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (answer == DialogResult.Yes) { numQuantity.Value = availableWhole; UpdateTotalFromCost(); }
            return;
        }
        decimal totalCost = decimal.Round(numQuantity.Value * numCost.Value, 2, MidpointRounding.AwayFromZero);
        if (stockIn && totalWasEdited && decimal.TryParse(txtTotal.Text.Trim().Replace("₱", "").Replace(",", ""), NumberStyles.Number, CultureInfo.InvariantCulture, out decimal enteredTotal) && enteredTotal >= 0)
        {
            // When the user edits Total Transaction, preserve that exact two-decimal total
            // and derive the displayed unit cost from the current whole-number quantity.
            totalCost = decimal.Round(enteredTotal, 2, MidpointRounding.AwayFromZero);
            decimal computedCost = decimal.Round(totalCost / numQuantity.Value, 2, MidpointRounding.AwayFromZero);
            updatingAmounts = true;
            numCost.Value = Math.Clamp(computedCost, numCost.Minimum, numCost.Maximum);
            updatingAmounts = false;
        }
        var confirm = MessageBox.Show($"Save {type} for {med.MedId} — {med.GenericName}?\nBatch: {(isNew ? ((NewBatchChoice)cmbBatch.SelectedItem!).BatchNo : batch!.BatchNo)}\nQuantity: {numQuantity.Value:N0} {txtUnit.Text}\nCost per unit: ₱{numCost.Value:N2}\nTotal transaction: ₱{totalCost:N2}\nDate and time will be recorded automatically when you confirm.", "Confirm Transaction", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (confirm != DialogResult.Yes) return;
        DateTime now = DateTime.Now;
        try
        {
            string? newBatchNo = isNew ? ((NewBatchChoice)cmbBatch.SelectedItem!).BatchNo : null;
            var request = new StockMovementService.SaveRequest(now, med.MedId, batch?.BatchId,
                newBatchNo, isNew ? dtExpiry.Value.Date : null, type, numQuantity.Value,
                (stockIn ? numCost.Value : batch!.UnitCost), txtUnit.Text.Trim(), txtRemarks.Text.Trim(), session.Username);
            var result = StockMovementService.Save(request);
            SavedTransactionId = result.TransactionId; SavedBalance = result.NewBalance; SavedUnit = txtUnit.Text;
            DialogResult = DialogResult.OK; Close();
        }
        catch (Exception ex) { MessageBox.Show("The transaction could not be saved. No inventory change was committed.\n\n" + ex.Message, "Save Transaction", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void Clear()
    {
        if (MessageBox.Show("Clear the unsaved transaction details?", "Clear Form", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        txtRemarks.Clear(); totalWasEdited = false; numQuantity.Value = 1; numCost.Value = 0; dtExpiry.Value = DateTime.Today.AddYears(1);
        cmbType.SelectedItem = StockMovementService.StockIn;
        UpdateMedicineOptions(false);
        if (eligibleMedicines.Count > 0) cmbMedicine.SelectedIndex = 0;
        if (cmbMedicine.SelectedItem is StockMovementService.MedicineOption) MedicineChanged(); else BatchChanged();
    }

    private void Warn(string message, Control control) { MessageBox.Show(message, "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); control.Focus(); }

    private static void AddRow(TableLayoutPanel panel, int row, string label, Control control)
    {
        panel.Controls.Add(new Label { Text = label, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, ForeColor = DarkGreen, Font = new Font("Segoe UI", 8.5f, FontStyle.Bold), Margin = new Padding(0, 3, 10, 3) }, 0, row);
        control.Margin = new Padding(0, 4, 0, 4);
        if (control is TextBox || control is ComboBox || control is NumericUpDown || control is DateTimePicker)
        {
            control.Font = new Font("Segoe UI", 9.5f);
            if (control is not TextBox { Multiline: true }) control.Height = 32;
        }
        panel.Controls.Add(control, 1, row);
    }

    private static void StyleEntryText(TextBox box)
    {
        box.Font = new Font("Segoe UI", 9.5f); box.BorderStyle = BorderStyle.FixedSingle;
        box.BackColor = box.ReadOnly ? Color.FromArgb(240, 247, 243) : Color.White; box.ForeColor = DarkGreen;
    }

    private static Button EntryButton(string text, bool primary)
    {
        var b = new Button { Text = text, Width = primary ? 160 : 112, Height = 34, FlatStyle = FlatStyle.Flat, BackColor = primary ? Green : Color.White, ForeColor = primary ? Color.White : DarkGreen, Font = new Font("Segoe UI", 9f, FontStyle.Bold), UseVisualStyleBackColor = false, Margin = new Padding(6, 0, 0, 0), Cursor = Cursors.Hand };
        b.FlatAppearance.BorderColor = Color.FromArgb(220, 232, 226); b.FlatAppearance.BorderSize = primary ? 0 : 1; return b;
    }
}
