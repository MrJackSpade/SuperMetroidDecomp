namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>Static SFX routing metadata used by the managed SPC driver.</summary>
public sealed record AudioSoundLibraryMetadata(
    int Library,
    IReadOnlyList<AudioSoundEffectMetadata> Effects);
