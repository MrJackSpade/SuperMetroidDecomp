namespace SuperMetroid.Core.Input;

/// <summary>
/// Bridges event-driven controller producers to a simulation thread. A complete tap
/// between two simulation samples remains visible for one frame, followed by release.
/// Presentation callbacks never consume this state. Access is synchronized across threads.
/// </summary>
public sealed class FrameInputLatch
{
    /// <summary>Protects producer updates and simulation sampling from racing each other.</summary>
    private readonly object gate = new();

    /// <summary>Current button contribution keyed by the independent host producer.</summary>
    private readonly Dictionary<int, SnesButton> sources = [];

    /// <summary>Union of all buttons currently contributed by producers.</summary>
    private SnesButton held;

    /// <summary>Rising edges retained until the next simulation sample.</summary>
    private SnesButton pressed;

    /// <summary>Each physical key/axis producer owns an independent contribution.</summary>
    /// <param name="source">Stable identity for the producer whose contribution is being replaced.</param>
    /// <param name="value">Buttons currently contributed; <see cref="SnesButton.None"/> removes that producer.</param>
    public void Set(int source, SnesButton value)
    {
        lock (gate)
        {
            if (value == SnesButton.None) sources.Remove(source);
            else sources[source] = value;
            SnesButton next = SnesButton.None;
            foreach (SnesButton input in sources.Values) next |= input;
            pressed |= next & ~held;
            held = next;
        }
    }

    /// <summary>Atomically samples the combined held buttons and presses accumulated since the last simulation sample, then consumes only the pending presses; taps completed between samples are visible once while sustained holds remain visible.</summary>
    /// <returns>Native sixteen-bit SNES button mask for one simulation update; does not remove any producer's held contribution.</returns>
    public ushort Sample()
    {
        lock (gate)
        {
            ushort result = (ushort)(held | pressed);
            pressed = SnesButton.None;
            return result;
        }
    }

    /// <summary>Focus loss cancels pending taps as well as held physical contributions.</summary>
    public void Clear()
    {
        lock (gate)
        {
            sources.Clear();
            held = pressed = SnesButton.None;
        }
    }
}
