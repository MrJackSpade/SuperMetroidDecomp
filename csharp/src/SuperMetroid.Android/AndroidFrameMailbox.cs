using SuperMetroid.Core.Assets;

namespace SuperMetroid.Android;

/// <summary>
/// One fixed-size pending frame. Publish copies borrowed pixels before returning;
/// consumption holds the gate until the UI has uploaded them, so the worker can safely
/// reuse its raster buffer. Only pending presentation is replaced, never emulation.
/// </summary>
internal sealed class AndroidFrameMailbox(int pixelCount)
{
    private readonly object gate = new();
    private readonly Rgba32[] pending = new Rgba32[pixelCount];
    private bool available;

    public bool Publish(ReadOnlySpan<Rgba32> frame)
    {
        if (frame.Length != pending.Length) throw new InvalidDataException("Unexpected Android framebuffer dimensions.");
        lock (gate)
        {
            bool replaced = available;
            frame.CopyTo(pending);
            available = true;
            return replaced;
        }
    }

    /// <summary>The consumer must not retain or mutate the borrowed array.</summary>
    public bool Consume(Action<Rgba32[]> upload)
    {
        lock (gate)
        {
            if (!available) return false;
            upload(pending);
            available = false;
            return true;
        }
    }
}
