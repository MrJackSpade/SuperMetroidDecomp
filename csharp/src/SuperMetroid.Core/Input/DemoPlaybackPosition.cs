namespace SuperMetroid.Core.Input;

/// <summary>Where a demo's playback stands (#1600): the recording, and the input it plays next.</summary>
/// <param name="Recording">The recording playing.</param>
/// <param name="NextInput">The input it plays when the current one runs out; past the last, the recording ends.</param>
public readonly record struct DemoPlaybackPosition(DemoInputRecordingId Recording, int NextInput)
{
    /// <summary>The start of <paramref name="recording"/>.</summary>
    public static DemoPlaybackPosition Start(DemoInputRecordingId recording) => new(recording, 0);
}
