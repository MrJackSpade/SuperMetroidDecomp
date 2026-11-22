namespace SuperMetroid.Core.Input;

/// <summary>How a demo recording ends once its last input has run out (#1600): a signalling recording is its own kind (<see cref="DemoSignallingRecording"/>).</summary>
public abstract partial record DemoRecordingEnd
{
    /// <summary>Allows only the endings below.</summary>
    private DemoRecordingEnd()
    {
    }

    /// <summary>The demo's input ends: the demo releases every button.</summary>
    public static DemoRecordingEnd Stops { get; } = new Stop();

    /// <summary>Holds the recording's last input, playing it again and again, until the owner switches the recording.</summary>
    public static DemoRecordingEnd HoldsLastInput { get; } = new HoldLastInput();

    /// <summary>The demo's input ends.</summary>
    public sealed partial record Stop : DemoRecordingEnd;

    /// <summary>The recording plays its last input again and again.</summary>
    public sealed partial record HoldLastInput : DemoRecordingEnd;
}
