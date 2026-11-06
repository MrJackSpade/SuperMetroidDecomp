namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The game's own acceptance timing: each update's sound handlers see what the session's
/// <see cref="SoundAcceptanceClock{TSound}"/> reported accepted after the previous update, so a request is accepted only
/// at its library's next request check.
/// </summary>
/// <typeparam name="TSound">The sound a library is asked for.</typeparam>
/// <remarks>Stateless; every session shares <see cref="Instance"/>, and the clock it reads is the session's own.</remarks>
[CompatibilityService("each update's sound handlers see the requests the session's clock has accepted")]
public sealed class ClockedSoundAcceptance<TSound> : ISoundAcceptanceTiming<TSound>
    where TSound : struct, IEquatable<TSound>
{
    /// <summary>The shared, stateless instance every session uses.</summary>
    public static ClockedSoundAcceptance<TSound> Instance { get; } = new();

    /// <summary>Restricts the timing to its shared instance.</summary>
    private ClockedSoundAcceptance() { }

    /// <summary>Returns what the clock reported accepted, unchanged.</summary>
    /// <param name="clocked">What each library reported as accepted by the session's clock after the previous update.</param>
    /// <param name="requested">Unused: the clock already accepts what was requested.</param>
    /// <returns><paramref name="clocked"/>.</returns>
    public SoundAcknowledgements<TSound> ForNextUpdate(SoundAcknowledgements<TSound> clocked, SoundAcknowledgements<TSound> requested) => clocked;
}
