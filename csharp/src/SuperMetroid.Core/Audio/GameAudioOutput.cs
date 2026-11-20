namespace SuperMetroid.Core.Audio;

/// <summary>
/// What a game session's audio produces for its host, chosen when the session is built: PCM mixed from the installed clips
/// (#1305), or silence for a host that plays no audio. Gameplay is identical either way: the sound acceptance clock and
/// its acknowledgements run whatever the output.
/// </summary>
public abstract record GameAudioOutput
{
    /// <summary>Closes the hierarchy to the outputs declared here.</summary>
    private GameAudioOutput()
    {
    }

    /// <summary>PCM mixed from <paramref name="Clips"/>, which the host copies after each update.</summary>
    /// <param name="Clips">The installed clips the session mixes.</param>
    public sealed record Mixed(ClipAudioLibrary Clips) : GameAudioOutput;

    /// <summary>No mixing: the PCM the host copies is silence.</summary>
    public sealed record Muted : GameAudioOutput;

    /// <summary>The output of a session that plays no audio.</summary>
    public static GameAudioOutput Silent { get; } = new Muted();

    /// <summary>The output that mixes <paramref name="clips"/>.</summary>
    /// <param name="clips">The installed clips the session mixes.</param>
    public static GameAudioOutput MixedFrom(ClipAudioLibrary clips) => new Mixed(Ensure.NotNull(clips));
}
