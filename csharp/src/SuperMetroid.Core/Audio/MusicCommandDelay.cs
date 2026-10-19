namespace SuperMetroid.Core.Audio;

/// <summary>
/// Effective countdown stored beside a music command after <c>QueueMusic_DelayedY</c>'s
/// minimum-eight-frame clamp.
/// </summary>
public readonly record struct MusicCommandDelay
{
    /// <summary>Stores the effective countdown after native minimum-delay handling.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="frames"/> is outside 0 through 65,535.</exception>
    internal MusicCommandDelay(int frames) => Frames = Ensure.BetweenInclusive(0, ushort.MaxValue, frames);

    /// <summary>
    /// Music-handler calls the command waits before dispatch, 0 through 65,535; zero marks an empty
    /// queue slot. The music timer starts at this count.
    /// </summary>
    public int Frames { get; }

    /// <summary>The fixed delay installed by <c>QueueMusic_Delayed8</c>.</summary>
    public static MusicCommandDelay EightFrames { get; } =
        new(AudioRomData.Queues.MinimumMusicDelayFrames);

    /// <summary>
    /// Converts a <c>QueueMusic_DelayedY</c> argument to the effective native countdown,
    /// clamping values below eight exactly as bank $80 does.
    /// </summary>
    public static MusicCommandDelay FromDelayedYArgument(ushort requestedFrames) =>
        new(Math.Max(requestedFrames, AudioRomData.Queues.MinimumMusicDelayFrames));

    /// <inheritdoc />
    public override string ToString() => $"{Frames} frames";
}
