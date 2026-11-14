namespace SuperMetroid.Core.Input;

/// <summary>A demo's recorded input (#1300, #1600): its inputs in the order they play, and how it ends.</summary>
public sealed record DemoRecording
{
    /// <summary>Creates a recording; a loop must name one of its inputs.</summary>
    /// <exception cref="ArgumentException">The loop names no input of the recording.</exception>
    public DemoRecording(IReadOnlyList<DemoInput> inputs, DemoRecordingEnd end)
    {
        Inputs = Ensure.NotNull(inputs);
        End = Ensure.NotNull(end);
        if (end is DemoRecordingEnd.Loop loop && loop.Input >= inputs.Count)
            throw new ArgumentException($"A recording of {inputs.Count} inputs cannot loop from input {loop.Input}.", nameof(end));
    }

    /// <summary>The inputs, in the order they play.</summary>
    public IReadOnlyList<DemoInput> Inputs { get; }

    /// <summary>How the recording ends once its last input has run out.</summary>
    public DemoRecordingEnd End { get; }
}
