#nullable enable
namespace InventoryMonitoringSystem;

partial class StockMovementControl
{
    private System.ComponentModel.IContainer? components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing) components?.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        Name = "StockMovementControl";
        Dock = System.Windows.Forms.DockStyle.Fill;
        Size = new System.Drawing.Size(1200, 760);
        BackColor = System.Drawing.Color.FromArgb(246, 250, 248);
    }
}
