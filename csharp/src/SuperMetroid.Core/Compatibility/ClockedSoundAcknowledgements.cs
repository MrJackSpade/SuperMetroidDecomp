namespace SuperMetroid.Core.Compatibility;

/// <summary>The game's own timing: each update reads the acknowledgements of the session's <see cref="SoundAcceptanceClock{TLibrary, TSound}"/>.</summary>
/// <typeparam name="TLibrary">A sound library.</typeparam>
/// <typeparam name="TSound">The sound a library is asked for.</typeparam>
[CompatibilityService("each update reads the session clock's acknowledgements")]
public sealed class ClockedSoundAcknowledgements<TLibrary, TSound> : ISoundAcknowledgementTiming<TLibrary, TSound>
    where TLibrary : struct, Enum
    where TSound : struct, IEquatable<TSound>
{
    /// <summary>The shared, stateless instance every session uses.</summary>
    public static ClockedSoundAcknowledgements<TLibrary, TSound> Instance { get; } = new();

    /// <summary>Restricts the policy to its shared instance.</summary>
    private ClockedSoundAcknowledgements() { }

    /// <summary>Returns the clock's acknowledgements unchanged.</summary>
    /// <param name="clocked">The acknowledgements the session's clock reported after the previous update.</param>
    /// <param name="requests">Unused: the clock already acknowledges what was requested.</param>
    /// <returns><paramref name="clocked"/>.</returns>
    public SoundAcknowledgements<TLibrary, TSound> ForNextUpdate(SoundAcknowledgements<TLibrary, TSound> clocked, SoundAcknowledgements<TLibrary, TSound> requests) => clocked;
}
