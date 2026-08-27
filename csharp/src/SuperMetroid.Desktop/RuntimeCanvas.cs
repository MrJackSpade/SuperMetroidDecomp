using System.Drawing.Drawing2D;

namespace SuperMetroid.Desktop;

/// <summary>Nearest-neighbor, integer-scaled display for one 256x224 SNES frame.</summary>
public sealed class RuntimeCanvas : Control
{
    private Bitmap? frame;

    public RuntimeCanvas()
    {
        BackColor = Color.Black;
        SetStyle(ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.UserPaint, true);
    }

    /// <summary>
    /// Claims the keys that WinForms otherwise reserves for dialog navigation.
    /// </summary>
    /// <remarks>
    /// <see cref="PlayableGameControl"/> listens to ordinary <c>KeyDown</c>/<c>KeyUp</c>
    /// events so the translated controller owns held/newly-pressed semantics. Without this
    /// override, the framework consumes the four arrows while moving focus and may consume
    /// Enter as a default-button key; those inputs would never reach the SNES controller word.
    /// </remarks>
    protected override bool IsInputKey(Keys keyData)
    {
        Keys keyCode = keyData & Keys.KeyCode;
        return keyCode is Keys.Left or Keys.Right or Keys.Up or Keys.Down or Keys.Enter ||
            base.IsInputKey(keyData);
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

    /// <summary>
    /// Clicking the picture is an explicit request to return keyboard control to gameplay.
    /// </summary>
    protected override void OnMouseDown(MouseEventArgs e)
    {
        Focus();
        base.OnMouseDown(e);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            frame?.Dispose();
        base.Dispose(disposing);
    }
}
