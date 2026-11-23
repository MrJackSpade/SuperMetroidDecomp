namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>Static SFX routing metadata used by the managed SPC driver.</summary>
public sealed partial record AudioSoundLibraryMetadata(
    int Library,
    IReadOnlyList<AudioSoundEffectMetadata> Effects);
