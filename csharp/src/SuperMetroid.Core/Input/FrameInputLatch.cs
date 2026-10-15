namespace SuperMetroid.Core.Input;

/// <summary>
/// Bridges event-driven controller producers to a simulation thread. A complete tap
/// between two simulation samples remains visible for one frame, followed by release.
/// Presentation callbacks never consume this state. Access is synchronized across threads.
/// </summary>
public sealed class FrameInputLatch
{
    /// <summary>Guards producer contributions and the simulation sample as one atomic state change.</summary>
    private readonly Lock gate = new();
    /// <summary>Independent physical key or axis contributions keyed by producer identity.</summary>
    private readonly Dictionary<int, SnesButton> sources = [];
    /// <summary>Combined buttons currently held by at least one producer.</summary>
    private SnesButton held;
    /// <summary>Edges accumulated since the previous simulation sample, including completed taps.</summary>
    private SnesButton pressed;

    /// <summary>Each physical key/axis producer owns an independent contribution.</summary>
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
    /// <returns>The buttons for one simulation update; does not remove any producer's held contribution.</returns>
    public SnesButton Sample()
    {
        lock (gate)
        {
            SnesButton result = held | pressed;
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
