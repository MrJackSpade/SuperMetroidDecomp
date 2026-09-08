namespace SuperMetroid.Core.Input;

/// <summary>
/// Bridges event-driven controller producers to a simulation thread. A complete tap
/// between two simulation samples remains visible for one frame, followed by release.
/// Presentation callbacks never consume this state. Access is synchronized across threads.
/// </summary>
public sealed class FrameInputLatch
{
    private readonly object gate = new();
    private readonly Dictionary<int, SnesButton> sources = [];
    private SnesButton held;
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
