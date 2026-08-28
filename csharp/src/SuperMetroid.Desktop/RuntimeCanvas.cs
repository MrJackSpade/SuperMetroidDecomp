using System.Drawing.Drawing2D;

namespace SuperMetroid.Desktop;

/// <summary>Nearest-neighbor display for one 256x224 SNES frame at the 4:3 TV aspect.</summary>
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

        // The PPU framebuffer is 256x224, but the consumer display presents the complete
        // picture at 4:3. Treating those samples as square host pixels produces the visibly
        // too-tall, squeezed image reported during Ceres. A 224-line frame is therefore
        // about 299 display pixels wide. Keep the vertical scale integral so tile edges
        // remain stable, then apply only this horizontal correction with nearest-neighbor
        // sampling; emulation and captured PNGs remain the untouched native raster.
        const int DisplayAspectNumerator = 4;
        const int DisplayAspectDenominator = 3;
        int correctedUnitWidth = (frame.Height * DisplayAspectNumerator +
            DisplayAspectDenominator / 2) / DisplayAspectDenominator;
        int scale = Math.Max(1, Math.Min(
            ClientSize.Width / correctedUnitWidth,
            ClientSize.Height / frame.Height));
        int drawWidth = correctedUnitWidth * scale;
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
