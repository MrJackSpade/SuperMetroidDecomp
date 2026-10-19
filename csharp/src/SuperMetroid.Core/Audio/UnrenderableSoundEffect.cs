namespace SuperMetroid.Core.Audio;

/// <summary>
/// A sound effect with no clip in one bank context because its instruments key BRR sources that
/// the context's upload does not provide (#1305). Requesting it there fails loudly.
/// </summary>
/// <param name="Context">The music bank uploaded after the common upload, or null for the common upload alone.</param>
/// <param name="Library">The effect's library.</param>
/// <param name="Command">The effect's library-relative request number, 1 through 255.</param>
/// <param name="MissingSources">Each instrument whose source the context lacks.</param>
public sealed record UnrenderableSoundEffect(
    AudioBank? Context,
    SoundEffectLibrary Library,
    int Command,
    IReadOnlyList<string> MissingSources)
{
    /// <summary>The request number, validated to the range a sound-effect library port carries.</summary>
    public int Command { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, Command);
}
