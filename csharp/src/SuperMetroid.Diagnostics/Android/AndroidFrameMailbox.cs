using SuperMetroid.Core.Assets;

namespace SuperMetroid.Android;

/// <summary>
/// One fixed-size pending frame. Publish copies borrowed pixels before returning;
/// consumption holds the gate until the UI has uploaded them, so the worker can safely
/// reuse its raster buffer. Only pending presentation is replaced, never emulation.
/// </summary>
/// <param name="pixelCount">Number of RGBA pixels in every frame accepted by this mailbox.</param>
/// <param name="trace">Optional trace sink for publication and UI-consumption events.</param>
internal sealed class AndroidFrameMailbox(int pixelCount, AndroidFrameHandoffTrace? trace = null)
{
    /// <summary>Serializes producer and consumer access, including the duration of upload.</summary>
    private readonly object gate = new();
    /// <summary>Storage for the single pending frame, reused after the consumer finishes.</summary>
    private readonly Rgba32[] pending = new Rgba32[pixelCount];
    /// <summary>Whether <see cref="pending"/> contains a frame not yet consumed.</summary>
    private bool available;
    /// <summary>Monotonic sequence number assigned to published frames.</summary>
    private long sequence;

    /// <summary>Copies a frame into the pending slot, replacing any older unconsumed presentation.</summary>
    /// <param name="frame">Borrowed pixels copied before this method returns.</param>
    /// <returns><see langword="true"/> when an existing pending frame was replaced.</returns>
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
    /// <param name="upload">Synchronous UI upload performed while the frame buffer is protected.</param>
    /// <returns><see langword="true"/> when a frame was available and uploaded.</returns>
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

    /// <summary>Flushes pending handoff trace records while holding the mailbox gate.</summary>
    /// <param name="write">Destination for serialized trace lines.</param>
    public void FlushTrace(Action<string> write)
    {
        lock (gate) trace?.Flush(write);
    }
}
