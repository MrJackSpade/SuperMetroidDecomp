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
    /// <summary>Stores bitmap.</summary>
    private readonly Bitmap bitmap = Bitmap.CreateBitmap(FrontendFrame.Width, FrontendFrame.Height, Bitmap.Config.Argb8888!)!;
    /// <summary>Stores pixels.</summary>
    private readonly Paint pixels = new() { FilterBitmap = false, AntiAlias = false };
    /// <summary>Stores text.</summary>
    private readonly Paint text = new() { Color = Color.White, TextSize = 24, AntiAlias = true };
    /// <summary>Stores argb.</summary>
    private readonly int[] argb = new int[FrontendFrame.Width * FrontendFrame.Height];
    /// <summary>Stores frames.</summary>
    private readonly AndroidFrameMailbox frames = new(FrontendFrame.Width * FrontendFrame.Height, new AndroidFrameHandoffTrace());
    /// <summary>Stores upload frame.</summary>
    private readonly Action<Rgba32[]> uploadFrame;
    /// <summary>Stores status.</summary>
    private string status = "Starting game...";
    /// <summary>Stores room identity.</summary>
    private string roomIdentity = "No active room";
    /// <summary>Stores paint count.</summary>
    private long paintCount;
    /// <summary>Stores draw ticks.</summary>
    private long drawTicks;
    /// <summary>Stores upload ticks.</summary>
    private long uploadTicks;
    /// <summary>Stores replaced frames.</summary>
    private long replacedFrames;
    /// <summary>Stores presentation active.</summary>
    private bool presentationActive;

    /// <summary>Initializes a new android game view instance.</summary>
    public AndroidGameView(Context context) : base(context)
    {
        uploadFrame = UploadFrame;
        Focusable = true;
        FocusableInTouchMode = true;
        KeepScreenOn = true;
    }

    /// <summary>Gets paint count.</summary>
    public long PaintCount => Interlocked.Read(ref paintCount);
    /// <summary>Cumulative UI-thread time; excludes deferred GPU execution after OnDraw returns.</summary>
    public long DrawTicks => Interlocked.Read(ref drawTicks);
    /// <summary>Cumulative CPU time converting pixels and uploading the small bitmap.</summary>
    public long UploadTicks => Interlocked.Read(ref uploadTicks);
    /// <summary>Published game frames replaced before the UI consumed them; never simulation steps skipped.</summary>
    public long ReplacedFrames => Interlocked.Read(ref replacedFrames);
    /// <summary>flush frame trace.</summary>
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

    /// <summary>publish.</summary>
    public void Publish(Rgba32[] frame, string description)
    {
        Volatile.Write(ref status, description);
        if (frames.Publish(frame))
            Interlocked.Increment(ref replacedFrames);
        PostInvalidateOnAnimation();
    }

    /// <summary>show status.</summary>
    public void ShowStatus(string description)
    {
        Volatile.Write(ref status, description);
        PostInvalidateOnAnimation();
    }

    /// <summary>on draw.</summary>
    protected override void OnDraw(Canvas canvas)
    {
        long drawStart = Stopwatch.GetTimestamp();
        base.OnDraw(canvas);
        canvas.DrawColor(Color.Black);
        if (frames.Consume(uploadFrame))
            Interlocked.Increment(ref paintCount);
        DisplayViewport viewport = DisplayViewport.IntegerPixels(DisplayViewport.ClientSize(Width, Height));
        int left = viewport.Offset.X, top = viewport.Offset.Y;
        int right = left + viewport.Size.X, bottom = top + viewport.Size.Y;
        if (viewport.Size.X != 0)
        {
            using var destination = new Rect(left, top, right, bottom);
            canvas.DrawBitmap(bitmap, null, destination, pixels);
        }
        // Diagnostic text is an overlay: changes cannot shift or resize the game picture.
        canvas.Save();
        canvas.ClipRect(left, top, right, bottom);
        canvas.DrawText(Volatile.Read(ref status), left + 8, top + 28, text);
        canvas.DrawText($"{Volatile.Read(ref roomIdentity)} | {Width}x{Height}; integer {viewport.Size.X / FrontendFrame.Width}x",
            left + 8, top + 56, text);
        canvas.Restore();
        Interlocked.Add(ref drawTicks, Stopwatch.GetTimestamp() - drawStart);
        // Rearm from the display traversal, independently of producer arrival phase.
        // A paused/backgrounded session does not maintain a redraw loop.
        if (Volatile.Read(ref presentationActive)) PostInvalidateOnAnimation();
    }

    /// <summary>upload frame.</summary>
    private void UploadFrame(Rgba32[] frame)
    {
        long uploadStart = Stopwatch.GetTimestamp();
        for (int i = 0; i < frame.Length; i++)
        {
            Rgba32 p = frame[i];
            argb[i] = AndroidPixelPacking.Argb(p);
        }
        bitmap.SetPixels(argb, 0, FrontendFrame.Width, 0, 0, FrontendFrame.Width, FrontendFrame.Height);
        Interlocked.Add(ref uploadTicks, Stopwatch.GetTimestamp() - uploadStart);
    }

    /// <summary>Releases resources held by this instance.</summary>
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
