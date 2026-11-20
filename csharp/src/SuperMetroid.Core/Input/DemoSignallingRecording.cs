namespace SuperMetroid.Core.Input;

/// <summary>
/// A recording that signals its owner as it ends (#1300, #2553): its inputs play as any recording's do, then its signal
/// runs the routine its owner loaded it with, then its input ends.
/// </summary>
public sealed record DemoSignallingRecording
{
    /// <summary>Creates a signalling recording; every input lasts at least one update.</summary>
    /// <param name="inputs">The inputs, in the order they play.</param>
    /// <param name="signal">What the recording signals as it ends.</param>
    /// <exception cref="ArgumentException">An input lasts no update.</exception>
    public DemoSignallingRecording(IReadOnlyList<DemoInput> inputs, DemoInputCue signal)
    {
        Recording = new DemoRecording(inputs, DemoRecordingEnd.Stops);
        Signal = signal;
    }

    /// <summary>The inputs, which stop once played: the signal comes just before they stop.</summary>
    public DemoRecording Recording { get; }

    /// <summary>What the recording signals as it ends: the routine its owner runs.</summary>
    public DemoInputCue Signal { get; }
}
