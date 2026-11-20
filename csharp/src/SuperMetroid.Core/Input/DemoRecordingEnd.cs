namespace SuperMetroid.Core.Input;

/// <summary>How a demo recording ends once its last input has run out (#1600): a signalling recording is its own kind (<see cref="DemoSignallingRecording"/>).</summary>
public abstract record DemoRecordingEnd
{
    /// <summary>Allows only the endings below.</summary>
    private DemoRecordingEnd()
    {
    }

    /// <summary>The demo's input ends: the demo releases every button.</summary>
    public static DemoRecordingEnd Stops { get; } = new Stop();

    /// <summary>Plays on from input <paramref name="input"/> again, until the owner switches the recording.</summary>
    public static DemoRecordingEnd LoopsFrom(int input) => new Loop(Ensure.AtLeastZero(input));

    /// <summary>The demo's input ends.</summary>
    public sealed record Stop : DemoRecordingEnd;

    /// <summary>The recording plays on from <paramref name="Input"/>.</summary>
    /// <param name="Input">The input it plays on from.</param>
    public sealed record Loop(int Input) : DemoRecordingEnd;
}
