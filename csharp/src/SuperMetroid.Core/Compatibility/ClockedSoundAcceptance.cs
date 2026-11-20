namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The game's own acceptance timing: each update's sound queues see what the session's
/// <see cref="SoundAcceptanceClock{TLibrary, TSound}"/> reported accepted after the previous update, so a request is accepted only
/// at its library's next request check.
/// </summary>
/// <typeparam name="TLibrary">A sound library.</typeparam>
/// <typeparam name="TSound">The sound a library is asked for.</typeparam>
/// <remarks>
/// Stateless; every session shares <see cref="Instance"/>, and the clock it reads is the session's own. Evidence: the
/// original game's sound queues see a library's reply only once the sound program has read the request, which it does
/// at its request checks on its own timer, never as the request is made; the sound acceptance clock check compares the
/// acknowledgements the game session's clock reports on every update with a recording of the emulated sound program's.
/// </remarks>
[CompatibilityService("each update's sound queues see the requests the session's clock has accepted")]
public sealed class ClockedSoundAcceptance<TLibrary, TSound> : ISoundAcceptanceTiming<TLibrary, TSound>
    where TLibrary : struct, Enum
    where TSound : struct, IEquatable<TSound>
{
    /// <summary>The shared, stateless instance every session uses.</summary>
    public static ClockedSoundAcceptance<TLibrary, TSound> Instance { get; } = new();

    /// <summary>Restricts the timing to its shared instance.</summary>
    private ClockedSoundAcceptance() { }

    /// <summary>Returns what the clock reported accepted, unchanged.</summary>
    /// <param name="clocked">What each library reported as accepted by the session's clock after the previous update.</param>
    /// <param name="requested">Unused: the clock already accepts what was requested.</param>
    /// <returns><paramref name="clocked"/>.</returns>
    public SoundAcknowledgements<TLibrary, TSound> ForNextUpdate(SoundAcknowledgements<TLibrary, TSound> clocked, SoundAcknowledgements<TLibrary, TSound> requested) => clocked;
}
