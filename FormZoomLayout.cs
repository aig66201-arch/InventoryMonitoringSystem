using System.Drawing;
using System.Collections.Generic;

namespace InventoryMonitoringSystem
{
    /// <summary>Uniformly zooms a fixed-size form design to fit its available client area.
    /// The original layout proportions are preserved instead of stretching docked panels.</summary>
    internal sealed class FormZoomLayout
    {
        private sealed class Item
        {
            public required Control Control { get; init; }
            public required Rectangle Bounds { get; init; }
            public required Font Font { get; init; }
        }

        private readonly Form _form;
        private readonly Size _designSize;
        private readonly List<Item> _items = new();

        public FormZoomLayout(Form form, Size designSize)
        {
            _form = form;
            _designSize = designSize;
            Capture(form.Controls);
            _form.Resize += (_, _) => Apply();
            Apply();
        }

        private void Capture(Control.ControlCollection controls)
        {
            foreach (Control control in controls)
            {
                _items.Add(new Item
                {
                    Control = control,
                    Bounds = control.Bounds,
                    Font = control.Font,
                });
                // Dock/anchor resizing stretches the original design, so the zoom manager owns layout.
                control.Dock = DockStyle.None;
                control.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                if (control is Label labelControl) labelControl.AutoSize = false;
                Capture(control.Controls);
            }
        }

        public void Apply()
        {
            if (_form.IsDisposed || _form.ClientSize.Width < 1 || _form.ClientSize.Height < 1) return;
            // Fill the entire client area using the larger scale ratio (cover behavior).
            // This removes side/top letterboxing; any excess is centered and clipped evenly.
            float scale = Math.Max((float)_form.ClientSize.Width / _designSize.Width,
                                   (float)_form.ClientSize.Height / _designSize.Height);
            if (scale <= 0) return;
            int canvasWidth = (int)Math.Round(_designSize.Width * scale);
            int canvasHeight = (int)Math.Round(_designSize.Height * scale);
            int offsetX = (_form.ClientSize.Width - canvasWidth) / 2;
            int offsetY = (_form.ClientSize.Height - canvasHeight) / 2;

            _form.SuspendLayout();
            foreach (Item item in _items) item.Control.SuspendLayout();
            try
            {
                foreach (Item item in _items)
                {
                    Rectangle b = item.Bounds;
                    int x = (int)Math.Round(b.X * scale);
                    int y = (int)Math.Round(b.Y * scale);
                    int w = Math.Max(1, (int)Math.Round(b.Width * scale));
                    int h = Math.Max(1, (int)Math.Round(b.Height * scale));
                    if (item.Control.Parent == _form) { x += offsetX; y += offsetY; }
                    item.Control.SetBounds(x, y, w, h);
                    float fontSize = Math.Max(5f, item.Font.Size * scale);
                    if (Math.Abs(item.Control.Font.Size - fontSize) > 0.05f)
                        item.Control.Font = new Font(item.Font.FontFamily, fontSize, item.Font.Style, item.Font.Unit);
                }
            }
            finally
            {
                foreach (Item item in _items) item.Control.ResumeLayout(false);
                _form.ResumeLayout(true);
            }
        }
    }
}
