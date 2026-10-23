using System.Data;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;
using InventoryMonitoringSystem.Services;

namespace InventoryMonitoringSystem;

/// <summary>Adaptive inventory control workspace: live stock register, batch expiry risk, and movement analytics.</summary>
public sealed class InventoryControlControl : UserControl
{
    private readonly UserSession session;
    private readonly TextBox searchBox = new();
    private readonly ComboBox statusFilter = new();
    private readonly ComboBox expiryDays = new();
    private readonly DateTimePicker startDate = new();
    private readonly DateTimePicker endDate = new();
    private readonly ComboBox quickPeriod = new();
    private readonly ComboBox ranking = new();
    private readonly ComboBox movementDirection = new();
    private readonly ComboBox riskFilter = new();
    private readonly Label statusMessage = new();
    private readonly FlowLayoutPanel metricFlow = new();
    private readonly TabControl tabs = new();
    private readonly DataGridView stockGrid = MakeGrid();
    private readonly DataGridView expiryGrid = MakeGrid();
    private readonly DataGridView moversGrid = MakeGrid();
    private readonly DataGridView trendGrid = MakeGrid();
    private readonly TrendChartPanel overviewTrendChart = new();
    private readonly TrendChartPanel analyticsTrendChart = new();
    private readonly TrendChartPanel moversChart = new() { ShowSingleSeries = true };
    private readonly Label[] metricValues = new Label[7];
    private readonly Label[] metricCaptions = new Label[7];
    private DataTable? lastStockTable;
    private bool loading;
    private bool changingQuickPeriod;

    private static readonly Color Ink = Color.FromArgb(28, 43, 35);
    private static readonly Color Muted = Color.FromArgb(104, 119, 110);
    private static readonly Color Green = Color.FromArgb(28, 120, 75);
    private static readonly Color Pale = Color.FromArgb(246, 249, 247);
    private static readonly Color Border = Color.FromArgb(224, 232, 226);

    public event EventHandler? DataChanged;
    public event EventHandler? RequestStockMovement;

    public InventoryControlControl(UserSession currentSession)
    {
        session = currentSession;
        Dock = DockStyle.Fill;
        BackColor = Pale;
        Padding = new Padding(2);
        BuildUi();
    }

    private void BuildUi()
    {
        // One shared vertical scrollbar belongs to the Inventory Control content itself.
        // Individual tabs do not create their own vertical scroll containers; tables retain
        // their native grid scrolling when their row count exceeds the available area.
        var contentScroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Pale, Padding = new Padding(0) };
        var root = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 1, RowCount = 4, BackColor = Pale, Padding = new Padding(0), Margin = new Padding(0) };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tabs.Height = 1120;
        tabs.Dock = DockStyle.Top;
        contentScroll.Controls.Add(root);
        Controls.Add(contentScroll);
        contentScroll.Resize += (_, _) => root.Width = Math.Max(260, contentScroll.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - 2);

        var header = new Panel { Dock = DockStyle.Top, Height = 86, BackColor = Pale, Margin = new Padding(0) };
        var title = new Label { Text = "Inventory Control", Font = new Font("Segoe UI", 19, FontStyle.Bold), ForeColor = Ink, AutoSize = true, Location = new Point(2, 3) };
        var subtitle = new Label { Text = "Live stock position, batch expiry risk and movement performance", Font = new Font("Segoe UI", 9.5f), ForeColor = Muted, AutoSize = true, Location = new Point(4, 40) };
        var refresh = MakeButton("Refresh", Green, Color.White);
        refresh.Anchor = AnchorStyles.Top | AnchorStyles.Right; refresh.Size = new Size(92, 34); refresh.Click += (_, _) => RefreshAll();
        var movement = MakeButton("Record movement", Color.FromArgb(231, 243, 235), Green); movement.Anchor = AnchorStyles.Top | AnchorStyles.Right; movement.Size = new Size(142, 34); movement.Click += (_, _) => RequestStockMovement?.Invoke(this, EventArgs.Empty);
        var export = MakeButton("Export CSV", Color.White, Green); export.FlatAppearance.BorderColor = Border; export.Anchor = AnchorStyles.Top | AnchorStyles.Right; export.Size = new Size(100, 34); export.Click += (_, _) => ExportCurrentTable();
        statusMessage.Text = "Live database data"; statusMessage.Font = new Font("Segoe UI", 8f); statusMessage.ForeColor = Muted; statusMessage.AutoSize = true; statusMessage.Location = new Point(4, 63);
        header.Controls.Add(title); header.Controls.Add(subtitle); header.Controls.Add(refresh); header.Controls.Add(movement); header.Controls.Add(export); header.Controls.Add(statusMessage);
        header.Resize += (_, _) =>
        {
            bool compact = header.ClientSize.Width < 850;
            subtitle.Visible = !compact;
            title.Font = new Font("Segoe UI", compact ? 16f : 19f, FontStyle.Bold);
            export.Text = compact ? "Export" : "Export CSV"; export.Width = compact ? 76 : 100;
            movement.Text = compact ? "Movement" : "Record movement"; movement.Width = compact ? 96 : 142;
            refresh.Location = new Point(Math.Max(8, header.ClientSize.Width - refresh.Width - 4), 10);
            export.Location = new Point(Math.Max(8, refresh.Left - export.Width - 8), 10);
            movement.Location = new Point(Math.Max(8, export.Left - movement.Width - 8), 10);
        };
        root.Controls.Add(header, 0, 0);

        var filterCard = new Panel { Dock = DockStyle.Top, Height = 86, BackColor = Color.White, Padding = new Padding(12), Margin = new Padding(0, 0, 0, 12) };
        filterCard.Paint += (_, e) => DrawBorder(filterCard, e.Graphics);
        var filterFlow = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, WrapContents = false, FlowDirection = FlowDirection.LeftToRight, BackColor = Color.White, Padding = new Padding(0, 7, 0, 0), Margin = new Padding(0) };
        filterCard.Controls.Add(filterFlow);
        searchBox.Width = 205; searchBox.Font = new Font("Segoe UI", 9.5f); searchBox.PlaceholderText = "Search medicine, ID, brand...";
        statusFilter.DropDownStyle = ComboBoxStyle.DropDownList; statusFilter.Width = 145; statusFilter.Font = new Font("Segoe UI", 9.5f);
        statusFilter.Items.AddRange(new object[] { "All", "Healthy", "Low Stock", "Out of Stock", "Expiring", "Expired Batch" }); statusFilter.SelectedIndex = 0;
        expiryDays.DropDownStyle = ComboBoxStyle.DropDownList; expiryDays.Width = 105; expiryDays.Font = new Font("Segoe UI", 9.5f); expiryDays.Items.AddRange(new object[] { "7 days", "30 days", "60 days", "90 days" }); expiryDays.SelectedIndex = 1;
        AddFilterItem(filterFlow, "Search", searchBox, 220); AddFilterItem(filterFlow, "Stock status", statusFilter, 160); AddFilterItem(filterFlow, "Expiry window", expiryDays, 120);
        root.Controls.Add(filterCard, 0, 1);

        metricFlow.Dock = DockStyle.Fill; metricFlow.AutoSize = true; metricFlow.WrapContents = true; metricFlow.FlowDirection = FlowDirection.LeftToRight; metricFlow.BackColor = Pale; metricFlow.Margin = new Padding(0, 0, 0, 10); metricFlow.Padding = new Padding(0);
        string[] captions = ["ACTIVE MEDICINES", "USABLE UNITS", "LOW STOCK", "OUT OF STOCK", "EXPIRING MEDICINES", "EXPIRED UNITS", "USABLE STOCK VALUE"];
        for (int i = 0; i < captions.Length; i++) metricFlow.Controls.Add(MakeMetric(i, captions[i]));
        root.Controls.Add(metricFlow, 0, 2);

        tabs.Dock = DockStyle.Top; tabs.Height = 1120; tabs.Font = new Font("Segoe UI", 9.5f); tabs.Padding = new Point(16, 6); tabs.DrawMode = TabDrawMode.Normal;
        tabs.TabPages.Add(BuildOverviewTab());
        tabs.TabPages.Add(BuildStockTab());
        tabs.TabPages.Add(BuildExpiryTab());
        tabs.TabPages.Add(BuildAnalyticsTab());
        root.Controls.Add(tabs, 0, 3);

        searchBox.TextChanged += (_, _) => { if (!loading && IsHandleCreated) LoadStockRegister(); };
        statusFilter.SelectedIndexChanged += (_, _) => { if (!loading && IsHandleCreated) LoadStockRegister(); };
        expiryDays.SelectedIndexChanged += (_, _) => { if (!loading && IsHandleCreated) RefreshAll(); };
    }

    private Control MakeMetric(int index, string caption)
    {
        var card = new Panel { Width = 180, Height = 88, BackColor = Color.White, Margin = new Padding(0, 0, 10, 10), Padding = new Padding(13) };
        card.Paint += (_, e) => DrawBorder(card, e.Graphics);
        metricCaptions[index] = new Label { Text = caption, Font = new Font("Segoe UI", 8f, FontStyle.Bold), ForeColor = Muted, AutoSize = false, Dock = DockStyle.Top, Height = 24, TextAlign = ContentAlignment.MiddleLeft };
        metricValues[index] = new Label { Text = "—", Font = new Font("Segoe UI", 20, FontStyle.Bold), ForeColor = index is 2 or 4 or 5 ? Color.FromArgb(180, 83, 9) : index == 3 ? Color.FromArgb(185, 28, 28) : Green, AutoSize = false, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
        card.Controls.Add(metricValues[index]); card.Controls.Add(metricCaptions[index]);
        return card;
    }

    private TabPage BuildOverviewTab()
    {
        var page = MakePage("Overview");
        // The shared Inventory Control scrollbar handles vertical movement for all tabs.
        var layout = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 1, RowCount = 4, BackColor = Pale, Padding = new Padding(0), Margin = new Padding(0) };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 340));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 340));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 215));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 175));
        var chartCard = MakeCard("Stock movement trend", out var chartBody); chartCard.Height = 330; chartCard.MinimumSize = new Size(0, 300);
        overviewTrendChart.Dock = DockStyle.Fill; overviewTrendChart.MinimumSize = new Size(240, 230); chartBody.Controls.Add(overviewTrendChart);
        var moversCard = MakeCard("Top stock-in medicines", out var moversBody); moversCard.Height = 330; moversCard.MinimumSize = new Size(0, 300);
        moversChart.Dock = DockStyle.Fill; moversChart.MinimumSize = new Size(240, 230); moversBody.Controls.Add(moversChart);
        var alertsCard = MakeCard("Priority attention", out var alertsBody); alertsCard.Height = 205; alertsCard.MinimumSize = new Size(0, 185);
        var alerts = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = false, BackColor = Color.White, Padding = new Padding(4) };
        alertsBody.Controls.Add(alerts);
        AddAlert(alerts, "Expired batches", "Review and quarantine according to your procedure", Color.FromArgb(185, 28, 28));
        AddAlert(alerts, "Low stock", "Replenishment may be required", Color.FromArgb(161, 98, 7));
        AddAlert(alerts, "Expiring batches", "Review FEFO issue and expiry schedule", Color.FromArgb(161, 98, 7));
        alerts.Resize += (_, _) => { foreach (Control item in alerts.Controls) item.Width = Math.Max(220, alerts.ClientSize.Width - 28); };
        var summaryCard = MakeCard("How to read this dashboard", out var summaryBody); summaryCard.Height = 165; summaryCard.MinimumSize = new Size(0, 145);
        summaryBody.Controls.Add(new Label { Dock = DockStyle.Fill, Text = "• Usable stock excludes batches already past expiry.\r\n• Expiry risk is counted at batch level.\r\n• Movement charts use saved transaction history and the selected date range.\r\n• Inventory value is based on the recorded batch unit cost.", Font = new Font("Segoe UI", 9.5f), ForeColor = Muted, Padding = new Padding(12), AutoSize = false });
        layout.Controls.Add(chartCard, 0, 0); layout.Controls.Add(moversCard, 0, 1); layout.Controls.Add(alertsCard, 0, 2); layout.Controls.Add(summaryCard, 0, 3);
        page.Controls.Add(layout);
        page.Resize += (_, _) => layout.Width = Math.Max(260, page.ClientSize.Width - 18);
        return page;
    }

    private TabPage BuildStockTab()
    {
        var page = MakePage("Stock Register");
        var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(8) }; panel.Paint += (_, e) => DrawBorder(panel, e.Graphics);
        ConfigureGrid(stockGrid); panel.Controls.Add(stockGrid); page.Controls.Add(panel); return page;
    }

    private TabPage BuildExpiryTab()
    {
        var page = MakePage("Expiry & Risk");
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = Pale, Padding = new Padding(0) };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        var toolbar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 48, WrapContents = false, BackColor = Pale, Padding = new Padding(0, 4, 0, 6) };
        riskFilter.DropDownStyle = ComboBoxStyle.DropDownList; riskFilter.Width = 140; riskFilter.Font = new Font("Segoe UI", 9.5f); riskFilter.Items.AddRange(new object[] { "All", "Expired", "Expiring" }); riskFilter.SelectedIndex = 0;
        var label = new Label { Text = "Show positive-quantity batches that are expired or inside the selected expiry window", AutoSize = true, ForeColor = Muted, Margin = new Padding(10, 8, 8, 0), Font = new Font("Segoe UI", 9f) };
        toolbar.Controls.Add(riskFilter); toolbar.Controls.Add(label); layout.Controls.Add(toolbar, 0, 0);
        var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(8) }; panel.Paint += (_, e) => DrawBorder(panel, e.Graphics); ConfigureGrid(expiryGrid); panel.Controls.Add(expiryGrid); layout.Controls.Add(panel, 0, 1);
        riskFilter.SelectedIndexChanged += (_, _) => { if (!loading) LoadExpiryRisk(); };
        page.Controls.Add(layout); return page;
    }

    private TabPage BuildAnalyticsTab()
    {
        var page = MakePage("Movement Analytics");
        // The parent Inventory Control content owns vertical scrolling; keep this tab's
        // filters, chart and ranking table in a simple stacked layout without nested scrollbars.
        var root = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 1, RowCount = 3, BackColor = Pale, Padding = new Padding(0), Margin = new Padding(0) };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 88));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 360));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 350));
        var filters = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 78, AutoScroll = true, WrapContents = false, BackColor = Pale, Padding = new Padding(0, 5, 0, 4) };
        quickPeriod.DropDownStyle = ComboBoxStyle.DropDownList; quickPeriod.Width = 130; quickPeriod.Font = new Font("Segoe UI", 9.5f); quickPeriod.Items.AddRange(new object[] { "Last 7 days", "Last 30 days", "This month", "This year", "Custom range" }); quickPeriod.SelectedIndex = 1;
        startDate.Width = 125; endDate.Width = 125; startDate.Format = DateTimePickerFormat.Short; endDate.Format = DateTimePickerFormat.Short; startDate.Value = DateTime.Today.AddDays(-29); endDate.Value = DateTime.Today;
        ranking.DropDownStyle = ComboBoxStyle.DropDownList; ranking.Width = 135; ranking.Items.AddRange(new object[] { "Frequency", "Total quantity" }); ranking.SelectedIndex = 0;
        movementDirection.DropDownStyle = ComboBoxStyle.DropDownList; movementDirection.Width = 115; movementDirection.Items.AddRange(new object[] { "Stock In", "Stock Out" }); movementDirection.SelectedIndex = 0;
        var apply = MakeButton("Apply filters", Green, Color.White); apply.Size = new Size(112, 32); apply.Margin = new Padding(6, 19, 0, 0); apply.Click += (_, _) => LoadAnalytics();
        AddFilterItem(filters, "Period", quickPeriod, 145); AddFilterItem(filters, "Start date", startDate, 140); AddFilterItem(filters, "End date", endDate, 140); AddFilterItem(filters, "Rank by", ranking, 150); AddFilterItem(filters, "Direction", movementDirection, 130); filters.Controls.Add(apply);
        quickPeriod.SelectedIndexChanged += (_, _) => { if (quickPeriod.SelectedItem?.ToString() != "Custom range") SetQuickDates(); };
        startDate.ValueChanged += (_, _) => { if (!changingQuickPeriod && quickPeriod.Items.Contains("Custom range")) quickPeriod.SelectedItem = "Custom range"; };
        endDate.ValueChanged += (_, _) => { if (!changingQuickPeriod && quickPeriod.Items.Contains("Custom range")) quickPeriod.SelectedItem = "Custom range"; };
        var trendCard = MakeCard("Daily stock movement", out var trendBody); trendCard.Height = 350; trendCard.MinimumSize = new Size(0, 320); analyticsTrendChart.Dock = DockStyle.Fill; analyticsTrendChart.MinimumSize = new Size(240, 250); trendBody.Controls.Clear(); trendBody.Controls.Add(analyticsTrendChart);
        var gridCard = MakeCard("Most active medicines in selected period", out var gridBody); gridCard.Height = 340; gridCard.MinimumSize = new Size(0, 300); ConfigureGrid(moversGrid); gridBody.Controls.Add(moversGrid);
        filters.Height = 82; root.Controls.Add(filters, 0, 0); root.Controls.Add(trendCard, 0, 1); root.Controls.Add(gridCard, 0, 2); page.Controls.Add(root);
        page.Resize += (_, _) => root.Width = Math.Max(260, page.ClientSize.Width - 18);
        return page;
    }

    private static TabPage MakePage(string text) => new(text) { BackColor = Pale, Padding = new Padding(8) };
    private static Panel MakeCard(string title, out Panel body)
    {
        var card = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(12), Margin = new Padding(0, 0, 10, 10) };
        card.Paint += (_, e) => DrawBorder(card, e.Graphics);
        var heading = new Label { Text = title, Dock = DockStyle.Top, Height = 32, Font = new Font("Segoe UI", 10.5f, FontStyle.Bold), ForeColor = Ink, TextAlign = ContentAlignment.MiddleLeft };
        body = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(0, 4, 0, 0) };
        card.Controls.Add(body); card.Controls.Add(heading); return card;
    }

    private static void AddAlert(FlowLayoutPanel parent, string heading, string description, Color color)
    {
        var item = new Panel { Width = 520, Height = 54, BackColor = Color.White, Margin = new Padding(0, 0, 0, 6), Padding = new Padding(10) };
        item.Paint += (_, e) => { using var p = new Pen(Border); e.Graphics.DrawLine(p, 0, item.Height - 1, item.Width, item.Height - 1); using var b = new SolidBrush(color); e.Graphics.FillEllipse(b, 2, 13, 8, 8); };
        item.Controls.Add(new Label { Text = heading, Location = new Point(20, 3), AutoSize = true, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = Ink });
        item.Controls.Add(new Label { Text = description, Location = new Point(20, 25), AutoSize = true, Font = new Font("Segoe UI", 8.5f), ForeColor = Muted }); parent.Controls.Add(item);
    }

    private static void AddFilterItem(FlowLayoutPanel flow, string caption, Control control, int width)
    {
        var wrapper = new Panel { Width = width, Height = 55, Margin = new Padding(0, 0, 8, 0), BackColor = Color.White };
        wrapper.Controls.Add(new Label { Text = caption, Location = new Point(0, 0), AutoSize = true, Font = new Font("Segoe UI", 8f, FontStyle.Bold), ForeColor = Muted });
        control.Location = new Point(0, 20); control.Height = 29; control.Width = width - 4; wrapper.Controls.Add(control); flow.Controls.Add(wrapper);
    }

    private static Button MakeButton(string text, Color back, Color fore) => new() { Text = text, BackColor = back, ForeColor = fore, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold), Cursor = Cursors.Hand, Height = 34, Padding = new Padding(8, 0, 8, 0), Margin = new Padding(0), UseVisualStyleBackColor = false };
    private static void DrawBorder(Control control, Graphics g) { using var pen = new Pen(Border); g.DrawRectangle(pen, 0, 0, Math.Max(0, control.Width - 1), Math.Max(0, control.Height - 1)); }

    private static DataGridView MakeGrid() => new() { Dock = DockStyle.Fill, BackgroundColor = Color.White, BorderStyle = BorderStyle.None, GridColor = Color.FromArgb(235, 240, 236), EnableHeadersVisualStyles = false, ColumnHeadersHeight = 40, ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing, RowTemplate = { Height = 36 }, AllowUserToAddRows = false, AllowUserToDeleteRows = false, ReadOnly = true, MultiSelect = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None, AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None, RowHeadersVisible = false };
    private static void ConfigureGrid(DataGridView grid)
    {
        grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(241, 247, 243); grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(46, 72, 57); grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold); grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 0, 8, 0);
        grid.DefaultCellStyle.Font = new Font("Segoe UI", 9); grid.DefaultCellStyle.ForeColor = Ink; grid.DefaultCellStyle.BackColor = Color.White; grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(220, 242, 229); grid.DefaultCellStyle.SelectionForeColor = Ink; grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 252, 250); grid.RowTemplate.Height = 36;
        grid.Dock = DockStyle.Top;
        grid.DataBindingComplete += (_, _) =>
        {
            foreach (DataGridViewColumn column in grid.Columns) column.SortMode = DataGridViewColumnSortMode.Automatic;
            if (grid.Columns["Status"] is DataGridViewColumn statusColumn) statusColumn.DefaultCellStyle.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            SizeColumnsToContent(grid);
            FitGridToRows(grid);
        };
        grid.Resize += (_, _) =>
        {
            SizeColumnsToContent(grid);
            FitGridToRows(grid);
        };
        grid.ParentChanged += (_, _) => FitGridToRows(grid);
    }

    // Keep a short table compact so the horizontal scrollbar sits immediately beneath
    // the last row. When there are many rows, use the available panel height and let the
    // DataGridView provide its normal vertical scrollbar.
    private static void FitGridToRows(DataGridView grid)
    {
        if (grid.IsDisposed || grid.Parent is not Control host || host.ClientSize.Height <= 0) return;
        int availableHeight = Math.Max(90, host.ClientSize.Height - host.Padding.Vertical);
        int rowHeight = grid.RowTemplate.Height > 0 ? grid.RowTemplate.Height : 36;
        int visibleRows = Math.Min(grid.Rows.Count, 12);
        int desiredHeight = grid.Rows.Count > 12
            ? availableHeight
            : grid.ColumnHeadersHeight + (visibleRows * rowHeight) + 4;
        int availableWidth = Math.Max(0, host.ClientSize.Width - host.Padding.Horizontal);
        int totalColumnWidth = 0;
        foreach (DataGridViewColumn column in grid.Columns)
            if (column.Visible) totalColumnWidth += column.Width;
        if (totalColumnWidth > availableWidth) desiredHeight += SystemInformation.HorizontalScrollBarHeight + 2;
        grid.Height = Math.Min(availableHeight, Math.Max(80, desiredHeight));
    }

    // Size columns from actual values: compact numeric/date fields, content-aware names, and
    // a responsive fill only for leftover space. Long medicine names receive more room but are capped.
    private static void SizeColumnsToContent(DataGridView grid)
    {
        if (grid.IsDisposed || grid.Columns.Count == 0 || grid.ClientSize.Width <= 20) return;
        try
        {
            int available = Math.Max(200, grid.ClientSize.Width - 4);
            int used = 0;
            DataGridViewColumn? flexible = null;
            var widths = new List<(DataGridViewColumn Column, int Width, int Min, int Max)>();
            foreach (DataGridViewColumn column in grid.Columns)
            {
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                int maxContent = TextRenderer.MeasureText(column.HeaderText ?? string.Empty, grid.ColumnHeadersDefaultCellStyle.Font).Width + 30;
                int sampleRows = Math.Min(grid.Rows.Count, 160);
                for (int i = 0; i < sampleRows; i++)
                {
                    var row = grid.Rows[i];
                    if (row.IsNewRow) continue;
                    string value = Convert.ToString(row.Cells[column.Index].FormattedValue, CultureInfo.CurrentCulture) ?? string.Empty;
                    if (value.Length > 90) value = value[..90];
                    maxContent = Math.Max(maxContent, TextRenderer.MeasureText(value, grid.DefaultCellStyle.Font).Width + 24);
                }
                string name = (column.Name + " " + column.HeaderText).ToLowerInvariant();
                int min = 76, max = 190;
                if (name.Contains("medicine") || name.Contains("description") || name.Contains("brand") || name.Contains("generic name")) { min = 145; max = Math.Min(380, Math.Max(230, available / 2)); }
                else if (name.Contains("status")) { min = 105; max = 165; }
                else if (name.Contains("date") || name.Contains("expiry")) { min = 92; max = 145; }
                else if (name.Contains("id") || name.Contains("batch")) { min = 82; max = 150; }
                else if (name.Contains("quantity") || name.Contains("units") || name.Contains("count") || name.Contains("frequency")) { min = 78; max = 125; }
                else if (name.Contains("value") || name.Contains("cost") || name.Contains("price")) { min = 90; max = 145; }
                int width = Math.Clamp(maxContent, min, max);
                column.MinimumWidth = min;
                widths.Add((column, width, min, max));
                used += width;
                if (name.Contains("medicine") || name.Contains("description") || name.Contains("generic name")) flexible = column;
            }

            // Keep compact fields compact. If content widths exceed the viewport, reduce
            // them toward their minimums before allowing horizontal scrolling. This prevents
            // an unnecessary bottom scrollbar and the large empty band above it on wide screens.
            if (used > available)
            {
                for (int i = widths.Count - 1; i >= 0 && used > available; i--)
                {
                    var item = widths[i];
                    if (item.Column == flexible) continue;
                    int reduction = Math.Min(item.Width - item.Min, used - available);
                    if (reduction > 0)
                    {
                        widths[i] = (item.Column, item.Width - reduction, item.Min, item.Max);
                        used -= reduction;
                    }
                }
                for (int i = 0; i < widths.Count && used > available; i++)
                {
                    var item = widths[i];
                    int reduction = Math.Min(item.Width - item.Min, used - available);
                    if (reduction > 0)
                    {
                        widths[i] = (item.Column, item.Width - reduction, item.Min, item.Max);
                        used -= reduction;
                    }
                }
            }

            foreach (var item in widths) item.Column.Width = item.Width;
            if (flexible != null)
            {
                var flexItem = widths.FirstOrDefault(x => x.Column == flexible);
                if (used < available && flexItem.Column != null)
                    flexible.Width = Math.Min(flexItem.Width + available - used, flexItem.Max);
            }
        }
        catch (InvalidOperationException) { /* A binding/resize is in progress; the next layout pass will retry. */ }
    }

    private int ExpiryDays => int.TryParse(expiryDays.SelectedItem?.ToString()?.Split(' ')[0], out int days) ? days : 30;
    private DateTime RangeStart => startDate.Value.Date;
    private DateTime RangeEnd => endDate.Value.Date < startDate.Value.Date ? startDate.Value.Date : endDate.Value.Date;

    private void ExportCurrentTable()
    {
        DataGridView grid = tabs.SelectedIndex switch { 1 => stockGrid, 2 => expiryGrid, 3 => moversGrid, _ => stockGrid };
        if (grid.DataSource is not DataTable table || table.Columns.Count == 0) { MessageBox.Show(this, "There is no data to export.", "Export CSV", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
        using SaveFileDialog dialog = new() { Filter = "CSV file (*.csv)|*.csv", FileName = $"Inventory_{DateTime.Now:yyyyMMdd_HHmm}.csv", Title = "Export inventory data" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            using var writer = new System.IO.StreamWriter(dialog.FileName, false, new System.Text.UTF8Encoding(true));
            writer.WriteLine(string.Join(",", table.Columns.Cast<DataColumn>().Select(c => Csv(c.ColumnName))));
            foreach (DataRow row in table.Rows) writer.WriteLine(string.Join(",", table.Columns.Cast<DataColumn>().Select(c => Csv(row[c] == DBNull.Value ? "" : Convert.ToString(row[c], CultureInfo.CurrentCulture) ?? ""))));
            statusMessage.Text = "Exported " + table.Rows.Count + " rows to CSV.";
        }
        catch (Exception ex) { MessageBox.Show(this, "Could not export CSV: " + ex.Message, "Export CSV", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        static string Csv(string value) => "\"" + value.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";
    }

    public void RefreshAll()
    {
        if (loading || IsDisposed) return;
        loading = true;
        try
        {
            statusMessage.Text = "Loading live inventory data…";
            DataTable summary = InventoryControlService.GetSummary(ExpiryDays);
            if (summary.Rows.Count > 0)
            {
                DataRow r = summary.Rows[0];
                SetMetric(0, r["TotalMedicines"]); SetMetric(1, r["UsableUnits"]); SetMetric(2, r["LowStock"]); SetMetric(3, r["OutOfStock"]); SetMetric(4, r["WithExpiring"]); SetMetric(5, r["ExpiredUnits"]);
                metricCaptions[4].Text = "EXPIRING MEDICINES"; SetMetric(6, r["InventoryValue"]);
            }
            LoadStockRegister(); LoadExpiryRisk(); LoadAnalytics();
            statusMessage.Text = $"Updated {DateTime.Now:MMM d, yyyy h:mm tt} · Data source: inventory batches and transaction ledger";
            DataChanged?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            statusMessage.Text = "Inventory data could not be loaded.";
            MessageBox.Show(this, "Inventory Control could not load live data. Check the database connection and schema.\r\n\r\nDetails: " + ex.Message, "Inventory Control", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally { loading = false; }
    }

    private void SetMetric(int index, object value)
    {
        decimal number = value == DBNull.Value ? 0 : Convert.ToDecimal(value, CultureInfo.InvariantCulture);
        metricValues[index].Text = index == 6 ? number.ToString("₱#,##0.00", CultureInfo.CurrentCulture) : number.ToString("N0", CultureInfo.CurrentCulture);
    }

    private void LoadStockRegister()
    {
        try
        {
            lastStockTable = InventoryControlService.GetStockRegister(ExpiryDays, searchBox.Text, statusFilter.SelectedItem?.ToString() ?? "All");
            stockGrid.DataSource = lastStockTable;
            StyleStatusCells(stockGrid);
            ConfigureStockColumns();
        }
        catch (Exception ex) { statusMessage.Text = "Stock register error: " + ex.Message; }
    }

    private void ConfigureStockColumns()
    {
        SizeColumnsToContent(stockGrid);
        if (stockGrid.Columns["Status"] is DataGridViewColumn statusColumn) statusColumn.MinimumWidth = 120;
        // These are item/unit counts, so display them as whole numbers (no decimal places).
        foreach (string columnName in new[] { "Physical Stock", "Usable Stock", "Expired Qty", "Expiring Qty" })
            if (stockGrid.Columns[columnName] is DataGridViewColumn quantityColumn)
                quantityColumn.DefaultCellStyle.Format = "N0";
        if (stockGrid.Columns["Stock Value"] is DataGridViewColumn valueColumn) valueColumn.DefaultCellStyle.Format = "N2";
        if (stockGrid.Columns["Last Movement"] is DataGridViewColumn movementColumn) movementColumn.DefaultCellStyle.Format = "dd MMM yyyy";
        FitGridToRows(stockGrid);
    }

    private static void StyleStatusCells(DataGridView grid)
    {
        if (!grid.Columns.Contains("Status")) return;
        foreach (DataGridViewRow row in grid.Rows)
        {
            string status = Convert.ToString(row.Cells["Status"].Value) ?? "";
            var cell = row.Cells["Status"];
            if (status.Contains("Out of Stock", StringComparison.OrdinalIgnoreCase) || status == "Expired") { cell.Style.BackColor = Color.FromArgb(254, 226, 226); cell.Style.ForeColor = Color.FromArgb(153, 27, 27); }
            else if (status.Contains("Low Stock", StringComparison.OrdinalIgnoreCase) || status == "Expiring") { cell.Style.BackColor = Color.FromArgb(254, 243, 199); cell.Style.ForeColor = Color.FromArgb(146, 64, 14); }
            else { cell.Style.BackColor = Color.FromArgb(220, 252, 231); cell.Style.ForeColor = Color.FromArgb(22, 101, 52); }
        }
    }

    private void LoadExpiryRisk()
    {
        try
        {
            expiryGrid.DataSource = InventoryControlService.GetExpiryRisk(ExpiryDays, riskFilter.SelectedItem?.ToString() ?? "All");
            SizeColumnsToContent(expiryGrid);
            if (expiryGrid.Columns["Expiry Date"] is DataGridViewColumn expiryColumn) expiryColumn.DefaultCellStyle.Format = "dd MMM yyyy";
            if (expiryGrid.Columns["Value at Risk"] is DataGridViewColumn valueColumn) valueColumn.DefaultCellStyle.Format = "N2";
            FitGridToRows(expiryGrid);
        }
        catch (Exception ex) { statusMessage.Text = "Expiry risk error: " + ex.Message; }
    }

    private void LoadAnalytics()
    {
        if (endDate.Value.Date < startDate.Value.Date) { statusMessage.Text = "End date must be on or after start date."; return; }
        try
        {
            DataTable trend = InventoryControlService.GetMovementTrend(RangeStart, RangeEnd);
            overviewTrendChart.Data = trend; overviewTrendChart.Invalidate(); analyticsTrendChart.Data = trend; analyticsTrendChart.Invalidate();
            string direction = movementDirection.SelectedItem?.ToString() == "Stock Out" ? "Out" : "In";
            string rank = ranking.SelectedItem?.ToString() ?? "Frequency";
            DataTable top = InventoryControlService.GetTopMovers(RangeStart, RangeEnd, direction, rank);
            moversGrid.DataSource = top; trendGrid.DataSource = trend;
            DataTable overviewTop = InventoryControlService.GetTopMovers(RangeStart, RangeEnd, "In", "Frequency");
            moversChart.Data = overviewTop; moversChart.Invalidate();
            SizeColumnsToContent(moversGrid);
            if (trendGrid.Columns["Movement Date"] is DataGridViewColumn dateColumn) dateColumn.DefaultCellStyle.Format = "dd MMM";
            if (moversGrid.Columns["Total Quantity"] is DataGridViewColumn quantityColumn) quantityColumn.DefaultCellStyle.Format = "N0";
        }
        catch (Exception ex) { statusMessage.Text = "Movement analytics error: " + ex.Message; }
    }

    private void SetQuickDates()
    {
        DateTime today = DateTime.Today;
        changingQuickPeriod = true;
        try
        {
            switch (quickPeriod.SelectedItem?.ToString())
            {
                case "Last 7 days": startDate.Value = today.AddDays(-6); endDate.Value = today; break;
                case "Last 30 days": startDate.Value = today.AddDays(-29); endDate.Value = today; break;
                case "This month": startDate.Value = new DateTime(today.Year, today.Month, 1); endDate.Value = today; break;
                case "This year": startDate.Value = new DateTime(today.Year, 1, 1); endDate.Value = today; break;
            }
        }
        finally { changingQuickPeriod = false; }
    }

    private sealed class TrendChartPanel : Panel
    {
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DataTable? Data { get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ShowSingleSeries { get; set; }
        public TrendChartPanel() { DoubleBuffered = true; BackColor = Color.White; MinimumSize = new Size(180, 130); }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e); Graphics g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle area = new(38, 30, Math.Max(20, Width - 55), Math.Max(36, Height - 66));
            using var gridPen = new Pen(Color.FromArgb(235, 240, 236)); using var inBrush = new SolidBrush(Color.FromArgb(36, 132, 82)); using var outBrush = new SolidBrush(Color.FromArgb(232, 164, 53)); using var textBrush = new SolidBrush(Muted); using var axisPen = new Pen(Border);
            if (Data == null || Data.Rows.Count == 0) { using var f = new Font("Segoe UI", 9); g.DrawString("No movement data in this date range", f, textBrush, new PointF(12, Height / 2f - 8)); return; }
            bool movement = Data.Columns.Contains("Movement Date");
            string primary = ShowSingleSeries ? (Data.Columns.Contains("Transactions") ? "Transactions" : Data.Columns.Contains("Total Quantity") ? "Total Quantity" : Data.Columns[0].ColumnName) : "Stock In";
            string secondary = "Stock Out";
            decimal max = 0;
            foreach (DataRow row in Data.Rows) { max = Math.Max(max, ToDecimal(row[primary])); if (!ShowSingleSeries && Data.Columns.Contains(secondary)) max = Math.Max(max, ToDecimal(row[secondary])); }
            if (max <= 0) max = 1;
            for (int i = 0; i <= 4; i++) { float y = area.Bottom - area.Height * i / 4f; g.DrawLine(gridPen, area.Left, y, area.Right, y); using var f = new Font("Segoe UI", 7.5f); g.DrawString((max * i / 4m).ToString("0.#"), f, textBrush, 0, y - 7); }
            g.DrawLine(axisPen, area.Left, area.Top, area.Left, area.Bottom); g.DrawLine(axisPen, area.Left, area.Bottom, area.Right, area.Bottom);
            int count = Data.Rows.Count; float slot = area.Width / (float)Math.Max(1, count); float barWidth = Math.Max(2, Math.Min(22, slot * (ShowSingleSeries ? .5f : .32f)));
            for (int i = 0; i < count; i++)
            {
                DataRow row = Data.Rows[i]; float x = area.Left + i * slot + slot / 2f;
                decimal a = ToDecimal(row[primary]); float ah = (float)(a / max * area.Height);
                float aX = x - (ShowSingleSeries ? barWidth / 2f : barWidth);
                float aTop = area.Bottom - ah;
                g.FillRectangle(inBrush, aX, aTop, barWidth, Math.Max(1, ah));
                DrawBarValue(g, a, aX + barWidth / 2f, aTop, area.Top, count);
                if (!ShowSingleSeries && Data.Columns.Contains(secondary))
                {
                    decimal b = ToDecimal(row[secondary]); float bh = (float)(b / max * area.Height);
                    float bTop = area.Bottom - bh;
                    g.FillRectangle(outBrush, x, bTop, barWidth, Math.Max(1, bh));
                    DrawBarValue(g, b, x + barWidth / 2f, bTop, area.Top, count);
                }
                if (count <= 12 || i == 0 || i == count - 1 || i % Math.Max(1, count / 6) == 0)
                {
                    string label;
                    if (movement)
                    {
                        label = Convert.ToDateTime(row["Movement Date"]).ToString(count > 15 ? "dd" : "dd MMM");
                    }
                    else if (Data.Columns.Contains("Medicine ID") && Data.Columns.Contains("Medicine"))
                    {
                        string id = Convert.ToString(row["Medicine ID"]) ?? "";
                        string medicine = Convert.ToString(row["Medicine"]) ?? "";
                        if (medicine.Length > 8) medicine = medicine[..7] + "…";
                        label = id + "\n" + medicine;
                    }
                    else label = Convert.ToString(row[Data.Columns.Contains("Medicine") ? "Medicine" : Data.Columns[0].ColumnName]) ?? "";
                    if (movement && label.Length > 11) label = label[..10] + "…";
                    using var f = new Font("Segoe UI", 7.2f); SizeF size = g.MeasureString(label, f); g.DrawString(label, f, textBrush, x - size.Width / 2, area.Bottom + 3);
                }
            }
            using var legendFont = new Font("Segoe UI", 8f); g.FillRectangle(inBrush, area.Left, Height - 18, 8, 8); g.DrawString(primary, legendFont, textBrush, area.Left + 12, Height - 22);
            if (!ShowSingleSeries) { g.FillRectangle(outBrush, area.Left + 105, Height - 18, 8, 8); g.DrawString(secondary, legendFont, textBrush, area.Left + 117, Height - 22); }
        }
        private static void DrawBarValue(Graphics g, decimal value, float centerX, float barTop, int chartTop, int barCount)
        {
            // Compact labels keep each bar's value visible even for longer date ranges.
            string label = value >= 1000000m ? (value / 1000000m).ToString("0.#", CultureInfo.InvariantCulture) + "m"
                : value >= 10000m ? (value / 1000m).ToString("0.#", CultureInfo.InvariantCulture) + "k"
                : value.ToString("0", CultureInfo.InvariantCulture);
            using var font = new Font("Segoe UI", barCount > 18 ? 6.1f : 7f, FontStyle.Bold);
            using var brush = new SolidBrush(Ink);
            SizeF size = g.MeasureString(label, font);
            float y = Math.Max(chartTop - 16, barTop - size.Height - 1);
            g.DrawString(label, font, brush, centerX - size.Width / 2f, y);
        }

        private static decimal ToDecimal(object value) => value == DBNull.Value ? 0 : decimal.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result) ? result : 0;
    }
}
