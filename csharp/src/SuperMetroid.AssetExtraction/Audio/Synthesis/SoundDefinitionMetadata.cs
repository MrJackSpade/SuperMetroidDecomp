namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>The validated sound program and library metadata.</summary>
/// <param name="Programs">The programs.</param>
/// <param name="Libraries">The libraries.</param>
public readonly record struct SoundDefinitionMetadata(IReadOnlyList<AudioSoundProgramMetadata> Programs, IReadOnlyList<AudioSoundLibraryMetadata> Libraries);
