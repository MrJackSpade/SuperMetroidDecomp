using Android.Content;
using Android.Graphics;
using Android.Views;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Android;

/// <summary>
/// UI-thread presenter for software reference frames. Publishing replaces a single
/// pending immutable pixel array; it never queues paint work or advances simulation.
/// Android may coalesce display invalidations without dropping game/audio steps.
/// </summary>
internal sealed class AndroidGameView : View
{
    private readonly Bitmap bitmap = Bitmap.CreateBitmap(FrontendFrame.Width, FrontendFrame.Height, Bitmap.Config.Argb8888!)!;
    private readonly Paint pixels = new() { FilterBitmap = false, AntiAlias = false };
    private readonly Paint text = new() { Color = Color.White, TextSize = 24, AntiAlias = true };
    private readonly int[] argb = new int[FrontendFrame.Width * FrontendFrame.Height];
    private Rgba32[]? pending;
    private string status = "Starting game...";
    private string roomIdentity = "No active room";
    private long paintCount;

    public AndroidGameView(Context context) : base(context)
    {
        Focusable = true;
        FocusableInTouchMode = true;
        KeepScreenOn = true;
    }

    public long PaintCount => Interlocked.Read(ref paintCount);

    /// <summary>Updated per emulated frame; not delayed by the one-second FPS window.</summary>
    public void SetRoomIdentity(string identity) => Volatile.Write(ref roomIdentity, identity);

    public void Publish(Rgba32[] frame, string description)
    {
        Volatile.Write(ref status, description);
        Interlocked.Exchange(ref pending, frame);
        PostInvalidateOnAnimation();
    }

    public void ShowStatus(string description)
    {
        Volatile.Write(ref status, description);
        PostInvalidateOnAnimation();
    }

    protected override void OnDraw(Canvas canvas)
    {
        base.OnDraw(canvas);
        canvas.DrawColor(Color.Black);
        Rgba32[]? frame = Interlocked.Exchange(ref pending, null);
        if (frame is not null)
        {
            if (frame.Length != argb.Length) throw new InvalidDataException("Unexpected Android framebuffer dimensions.");
            for (int i = 0; i < frame.Length; i++)
            {
                Rgba32 p = frame[i];
                argb[i] = unchecked((int)((uint)p.A << 24 | (uint)p.R << 16 | (uint)p.G << 8 | p.B));
            }
            bitmap.SetPixels(argb, 0, FrontendFrame.Width, 0, 0, FrontendFrame.Width, FrontendFrame.Height);
            Interlocked.Increment(ref paintCount);
        }
        DisplayViewport viewport = DisplayViewport.IntegerPixels(Width, Height);
        if (viewport.Width != 0)
        {
            using var destination = new Rect(viewport.Left, viewport.Top,
                viewport.Left + viewport.Width, viewport.Top + viewport.Height);
            canvas.DrawBitmap(bitmap, null, destination, pixels);
        }
        // Diagnostic text is an overlay: changes cannot shift or resize the game picture.
        canvas.Save();
        canvas.ClipRect(viewport.Left, viewport.Top, viewport.Left + viewport.Width, viewport.Top + viewport.Height);
        canvas.DrawText(Volatile.Read(ref status), viewport.Left + 8, viewport.Top + 28, text);
        canvas.DrawText($"{Volatile.Read(ref roomIdentity)} | {Width}x{Height}; integer {viewport.Width / FrontendFrame.Width}x",
            viewport.Left + 8, viewport.Top + 56, text);
        canvas.Restore();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            bitmap.Dispose();
            pixels.Dispose();
            text.Dispose();
        }
        base.Dispose(disposing);
    }
}
