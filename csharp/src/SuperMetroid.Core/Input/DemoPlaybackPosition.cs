namespace SuperMetroid.Core.Input;

/// <summary>
/// Where a demo's playback stands (#1600, #2553): the recording, the input it plays next, and the action it runs as its
/// inputs run out (a signalling recording's owner's, or none).
/// </summary>
public readonly partial record struct DemoPlaybackPosition
{
    /// <summary>A value of its parts; only its own operations make one.</summary>
    /// <param name="recording">The recording playing.</param>
    /// <param name="nextInput">The input it plays when the current one runs out; past the last, the recording ends.</param>
    /// <param name="ends">The action the recording runs as its inputs run out, before it ends as it says.</param>
    internal DemoPlaybackPosition(DemoRecording recording, int nextInput, Action ends)
    {
        Recording = recording;
        NextInput = nextInput;
        Ends = ends;
    }

    /// <summary>The recording playing.</summary>
    public DemoRecording Recording { get; }

    /// <summary>The action the recording runs as its inputs run out, before it ends as it says.</summary>
    public Action Ends { get; }

    /// <summary>The input it plays when the current one runs out; past the last, the recording ends.</summary>
    public int NextInput { get; init; }

    /// <summary>The start of <paramref name="recording"/>, which tells its owner nothing as it ends.</summary>
    public static DemoPlaybackPosition Start(DemoInputRecordingId recording) => new(DemoInputRecordings.Of(recording), 0, EndsUntold);

    /// <summary>The start of <paramref name="recording"/>, whose signal runs <paramref name="signalled"/> as its inputs run out.</summary>
    public static DemoPlaybackPosition Start(DemoSignallingRecordingId recording, Action signalled) =>
        new(DemoInputRecordings.Of(recording).Recording, 0, Ensure.NotNull(signalled));

    /// <summary>What a recording that does not signal runs as its inputs run out: nothing, its owner is told nothing.</summary>
    private static void EndsUntold()
    {
    }
}
