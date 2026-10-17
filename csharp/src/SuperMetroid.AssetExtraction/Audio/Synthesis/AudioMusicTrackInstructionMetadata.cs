using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>One phrase-flow operation; repeat uses Target while other operations do not.</summary>
public sealed record AudioMusicTrackInstructionMetadata(
    string Operation,
    int Value,
    int? Target)
{
    /// <summary>The operation's word operand, 0 through 65535.</summary>
    public int Value { get; init; } = Ensure.BetweenInclusive(0, ushort.MaxValue, Value);

    /// <summary>The repeat target's audio-RAM address, 0 through 65535, or null for other operations.</summary>
    public int? Target { get; init; } = Target is null ? null : Ensure.BetweenInclusive(0, ushort.MaxValue, Target.Value);
}
