using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>
/// Repeats the track from one of its own instructions, <paramref name="Target"/>, as the driver counts
/// <paramref name="Count"/>.
/// </summary>
/// <param name="Count">The repeat count, 1 through 255, neither of the fast-forward control words.</param>
/// <param name="Target">The place in the track's instruction list of the instruction it repeats from.</param>
public sealed partial record AudioMusicRepeat(int Count, int Target) : AudioMusicTrackInstructionMetadata
{
    /// <summary>The place in the track's instruction list of the instruction it repeats from, zero or more.</summary>
    public int Target { get; init; } = Ensure.AtLeastZero(Target);
}
