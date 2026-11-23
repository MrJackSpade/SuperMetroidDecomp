using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>Stable identity, routing and voice-allocation class for a one-based SFX command.</summary>
public sealed partial record AudioSoundEffectMetadata(
    int Command,
    string Id,
    int StreamPointer,
    string Policy,
    IReadOnlyList<string> ChannelPrograms)
{
    /// <summary>The one-based sound-effect command, 0 through 255.</summary>
    public int Command { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, Command);

    /// <summary>The effect's audio-RAM stream address, 0 through 65535.</summary>
    public int StreamPointer { get; init; } = Ensure.BetweenInclusive(0, ushort.MaxValue, StreamPointer);

    /// <summary>The voice-allocation configuration byte, 0 through 255.</summary>
    public string Policy { get; init; } = string.IsNullOrEmpty(Policy) ? throw new ArgumentException("A sound effect names its policy.", nameof(Policy)) : Policy;
}
