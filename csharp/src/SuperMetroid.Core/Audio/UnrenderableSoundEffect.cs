namespace SuperMetroid.Core.Audio;

/// <summary>
/// A sound effect with no clip in one data set because its instruments key BRR sources that
/// the context's upload does not provide (#1305). Requesting it there fails loudly.
/// </summary>
/// <param name="Context">The music data set loaded, or null for only the shared sounds.</param>
/// <param name="Sound">The sound effect.</param>
/// <param name="MissingSources">Each instrument whose source the context lacks.</param>
public sealed record UnrenderableSoundEffect(
    MusicDataSet? Context,
    SoundEffectId Sound,
    IReadOnlyList<string> MissingSources);
