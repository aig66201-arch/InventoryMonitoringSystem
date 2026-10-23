#nullable enable
namespace InventoryMonitoringSystem;

partial class MedicineCatalogControl
{
    private System.ComponentModel.IContainer? components = null;

    private TableLayoutPanel mainLayout = null!;
    private Panel pnlFormCard = null!;
    private Panel pnlFormBody = null!;
    private TableLayoutPanel formFieldsLayout = null!;
    private Panel pnlButtonContainer = null!;
    private Panel pnlSecondaryButtons = null!;
    private Panel pnlDataCard = null!;
    private Panel pnlSearchHeader = null!;

    private Label lblFormHeader = null!;
    private Label lblFormSub = null!;
    private Label lblMedId = null!;
    private Label lblGenericName = null!;
    private Label lblBrandName = null!;
    private Label lblDosage = null!;
    private Label lblDosageForm = null!;
    private Label lblUnit = null!;
    private Label lblReorderLevel = null!;
    private Label lblSearchIcon = null!;
    private Label lblRecordCount = null!;

    private TextBox txtMedId = null!;
    private TextBox txtGenericName = null!;
    private TextBox txtBrandName = null!;
    private TextBox txtDosage = null!;
    private ComboBox cmbDosageUnit = null!;
    private Panel pnlDosageInput = null!;
    private ComboBox cmbDosageForm = null!;
    private ComboBox cmbUnit = null!;
    private NumericUpDown numReorderLevel = null!;
    private TextBox txtSearch = null!;
    private DataGridView dgvMedicines = null!;
    private Button btnAddUpdate = null!;
    private Button btnDelete = null!;
    private Button btnClear = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing) components?.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        mainLayout = new TableLayoutPanel();
        pnlFormCard = new Panel();
        pnlFormBody = new Panel();
        formFieldsLayout = new TableLayoutPanel();
        pnlButtonContainer = new Panel();
        pnlSecondaryButtons = new Panel();
        pnlDataCard = new Panel();
        pnlSearchHeader = new Panel();
        lblFormHeader = new Label();
        lblFormSub = new Label();
        lblMedId = new Label();
        lblGenericName = new Label();
        lblBrandName = new Label();
        lblDosage = new Label();
        lblDosageForm = new Label();
        lblUnit = new Label();
        lblReorderLevel = new Label();
        lblSearchIcon = new Label();
        lblRecordCount = new Label();
        txtMedId = new TextBox { Name = "txtMedId", AccessibleName = "Medicine ID" };
        txtGenericName = new TextBox { Name = "txtGenericName", AccessibleName = "Generic Name" };
        txtBrandName = new TextBox { Name = "txtBrandName", AccessibleName = "Brand Name" };
        txtDosage = new TextBox { Name = "txtDosage", AccessibleName = "Dosage amount" };
        cmbDosageUnit = new ComboBox { Name = "cmbDosageUnit", AccessibleName = "Dosage unit" };
        pnlDosageInput = new Panel { Name = "pnlDosageInput", Dock = DockStyle.Top, Height = 30, Margin = new Padding(0, 0, 0, 12) };
        cmbDosageForm = new ComboBox { Name = "cmbDosageForm", AccessibleName = "Dosage Form" };
        cmbUnit = new ComboBox { Name = "cmbUnit", AccessibleName = "Unit of Measurement" };
        numReorderLevel = new NumericUpDown { Name = "numReorderLevel", AccessibleName = "Reorder Alert Level" };
        txtSearch = new TextBox { Name = "txtSearch", AccessibleName = "Search Medicine Catalog" };
        dgvMedicines = new DataGridView { Name = "dgvMedicines", AccessibleName = "Medicine Catalog Records" };
        btnAddUpdate = new Button();
        btnDelete = new Button();
        btnClear = new Button();

        SuspendLayout();
        mainLayout.SuspendLayout();
        pnlFormCard.SuspendLayout();
        pnlFormBody.SuspendLayout();
        formFieldsLayout.SuspendLayout();
        pnlButtonContainer.SuspendLayout();
        pnlSecondaryButtons.SuspendLayout();
        pnlDataCard.SuspendLayout();
        pnlSearchHeader.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)numReorderLevel).BeginInit();
        ((System.ComponentModel.ISupportInitialize)dgvMedicines).BeginInit();

        BackColor = Color.FromArgb(243, 244, 246);
        Font = new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point);
        Dock = DockStyle.Fill;
        Name = "MedicineCatalogControl";
        Size = new Size(1200, 700);

        mainLayout.Dock = DockStyle.Fill;
        mainLayout.ColumnCount = 2;
        mainLayout.RowCount = 1;
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 360F));
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        mainLayout.Margin = new Padding(0);
        mainLayout.Padding = new Padding(0);

        pnlFormCard.Dock = DockStyle.Fill;
        pnlFormCard.BackColor = Color.White;
        pnlFormCard.Margin = new Padding(0, 0, 16, 0);
        pnlFormCard.Padding = new Padding(20);

        lblFormHeader.Text = "Medicine Details";
        lblFormHeader.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        lblFormHeader.ForeColor = Color.FromArgb(6, 78, 59);
        lblFormHeader.Dock = DockStyle.Top;
        lblFormHeader.Height = 32;

        lblFormSub.Text = "Enter specifications to add or update item";
        lblFormSub.Font = new Font("Segoe UI", 8.5F);
        lblFormSub.ForeColor = Color.FromArgb(107, 114, 128);
        lblFormSub.Dock = DockStyle.Top;
        lblFormSub.Height = 24;

        pnlFormBody.Dock = DockStyle.Fill;
        // Keep the action buttons pinned below the form while allowing all fields to scroll.
        pnlFormBody.AutoScroll = true;
        pnlFormBody.AutoScrollMargin = new Size(0, 12);
        pnlFormBody.Padding = new Padding(0, 8, 8, 8);

        formFieldsLayout.Dock = DockStyle.Top;
        formFieldsLayout.AutoSize = true;
        formFieldsLayout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        formFieldsLayout.ColumnCount = 1;
        formFieldsLayout.RowCount = 14;
        formFieldsLayout.GrowStyle = TableLayoutPanelGrowStyle.AddRows;
        formFieldsLayout.Margin = new Padding(0);
        formFieldsLayout.Padding = new Padding(0, 0, 0, 8);
        formFieldsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        for (int row = 0; row < 14; row++)
            formFieldsLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        ConfigureLabel(lblMedId, "Medicine ID (Auto)");
        ConfigureTextBox(txtMedId, true);
        ConfigureLabel(lblGenericName, "Generic Name *");
        ConfigureTextBox(txtGenericName, false);
        ConfigureLabel(lblBrandName, "Brand Name");
        ConfigureTextBox(txtBrandName, false);
        ConfigureLabel(lblDosage, "Dosage (numbers only, e.g. 500) *");
        ConfigureTextBox(txtDosage, false);
        txtDosage.Dock = DockStyle.Fill;
        txtDosage.Margin = new Padding(0);
        txtDosage.MaxLength = 9;
        txtDosage.KeyPress += (_, e) =>
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) e.Handled = true;
        };
        txtDosage.TextChanged += (_, _) =>
        {
            string digits = new string(txtDosage.Text.Where(char.IsDigit).ToArray());
            if (txtDosage.Text != digits)
            {
                int caret = txtDosage.SelectionStart;
                txtDosage.Text = digits;
                txtDosage.SelectionStart = Math.Min(caret, txtDosage.Text.Length);
            }
        };
        ConfigureCombo(cmbDosageUnit, new[] { "mg", "g", "mcg", "mg/5mL", "mg/mL", "IU", "%" });
        cmbDosageUnit.Dock = DockStyle.Right;
        cmbDosageUnit.Width = 105;
        cmbDosageUnit.Margin = new Padding(8, 0, 0, 0);
        cmbDosageUnit.Height = 30;
        cmbDosageUnit.SelectedItem = "mg";
        pnlDosageInput.Controls.Add(txtDosage);
        pnlDosageInput.Controls.Add(cmbDosageUnit);
        ConfigureLabel(lblDosageForm, "Dosage Form *");
        ConfigureCombo(cmbDosageForm, new[] { "Tablet", "Capsule", "Syrup", "Suspension", "Ampule", "Vial", "Ointment", "Drops", "Inhaler" });
        ConfigureLabel(lblUnit, "Unit of Measure *");
        ConfigureCombo(cmbUnit, new[] { "Box", "Bottle", "Blister Pack", "Piece", "Strip", "Tube", "Vial" });
        ConfigureLabel(lblReorderLevel, "Reorder Alert Level *");
        numReorderLevel.Font = new Font("Segoe UI", 9.5F);
        numReorderLevel.Dock = DockStyle.Top;
        numReorderLevel.Height = 28;
        numReorderLevel.Margin = new Padding(0, 0, 0, 12);
        numReorderLevel.Minimum = 1;
        numReorderLevel.Maximum = 10000;
        numReorderLevel.Value = 50;

        Control[] formFields =
        {
            lblMedId, txtMedId, lblGenericName, txtGenericName, lblBrandName, txtBrandName,
            lblDosage, pnlDosageInput, lblDosageForm, cmbDosageForm, lblUnit, cmbUnit,
            lblReorderLevel, numReorderLevel
        };
        txtDosage.TabIndex = 0;
        cmbDosageUnit.TabIndex = 1;
        for (int i = 0; i < formFields.Length; i++)
        {
            formFields[i].TabIndex = i;
            formFieldsLayout.Controls.Add(formFields[i], 0, i);
        }
        pnlFormBody.Controls.Add(formFieldsLayout);

        pnlButtonContainer.Dock = DockStyle.Bottom;
        pnlButtonContainer.Height = 110;
        pnlButtonContainer.Padding = new Padding(0, 10, 0, 0);

        btnAddUpdate.Text = "➕ Add New Medicine";
        btnAddUpdate.Name = "btnAddUpdate";
        btnAddUpdate.AccessibleName = "Add or update medicine";
        btnAddUpdate.TabIndex = 14;
        btnAddUpdate.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnAddUpdate.BackColor = Color.FromArgb(5, 150, 105);
        btnAddUpdate.ForeColor = Color.White;
        btnAddUpdate.FlatStyle = FlatStyle.Flat;
        btnAddUpdate.FlatAppearance.BorderSize = 0;
        btnAddUpdate.Height = 42;
        btnAddUpdate.Dock = DockStyle.Top;
        btnAddUpdate.Cursor = Cursors.Hand;
        btnAddUpdate.Click += BtnAddUpdate_Click;

        pnlSecondaryButtons.Dock = DockStyle.Bottom;
        pnlSecondaryButtons.Height = 42;

        btnDelete.Text = "🗑️ Delete";
        btnDelete.Name = "btnDelete";
        btnDelete.AccessibleName = "Delete selected medicine";
        btnDelete.TabIndex = 15;
        btnDelete.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        btnDelete.BackColor = Color.FromArgb(239, 68, 68);
        btnDelete.ForeColor = Color.White;
        btnDelete.FlatStyle = FlatStyle.Flat;
        btnDelete.FlatAppearance.BorderSize = 0;
        btnDelete.Width = 150;
        btnDelete.Dock = DockStyle.Left;
        btnDelete.Cursor = Cursors.Hand;
        btnDelete.Enabled = false;
        btnDelete.Click += BtnDelete_Click;

        btnClear.Text = "🧹 Clear Form";
        btnClear.Name = "btnClear";
        btnClear.AccessibleName = "Clear medicine form";
        btnClear.TabIndex = 16;
        btnClear.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        btnClear.BackColor = Color.FromArgb(229, 231, 235);
        btnClear.ForeColor = Color.FromArgb(55, 65, 81);
        btnClear.FlatStyle = FlatStyle.Flat;
        btnClear.FlatAppearance.BorderSize = 0;
        btnClear.Width = 150;
        btnClear.Dock = DockStyle.Right;
        btnClear.Cursor = Cursors.Hand;
        btnClear.Click += btnClear_Click;

        pnlSecondaryButtons.Controls.Add(btnDelete);
        pnlSecondaryButtons.Controls.Add(btnClear);
        pnlButtonContainer.Controls.Add(btnAddUpdate);
        pnlButtonContainer.Controls.Add(pnlSecondaryButtons);

        pnlFormCard.Controls.Add(pnlFormBody);
        pnlFormCard.Controls.Add(pnlButtonContainer);
        pnlFormCard.Controls.Add(lblFormSub);
        pnlFormCard.Controls.Add(lblFormHeader);

        pnlDataCard.Dock = DockStyle.Fill;
        pnlDataCard.BackColor = Color.White;
        pnlDataCard.Padding = new Padding(20);

        pnlSearchHeader.Dock = DockStyle.Top;
        pnlSearchHeader.Height = 50;
        pnlSearchHeader.Padding = new Padding(0, 0, 0, 10);

        lblSearchIcon.Text = "🔍 Search Catalog:";
        lblSearchIcon.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        lblSearchIcon.ForeColor = Color.FromArgb(55, 65, 81);
        lblSearchIcon.AutoSize = true;
        lblSearchIcon.Location = new Point(0, 12);

        txtSearch.Font = new Font("Segoe UI", 10F);
        txtSearch.Location = new Point(135, 8);
        txtSearch.Width = 320;
        txtSearch.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        txtSearch.ForeColor = Color.FromArgb(17, 24, 39);
        txtSearch.TextChanged += TxtSearch_TextChanged;

        lblRecordCount.Text = "Total Items: 0";
        lblRecordCount.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblRecordCount.ForeColor = Color.FromArgb(5, 150, 105);
        lblRecordCount.AutoSize = true;
        lblRecordCount.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        lblRecordCount.Location = new Point(600, 12);

        pnlSearchHeader.Controls.Add(lblSearchIcon);
        pnlSearchHeader.Controls.Add(txtSearch);
        pnlSearchHeader.Controls.Add(lblRecordCount);

        dgvMedicines.Dock = DockStyle.Fill;
        dgvMedicines.BackgroundColor = Color.White;
        dgvMedicines.BorderStyle = BorderStyle.None;
        dgvMedicines.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        dgvMedicines.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        dgvMedicines.AllowUserToAddRows = false;
        dgvMedicines.AllowUserToDeleteRows = false;
        dgvMedicines.AllowUserToResizeRows = false;
        dgvMedicines.ReadOnly = true;
        dgvMedicines.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvMedicines.MultiSelect = false;
        dgvMedicines.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvMedicines.ScrollBars = ScrollBars.Vertical;
        dgvMedicines.RowHeadersVisible = false;
        dgvMedicines.EnableHeadersVisualStyles = false;
        dgvMedicines.RowTemplate.Height = 38;
        dgvMedicines.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(6, 78, 59);
        dgvMedicines.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        dgvMedicines.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        dgvMedicines.ColumnHeadersDefaultCellStyle.Padding = new Padding(10, 0, 0, 0);
        dgvMedicines.ColumnHeadersHeight = 42;
        dgvMedicines.DefaultCellStyle.SelectionBackColor = Color.FromArgb(167, 243, 208);
        dgvMedicines.DefaultCellStyle.SelectionForeColor = Color.FromArgb(6, 78, 59);
        dgvMedicines.DefaultCellStyle.Font = new Font("Segoe UI", 9.5F);
        dgvMedicines.DefaultCellStyle.ForeColor = Color.FromArgb(31, 41, 55);
        dgvMedicines.DefaultCellStyle.Padding = new Padding(10, 0, 0, 0);
        dgvMedicines.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(240, 253, 244);
        dgvMedicines.SelectionChanged += DgvMedicines_SelectionChanged;

        pnlDataCard.Controls.Add(dgvMedicines);
        pnlDataCard.Controls.Add(pnlSearchHeader);
        mainLayout.Controls.Add(pnlFormCard, 0, 0);
        mainLayout.Controls.Add(pnlDataCard, 1, 0);
        Controls.Add(mainLayout);

        ((System.ComponentModel.ISupportInitialize)numReorderLevel).EndInit();
        ((System.ComponentModel.ISupportInitialize)dgvMedicines).EndInit();
        pnlSearchHeader.ResumeLayout(false);
        pnlSearchHeader.PerformLayout();
        pnlDataCard.ResumeLayout(false);
        pnlSecondaryButtons.ResumeLayout(false);
        pnlButtonContainer.ResumeLayout(false);
        formFieldsLayout.ResumeLayout(false);
        formFieldsLayout.PerformLayout();
        pnlFormBody.ResumeLayout(false);
        pnlFormCard.ResumeLayout(false);
        pnlFormCard.PerformLayout();
        mainLayout.ResumeLayout(false);
        ResumeLayout(false);
    }

    private static void ConfigureLabel(Label label, string text)
    {
        label.Text = text;
        label.Name = "lbl" + text.Replace(" ", string.Empty).Replace("*", string.Empty).Replace("(", string.Empty).Replace(")", string.Empty).Replace("/", string.Empty).Replace(":", string.Empty).Replace(".", string.Empty);
        label.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        label.ForeColor = Color.FromArgb(55, 65, 81);
        label.AutoSize = true;
        label.Dock = DockStyle.Top;
        label.Margin = new Padding(0, 4, 0, 4);
        label.TabStop = false;
    }

    private static void ConfigureTextBox(TextBox box, bool readOnly)
    {
        box.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);
        box.Dock = DockStyle.Top;
        box.Height = 28;
        box.Margin = new Padding(0, 0, 0, 12);
        box.ReadOnly = readOnly;
        box.BackColor = readOnly ? Color.FromArgb(243, 244, 246) : Color.White;
        box.AccessibleName = box.Name;
    }

    private static void ConfigureCombo(ComboBox combo, string[] items)
    {
        combo.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);
        combo.Dock = DockStyle.Top;
        combo.Height = 28;
        combo.Margin = new Padding(0, 0, 0, 12);
        combo.DropDownStyle = ComboBoxStyle.DropDownList;
        combo.Items.AddRange(items);
        combo.AccessibleName = combo.Name;
    }
}
