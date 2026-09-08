using Android.Content;
using Android.Graphics;
using Android.Views;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Rendering;
using System.Diagnostics;

namespace SuperMetroid.Android;

/// <summary>
/// UI-thread presenter for software reference frames. Publishing replaces a single
/// pending copied pixel buffer; it never queues paint work or advances simulation.
/// Android may coalesce display invalidations without dropping game/audio steps.
/// </summary>
internal sealed class AndroidGameView : View
{
    private readonly Bitmap bitmap = Bitmap.CreateBitmap(FrontendFrame.Width, FrontendFrame.Height, Bitmap.Config.Argb8888!)!;
    private readonly Paint pixels = new() { FilterBitmap = false, AntiAlias = false };
    private readonly Paint text = new() { Color = Color.White, TextSize = 24, AntiAlias = true };
    private readonly int[] argb = new int[FrontendFrame.Width * FrontendFrame.Height];
    private readonly AndroidFrameMailbox frames = new(FrontendFrame.Width * FrontendFrame.Height, new AndroidFrameHandoffTrace());
    private readonly Action<Rgba32[]> uploadFrame;
    private string status = "Starting game...";
    private string roomIdentity = "No active room";
    private long paintCount;
    private long drawTicks;
    private long uploadTicks;
    private long replacedFrames;
    private bool presentationActive;

    public AndroidGameView(Context context) : base(context)
    {
        uploadFrame = UploadFrame;
        Focusable = true;
        FocusableInTouchMode = true;
        KeepScreenOn = true;
    }

    public long PaintCount => Interlocked.Read(ref paintCount);
    /// <summary>Cumulative UI-thread time; excludes deferred GPU execution after OnDraw returns.</summary>
    public long DrawTicks => Interlocked.Read(ref drawTicks);
    /// <summary>Cumulative CPU time converting pixels and uploading the small bitmap.</summary>
    public long UploadTicks => Interlocked.Read(ref uploadTicks);
    /// <summary>Published game frames replaced before the UI consumed them; never simulation steps skipped.</summary>
    public long ReplacedFrames => Interlocked.Read(ref replacedFrames);
    public void FlushFrameTrace(Action<string> write) => frames.FlushTrace(write);

    /// <summary>Updated per emulated frame; not delayed by the one-second FPS window.</summary>
    public void SetRoomIdentity(string identity) => Volatile.Write(ref roomIdentity, identity);

    /// <summary>
    /// Enables the display-paced redraw chain without coupling emulation to vsync.
    /// Already scheduled draws may finish after disabling, but do not rearm themselves.
    /// </summary>
    public void SetPresentationActive(bool value)
    {
        Volatile.Write(ref presentationActive, value);
        if (value) PostInvalidateOnAnimation();
    }

    public void Publish(Rgba32[] frame, string description)
    {
        Volatile.Write(ref status, description);
        if (frames.Publish(frame))
            Interlocked.Increment(ref replacedFrames);
        PostInvalidateOnAnimation();
    }

    public void ShowStatus(string description)
    {
        Volatile.Write(ref status, description);
        PostInvalidateOnAnimation();
    }

    protected override void OnDraw(Canvas canvas)
    {
        long drawStart = Stopwatch.GetTimestamp();
        base.OnDraw(canvas);
        canvas.DrawColor(Color.Black);
        if (frames.Consume(uploadFrame))
            Interlocked.Increment(ref paintCount);
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
        Interlocked.Add(ref drawTicks, Stopwatch.GetTimestamp() - drawStart);
        // Rearm from the display traversal, independently of producer arrival phase.
        // A paused/backgrounded session does not maintain a redraw loop.
        if (Volatile.Read(ref presentationActive)) PostInvalidateOnAnimation();
    }

    private void UploadFrame(Rgba32[] frame)
    {
        long uploadStart = Stopwatch.GetTimestamp();
        for (int i = 0; i < frame.Length; i++)
        {
            Rgba32 p = frame[i];
            argb[i] = unchecked((int)((uint)p.A << 24 | (uint)p.R << 16 | (uint)p.G << 8 | p.B));
        }
        bitmap.SetPixels(argb, 0, FrontendFrame.Width, 0, 0, FrontendFrame.Width, FrontendFrame.Height);
        Interlocked.Add(ref uploadTicks, Stopwatch.GetTimestamp() - uploadStart);
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
