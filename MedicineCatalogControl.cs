using System.Data;
using System.Reflection;
using System.Text;
using InventoryMonitoringSystem.Services;

namespace InventoryMonitoringSystem;

[System.ComponentModel.DesignerCategory("UserControl")]
public partial class MedicineCatalogControl : UserControl
{
    private DataTable medicineTable = null!;
    private bool isEditMode;

    public event EventHandler? DataChanged;

    private bool IsDesignTime => DesignMode ||
        System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime;

    public MedicineCatalogControl()
    {
        InitializeComponent();
        EnableDoubleBuffering(this);
        InitializeDataTable();

        if (!IsDesignTime)
        {
            ConfigureCustomDropdownMenus();
            LoadDatabaseData();
            ClearForm();
        }
    }

    protected override void OnCreateControl()
    {
        base.OnCreateControl();
        if (!IsDesignTime && medicineTable != null && medicineTable.Rows.Count == 0)
            LoadDatabaseData();
    }

    private static void EnableDoubleBuffering(Control control)
    {
        try
        {
            typeof(Control).InvokeMember("DoubleBuffered",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty,
                null, control, new object[] { true });
        }
        catch { }

        foreach (Control child in control.Controls)
            EnableDoubleBuffering(child);
    }

    private void InitializeDataTable()
    {
        medicineTable = new DataTable("Medicines");
        medicineTable.Columns.Add("Med_ID", typeof(string));
        medicineTable.Columns.Add("Generic_Name", typeof(string));
        medicineTable.Columns.Add("Brand_Name", typeof(string));
        medicineTable.Columns.Add("Dosage", typeof(string));
        medicineTable.Columns.Add("Dosage_Form", typeof(string));
        medicineTable.Columns.Add("Unit", typeof(string));
        medicineTable.Columns.Add("Reorder_Level", typeof(int));

        dgvMedicines.DataSource = medicineTable;
        ConfigureGridColumns();
    }

    private void ConfigureGridColumns()
    {
        SetHeader("Med_ID", "Med ID", 70);
        SetHeader("Generic_Name", "Generic Name");
        SetHeader("Brand_Name", "Brand Name");
        SetHeader("Dosage", "Dosage");
        SetHeader("Dosage_Form", "Form");
        SetHeader("Unit", "Unit");
        SetHeader("Reorder_Level", "Reorder Level", 85);
        UpdateRecordCount();
    }

    private void SetHeader(string name, string header, int? fillWeight = null)
    {
        if (dgvMedicines.Columns[name] is not DataGridViewColumn column) return;
        column.HeaderText = header;
        if (fillWeight.HasValue) column.FillWeight = fillWeight.Value;
    }

    private void LoadDatabaseData()
    {
        if (IsDesignTime) return;

        try
        {
            DataTable loaded = MedicineService.GetAllMedicines();
            medicineTable.Clear();
            foreach (DataRow row in loaded.Rows)
                medicineTable.ImportRow(row);

            LoadCustomDropdownValues();
            ConfigureGridColumns();
            UpdateRecordCount();
        }
        catch (Exception ex)
        {
            medicineTable.Clear();
            UpdateRecordCount();
            MessageBox.Show(
                "Unable to load the Medicine Catalog from InventoryMonitoringSystemDB.\n\n" +
                "The application attempted to verify and upgrade the Medicines table automatically.\n\n" +
                ex.Message,
                "Medicine Catalog",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    public void RefreshCatalog()
    {
        LoadDatabaseData();
        ClearForm();
    }


    private void ConfigureCustomDropdownMenus()
    {
        AttachAddOptionMenu(cmbDosageForm, "Add Dosage Form");
        AttachAddOptionMenu(cmbUnit, "Add Unit of Measurement");
    }

    private static void AttachAddOptionMenu(ComboBox combo, string title)
    {
        var menu = new ContextMenuStrip();
        menu.Items.Add("Add new option…", null, (_, _) => AddDropdownOption(combo, title));
        combo.ContextMenuStrip = menu;
    }

    private static void AddDropdownOption(ComboBox combo, string title)
    {
        using var dialog = new Form
        {
            Text = title, StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog,
            MinimizeBox = false, MaximizeBox = false, ShowInTaskbar = false, ClientSize = new Size(340, 125),
            BackColor = Color.White, Font = new Font("Segoe UI", 9F)
        };
        var label = new Label { Text = "Enter the new option name:", Location = new Point(16, 14), Size = new Size(308, 22), ForeColor = Color.FromArgb(6, 78, 59) };
        var input = new TextBox { Location = new Point(16, 40), Size = new Size(308, 28), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
        var cancel = new Button { Text = "Cancel", Location = new Point(158, 82), Size = new Size(78, 30), DialogResult = DialogResult.Cancel };
        var add = new Button { Text = "Add Option", Location = new Point(244, 82), Size = new Size(80, 30), BackColor = Color.FromArgb(5, 150, 105), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        add.Click += (_, _) => { if (string.IsNullOrWhiteSpace(input.Text)) { MessageBox.Show(dialog, "Enter an option name.", title, MessageBoxButtons.OK, MessageBoxIcon.Warning); input.Focus(); return; } dialog.DialogResult = DialogResult.OK; dialog.Close(); };
        dialog.Controls.AddRange(new Control[] { label, input, cancel, add }); dialog.AcceptButton = add; dialog.CancelButton = cancel;
        // MainForm uses TopMost while in kiosk mode. Give this owned modal dialog
        // the same z-order priority so it cannot appear behind the fullscreen window.
        Form? owner = combo.FindForm();
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
        if (result != DialogResult.OK) return;
        string value = input.Text.Trim();
        int existing = -1;
        for (int i = 0; i < combo.Items.Count; i++) if (string.Equals(Convert.ToString(combo.Items[i]), value, StringComparison.OrdinalIgnoreCase)) { existing = i; break; }
        if (existing >= 0) combo.SelectedIndex = existing;
        else { combo.Items.Add(value); combo.SelectedItem = value; }
    }

    private void LoadCustomDropdownValues()
    {
        foreach (DataRow row in medicineTable.Rows)
        {
            AddIfMissing(cmbDosageForm, Convert.ToString(row["Dosage_Form"]) ?? string.Empty);
            AddIfMissing(cmbUnit, Convert.ToString(row["Unit"]) ?? string.Empty);
        }
    }

    private static void AddIfMissing(ComboBox combo, string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return;
        for (int i = 0; i < combo.Items.Count; i++)
            if (string.Equals(Convert.ToString(combo.Items[i]), value, StringComparison.OrdinalIgnoreCase)) return;
        combo.Items.Add(value);
    }

    private void DgvMedicines_SelectionChanged(object? sender, EventArgs e)
    {
        if (dgvMedicines.SelectedRows.Count == 0 || dgvMedicines.SelectedRows[0].DataBoundItem is not DataRowView rowView)
            return;

        DataRow row = rowView.Row;
        txtMedId.Text = row["Med_ID"]?.ToString() ?? string.Empty;
        txtGenericName.Text = row["Generic_Name"]?.ToString() ?? string.Empty;
        txtBrandName.Text = row["Brand_Name"]?.ToString() ?? string.Empty;
        string storedDosage = row["Dosage"]?.ToString() ?? string.Empty;
        int firstNonDigit = 0;
        while (firstNonDigit < storedDosage.Length && char.IsDigit(storedDosage[firstNonDigit])) firstNonDigit++;
        txtDosage.Text = storedDosage[..firstNonDigit];
        string dosageUnit = storedDosage[firstNonDigit..].Trim();
        if (string.IsNullOrWhiteSpace(dosageUnit)) dosageUnit = "mg";
        AddIfMissing(cmbDosageUnit, dosageUnit);
        cmbDosageUnit.SelectedItem = dosageUnit;
        cmbDosageForm.SelectedItem = row["Dosage_Form"]?.ToString();
        cmbUnit.SelectedItem = row["Unit"]?.ToString();

        if (int.TryParse(row["Reorder_Level"]?.ToString(), out int reorder))
            numReorderLevel.Value = Math.Clamp(reorder, (int)numReorderLevel.Minimum, (int)numReorderLevel.Maximum);

        SetEditMode(true);
    }

    private void SetEditMode(bool edit)
    {
        isEditMode = edit;
        btnAddUpdate.Text = edit ? "✏️ Update Medicine" : "➕ Add New Medicine";
        btnAddUpdate.BackColor = edit ? Color.FromArgb(217, 119, 6) : Color.FromArgb(5, 150, 105);
        btnDelete.Enabled = edit;
    }

    private void BtnAddUpdate_Click(object? sender, EventArgs e)
    {
        if (!ValidateInputs()) return;

        try
        {
            string generic = txtGenericName.Text.Trim();
            string brand = txtBrandName.Text.Trim();
            string dosage = txtDosage.Text.Trim() + " " + Convert.ToString(cmbDosageUnit.SelectedItem);
            string form = cmbDosageForm.SelectedItem!.ToString()!;
            string unit = cmbUnit.SelectedItem!.ToString()!;
            int reorder = Convert.ToInt32(numReorderLevel.Value);

            if (isEditMode)
            {
                if (!MedicineService.UpdateMedicine(txtMedId.Text.Trim(), generic, brand, dosage, form, unit, reorder))
                {
                    MessageBox.Show("The selected medicine could not be updated.", "Update", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                MessageBox.Show("Medicine record updated successfully!", "Update Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                string newId = MedicineService.GenerateNextMedId();
                if (!MedicineService.AddMedicine(newId, generic, brand, dosage, form, unit, reorder))
                {
                    MessageBox.Show("The medicine could not be added.", "Add Medicine", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                MessageBox.Show("New medicine added to catalog successfully!", "Addition Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            LoadDatabaseData();
            ClearForm();
            DataChanged?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            MessageBox.Show("The medicine could not be saved.\n\n" + ex.Message,
                "Medicine Catalog", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnDelete_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtMedId.Text)) return;

        DialogResult result = MessageBox.Show(
            $"Are you sure you want to delete '{txtGenericName.Text}' from the medicine catalog?",
            "Confirm Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

        if (result != DialogResult.Yes) return;

        try
        {
            if (!MedicineService.DeleteMedicine(txtMedId.Text.Trim()))
            {
                MessageBox.Show("The medicine could not be deleted.", "Delete", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            MessageBox.Show("Medicine deleted successfully.", "Deleted", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadDatabaseData();
            ClearForm();
            DataChanged?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            MessageBox.Show("The medicine could not be deleted.\n\n" + ex.Message,
                "Medicine Catalog", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private bool ValidateInputs()
    {
        if (string.IsNullOrWhiteSpace(txtGenericName.Text))
        {
            MessageBox.Show("Please enter the Generic Name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            txtGenericName.Focus();
            return false;
        }
        if (string.IsNullOrWhiteSpace(txtDosage.Text) || !txtDosage.Text.All(char.IsDigit) || !int.TryParse(txtDosage.Text, out int dosageValue) || dosageValue <= 0)
        {
            MessageBox.Show("Dosage must contain numbers only and be greater than zero.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            txtDosage.Focus();
            return false;
        }
        if (cmbDosageUnit.SelectedItem == null)
        {
            MessageBox.Show("Please select a dosage unit.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            cmbDosageUnit.Focus();
            return false;
        }
        if (cmbDosageForm.SelectedItem == null)
        {
            MessageBox.Show("Please select a Dosage Form.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            cmbDosageForm.Focus();
            return false;
        }
        if (cmbUnit.SelectedItem == null)
        {
            MessageBox.Show("Please select a Unit of Measure.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            cmbUnit.Focus();
            return false;
        }
        return true;
    }

    private void ClearForm()
    {
        dgvMedicines.ClearSelection();
        txtMedId.Text = GetNextDisplayId();
        txtGenericName.Clear();
        txtBrandName.Clear();
        txtDosage.Clear();
        cmbDosageUnit.SelectedItem = "mg";
        cmbDosageForm.SelectedIndex = -1;
        cmbUnit.SelectedIndex = -1;
        numReorderLevel.Value = 50;
        SetEditMode(false);
    }

    private string GetNextDisplayId()
    {
        try { return MedicineService.GenerateNextMedId(); }
        catch { return "M-0001"; }
    }

    private void TxtSearch_TextChanged(object? sender, EventArgs e)
    {
        // DataView.RowFilter is an expression, not SQL: a trailing semicolon is invalid.
        // Escape apostrophes and LIKE metacharacters so ordinary search text never breaks it.
        var escaped = new StringBuilder();
        foreach (char ch in txtSearch.Text.Trim())
        {
            escaped.Append(ch switch
            {
                '\'' => "''",
                '[' => "[[]",
                ']' => "[]]",
                '%' => "[%]",
                '*' => "[*]",
                _ => ch.ToString()
            });
        }
        string filterText = escaped.ToString();
        medicineTable.DefaultView.RowFilter = string.IsNullOrEmpty(filterText)
            ? string.Empty
            : $"Generic_Name LIKE '%{filterText}%' OR Brand_Name LIKE '%{filterText}%' OR Med_ID LIKE '%{filterText}%'";
        UpdateRecordCount();
    }

    private void UpdateRecordCount()
    {
        if (lblRecordCount != null)
            lblRecordCount.Text = $"Total Items: {medicineTable?.DefaultView.Count ?? 0}";
    }

    private void btnClear_Click(object? sender, EventArgs e) => ClearForm();
}
