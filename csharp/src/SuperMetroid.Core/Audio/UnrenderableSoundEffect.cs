namespace SuperMetroid.Core.Audio;

/// <summary>
/// A sound effect with no clip in one clip set because its instruments play samples the set
/// does not hold (#1305). Requesting it there fails loudly.
/// </summary>
/// <param name="Context">The music data set naming the clip set, or null for only the shared sounds.</param>
/// <param name="Sound">The sound effect.</param>
/// <param name="MissingSources">Each instrument whose sample the clip set lacks.</param>
public sealed record UnrenderableSoundEffect(
    MusicDataSet? Context,
    SoundEffectId Sound,
    IReadOnlyList<string> MissingSources);
