using System;
using System.Drawing;
using System.Windows.Forms;

namespace InventoryMonitoringSystem;

internal static class ExpiryDaysPrompt
{
    public static int? Show(IWin32Window? owner, int currentDays)
    {
        using var dialog = new Form
        {
            Text = "Custom Expiry Window",
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MinimizeBox = false,
            MaximizeBox = false,
            ShowInTaskbar = false,
            ClientSize = new Size(340, 142),
            BackColor = Color.White,
            Font = new Font("Segoe UI", 9F)
        };
        var label = new Label
        {
            Text = "Show medicines expiring within how many days?",
            AutoSize = false,
            Location = new Point(16, 14),
            Size = new Size(308, 24),
            ForeColor = Color.FromArgb(31, 55, 46)
        };
        var days = new NumericUpDown
        {
            Minimum = 1,
            Maximum = 3650,
            Value = Math.Clamp(currentDays, 1, 3650),
            Location = new Point(16, 43),
            Size = new Size(308, 28),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            ThousandsSeparator = true
        };
        var cancel = new Button
        {
            Text = "Cancel",
            DialogResult = DialogResult.Cancel,
            Location = new Point(158, 91),
            Size = new Size(78, 32),
            FlatStyle = FlatStyle.Flat
        };
        var apply = new Button
        {
            Text = "Apply",
            DialogResult = DialogResult.None,
            Location = new Point(244, 91),
            Size = new Size(80, 32),
            BackColor = Color.FromArgb(5, 150, 105),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        apply.Click += (_, _) => { dialog.DialogResult = DialogResult.OK; dialog.Close(); };
        dialog.Controls.AddRange(new Control[] { label, days, cancel, apply });
        dialog.AcceptButton = apply;
        dialog.CancelButton = cancel;
        return dialog.ShowDialog(owner) == DialogResult.OK ? decimal.ToInt32(days.Value) : null;
    }
}
