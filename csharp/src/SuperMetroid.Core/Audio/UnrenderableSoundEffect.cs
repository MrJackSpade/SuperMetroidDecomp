namespace SuperMetroid.Core.Audio;

/// <summary>
/// A sound effect with no clip in one bank context because its instruments key BRR sources that
/// the context's upload does not provide (#1305). Requesting it there fails loudly.
/// </summary>
/// <param name="Context">The music bank uploaded after the common upload, or null for the common upload alone.</param>
/// <param name="Library">The effect's library.</param>
/// <param name="Command">The effect's request byte.</param>
/// <param name="MissingSources">Each instrument whose source the context lacks.</param>
public sealed record UnrenderableSoundEffect(
    AudioBank? Context,
    SoundEffectLibrary Library,
    byte Command,
    IReadOnlyList<string> MissingSources);
