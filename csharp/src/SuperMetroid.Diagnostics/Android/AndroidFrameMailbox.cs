using SuperMetroid.Core.Assets;

namespace SuperMetroid.Android;

/// <summary>
/// One fixed-size pending frame. Publish copies borrowed pixels before returning;
/// consumption holds the gate until the UI has uploaded them, so the worker can safely
/// reuse its raster buffer. Only pending presentation is replaced, never emulation.
/// </summary>
internal sealed class AndroidFrameMailbox(int pixelCount, AndroidFrameHandoffTrace? trace = null)
{
    /// <summary>Stores gate.</summary>
    private readonly Lock gate = new();
    /// <summary>Stores pending.</summary>
    private readonly Rgba32[] pending = new Rgba32[pixelCount];
    /// <summary>Stores available.</summary>
    private bool available;
    /// <summary>Stores sequence.</summary>
    private long sequence;

    /// <summary>Publishes.</summary>
    public bool Publish(ReadOnlySpan<Rgba32> frame)
    {
        if (frame.Length != pending.Length) throw new InvalidDataException("Unexpected Android framebuffer dimensions.");
        lock (gate)
        {
            bool replaced = available;
            frame.CopyTo(pending);
            available = true;
            trace?.Record(replaced ? 'R' : 'P', ++sequence, System.Diagnostics.Stopwatch.GetTimestamp());
            return replaced;
        }
    }

    /// <summary>The consumer must not retain or mutate the borrowed array.</summary>
    public bool Consume(Action<Rgba32[]> upload)
    {
        lock (gate)
        {
            if (!available)
            {
                trace?.Record('E', sequence, System.Diagnostics.Stopwatch.GetTimestamp());
                return false;
            }
            trace?.Record('C', sequence, System.Diagnostics.Stopwatch.GetTimestamp());
            upload(pending);
            trace?.Record('U', sequence, System.Diagnostics.Stopwatch.GetTimestamp());
            available = false;
            return true;
        }
    }

    /// <summary>Flush trace.</summary>
    public void FlushTrace(Action<string> write)
    {
        lock (gate) trace?.Flush(write);
    }
}
