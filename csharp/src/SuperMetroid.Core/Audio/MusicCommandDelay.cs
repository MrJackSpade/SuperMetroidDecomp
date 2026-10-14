namespace SuperMetroid.Core.Audio;

/// <summary>
/// Effective countdown stored beside a music command after <c>QueueMusic_DelayedY</c>'s
/// minimum-eight-frame clamp.
/// </summary>
public readonly record struct MusicCommandDelay
{
    /// <summary>Stores the effective countdown after native minimum-delay handling.</summary>
    internal MusicCommandDelay(ushort frames) => Frames = frames;

    /// <summary>Effective native countdown frames.</summary>
    public ushort Frames { get; }

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
