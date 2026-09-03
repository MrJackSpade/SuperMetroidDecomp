using System.Drawing.Drawing2D;
using System.ComponentModel;

namespace SuperMetroid.RoomViewer;

/// <summary>Pixel-preserving canvas whose physical size follows a selectable zoom factor.</summary>
internal sealed class RoomCanvas : Control
{
    private readonly Bitmap roomImage;
    private float zoom = 1f;

    public RoomCanvas(Bitmap roomImage)
    {
        this.roomImage = roomImage;

        // Painting a multi-megapixel room can otherwise expose partially erased frames while
        // scrolling. These flags render to an intermediate buffer and replace the frame once.
        SetStyle(ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.UserPaint, true);
        ResizeForZoom();
    }

    // This is live viewer state, not a designer-authored property. Without these attributes,
    // WinForms may try to persist it into generated InitializeComponent code.
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public float Zoom
    {
        get => zoom;
        set
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);
            zoom = value;
            ResizeForZoom();
            Invalidate();
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        // Nearest-neighbor interpolation is essential here: smoothing would invent colors
        // that never existed in CGRAM and obscure single-pixel tile/flip errors.
        e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
        e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
        e.Graphics.CompositingMode = CompositingMode.SourceCopy;
        e.Graphics.DrawImage(
            roomImage,
            new Rectangle(0, 0, ClientSize.Width, ClientSize.Height),
            new Rectangle(0, 0, roomImage.Width, roomImage.Height),
            GraphicsUnit.Pixel);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            roomImage.Dispose();
        base.Dispose(disposing);
    }

    private void ResizeForZoom()
    {
        Size = new Size(
            Math.Max(1, (int)Math.Round(roomImage.Width * zoom)),
            Math.Max(1, (int)Math.Round(roomImage.Height * zoom)));
    }
}
