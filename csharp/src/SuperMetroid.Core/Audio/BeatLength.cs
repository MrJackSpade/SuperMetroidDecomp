namespace SuperMetroid.Core.Audio;

/// <summary>
/// How long the music's beat lasts (#1725), exactly: <paramref name="Beats"/> beats take <paramref name="Frames"/> clip
/// frames, so one beat lasts <paramref name="Frames"/> / <paramref name="Beats"/> frames, a length that need not be a
/// whole number of frames. A length of no beats is a beat that never completes.
/// </summary>
/// <param name="Frames">Clip frames the beats take, at least one.</param>
/// <param name="Beats">Beats in those frames, zero or more.</param>
public sealed record BeatLength(long Frames, long Beats)
{
    /// <summary>The clip frames the beats take, validated to at least one.</summary>
    public long Frames { get; init; } = Frames >= 1
        ? Frames
        : throw new ArgumentOutOfRangeException(nameof(Frames), Frames, "A beat length spans at least one clip frame.");

    /// <summary>The beats in those frames, validated to zero or more.</summary>
    public long Beats { get; init; } = Ensure.AtLeastZero(Beats);
}
