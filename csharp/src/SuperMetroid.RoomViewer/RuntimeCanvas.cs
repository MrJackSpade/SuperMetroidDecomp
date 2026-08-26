using System.Drawing.Drawing2D;

namespace SuperMetroid.RoomViewer;

/// <summary>Nearest-neighbor, integer-scaled display for one 256x224 SNES frame.</summary>
internal sealed class RuntimeCanvas : Control
{
    private Bitmap? frame;

    public RuntimeCanvas()
    {
        BackColor = Color.Black;
        SetStyle(ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.UserPaint, true);
    }

    /// <summary>Takes ownership of a newly rendered frame and repaints the control.</summary>
    public void ReplaceFrame(Bitmap nextFrame)
    {
        ArgumentNullException.ThrowIfNull(nextFrame);
        Bitmap? previous = frame;
        frame = nextFrame;
        previous?.Dispose();
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if (frame is null)
            return;

        // Use the largest whole-number scale that fits. Integer scaling plus nearest
        // neighbor keeps every emulated PPU pixel a crisp rectangle with no invented edge
        // colors. Centering leaves a black overscan-like border on non-SNES aspect ratios.
        int scale = Math.Max(1, Math.Min(ClientSize.Width / frame.Width, ClientSize.Height / frame.Height));
        int drawWidth = frame.Width * scale;
        int drawHeight = frame.Height * scale;
        int drawX = (ClientSize.Width - drawWidth) / 2;
        int drawY = (ClientSize.Height - drawHeight) / 2;

        e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
        e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
        e.Graphics.CompositingMode = CompositingMode.SourceOver;
        e.Graphics.DrawImage(
            frame,
            new Rectangle(drawX, drawY, drawWidth, drawHeight),
            new Rectangle(0, 0, frame.Width, frame.Height),
            GraphicsUnit.Pixel);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            frame?.Dispose();
        base.Dispose(disposing);
    }
}
