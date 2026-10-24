namespace SuperMetroid.Core.Compatibility;

/// <summary>The game's own timing: each update reads the acknowledgements of the session's <see cref="SoundAcceptanceClock{TSound}"/>.</summary>
/// <typeparam name="TSound">The sound a library is asked for.</typeparam>
[CompatibilityService("each update reads the session clock's acknowledgements")]
public sealed class ClockedSoundAcknowledgements<TSound> : ISoundAcknowledgementTiming<TSound>
    where TSound : struct, IEquatable<TSound>
{
    /// <summary>The shared, stateless instance every session uses.</summary>
    public static ClockedSoundAcknowledgements<TSound> Instance { get; } = new();

    /// <summary>Restricts the policy to its shared instance.</summary>
    private ClockedSoundAcknowledgements() { }

    /// <summary>Returns the clock's acknowledgements unchanged.</summary>
    /// <param name="clocked">The acknowledgements the session's clock reported after the previous update.</param>
    /// <param name="requests">Unused: the clock already acknowledges what was requested.</param>
    /// <returns><paramref name="clocked"/>.</returns>
    public SoundAcknowledgements<TSound> ForNextUpdate(SoundAcknowledgements<TSound> clocked, SoundAcknowledgements<TSound> requests) => clocked;
}
