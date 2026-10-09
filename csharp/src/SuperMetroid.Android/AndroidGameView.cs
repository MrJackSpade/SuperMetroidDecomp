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
    /// <summary>Fixed-size ARGB bitmap that holds the latest consumed frontend frame for Canvas drawing.</summary>
    private readonly Bitmap bitmap = Bitmap.CreateBitmap(FrontendFrame.Width, FrontendFrame.Height, Bitmap.Config.Argb8888!)!;
    /// <summary>Nearest-neighbor paint used to keep the integer-scaled game pixels crisp.</summary>
    private readonly Paint pixels = new() { FilterBitmap = false, AntiAlias = false };
    /// <summary>Paint used for the status and viewport diagnostic overlay.</summary>
    private readonly Paint text = new() { Color = Color.White, TextSize = 24, AntiAlias = true };
    /// <summary>Reusable packed pixel buffer converted from RGBA frames before bitmap upload.</summary>
    private readonly int[] argb = new int[FrontendFrame.Width * FrontendFrame.Height];
    /// <summary>Single-pending-frame mailbox that reports replaced publications without advancing the game.</summary>
    private readonly AndroidFrameMailbox frames = new(FrontendFrame.Width * FrontendFrame.Height, new AndroidFrameHandoffTrace());
    /// <summary>Mailbox consumer callback that converts a frame and copies it into the bitmap.</summary>
    private readonly Action<Rgba32[]> uploadFrame;
    /// <summary>Most recently published user-facing status string drawn over the frame.</summary>
    private string status = "Starting game...";
    /// <summary>Room label shown in the diagnostic overlay.</summary>
    private string roomIdentity = "No active room";
    /// <summary>Count of mailbox frames consumed and uploaded by the view.</summary>
    private long paintCount;
    /// <summary>Accumulated elapsed ticks spent in the UI draw callback.</summary>
    private long drawTicks;
    /// <summary>Accumulated elapsed ticks spent converting and uploading frame pixels.</summary>
    private long uploadTicks;
    /// <summary>Number of pending published frames replaced before UI consumption.</summary>
    private long replacedFrames;
    /// <summary>Whether completed display traversals should schedule another redraw.</summary>
    private bool presentationActive;

    /// <summary>Creates the Android view and configures it to receive focus while the game is active.</summary>
    /// <param name="context">Android context used to construct the native view.</param>
    public AndroidGameView(Context context) : base(context)
    {
        uploadFrame = UploadFrame;
        Focusable = true;
        FocusableInTouchMode = true;
        KeepScreenOn = true;
    }

    /// <summary>Number of published frames consumed and uploaded by the UI thread.</summary>
    public long PaintCount => Interlocked.Read(ref paintCount);
    /// <summary>Cumulative UI-thread time; excludes deferred GPU execution after OnDraw returns.</summary>
    public long DrawTicks => Interlocked.Read(ref drawTicks);
    /// <summary>Cumulative CPU time converting pixels and uploading the small bitmap.</summary>
    public long UploadTicks => Interlocked.Read(ref uploadTicks);
    /// <summary>Published game frames replaced before the UI consumed them; never simulation steps skipped.</summary>
    public long ReplacedFrames => Interlocked.Read(ref replacedFrames);
    /// <summary>Writes accumulated mailbox handoff diagnostics through the supplied trace sink.</summary>
    /// <param name="write">Callback that receives each formatted diagnostic entry.</param>
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

    /// <summary>Publishes the latest reference frame and status, requesting a display-paced redraw.</summary>
    /// <param name="frame">Frontend-sized pixel buffer to hand off to the UI-thread mailbox.</param>
    /// <param name="description">Status text shown in the overlay with this publication.</param>
    public void Publish(Rgba32[] frame, string description)
    {
        Volatile.Write(ref status, description);
        if (frames.Publish(frame))
            Interlocked.Increment(ref replacedFrames);
        PostInvalidateOnAnimation();
    }

    /// <summary>Updates the overlay status without publishing a new game image.</summary>
    /// <param name="description">Text to show over the current bitmap.</param>
    public void ShowStatus(string description)
    {
        Volatile.Write(ref status, description);
        PostInvalidateOnAnimation();
    }

    /// <summary>Draws the latest consumed frame at integer scale with status and viewport diagnostics overlaid.</summary>
    /// <param name="canvas">Android drawing surface supplied for this view traversal.</param>
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

    /// <summary>Converts the consumed RGBA frame to packed ARGB pixels and updates the backing bitmap.</summary>
    /// <param name="frame">Frontend-sized source pixels supplied by the mailbox.</param>
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

    /// <summary>Releases the bitmap and paints when disposing, then delegates native view cleanup to the base class.</summary>
    /// <param name="disposing">True when managed and native resources should be released.</param>
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
