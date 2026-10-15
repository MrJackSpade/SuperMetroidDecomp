namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>Stable identity, routing and voice-allocation class for a one-based SFX command.</summary>
public sealed record AudioSoundEffectMetadata(
    byte Command,
    string Id,
    ushort StreamPointer,
    byte Configuration,
    IReadOnlyList<string> ChannelPrograms);
