namespace SuperMetroid.Core.Input;

/// <summary>Where a demo's playback stands (#1600): the recording, and the input it plays next.</summary>
public readonly record struct DemoPlaybackPosition
{
    /// <summary>A value of its parts; only its own operations make one.</summary>
    /// <param name="recording">The recording playing.</param>
    /// <param name="nextInput">The input it plays when the current one runs out; past the last, the recording ends.</param>
    private DemoPlaybackPosition(DemoInputRecordingId recording, int nextInput)
    {
        Recording = recording;
        NextInput = nextInput;
    }

    /// <summary>The recording playing.</summary>
    public DemoInputRecordingId Recording { get; init; }

    /// <summary>The input it plays when the current one runs out; past the last, the recording ends.</summary>
    public int NextInput { get; init; }

    /// <summary>The start of <paramref name="recording"/>.</summary>
    public static DemoPlaybackPosition Start(DemoInputRecordingId recording) => new(recording, 0);
}
