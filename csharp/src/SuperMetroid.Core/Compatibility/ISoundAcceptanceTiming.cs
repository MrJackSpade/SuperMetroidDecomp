namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// When the sound libraries accept the sounds the game requested, as the game's sound queues see it at the start of an
/// update. A queue that has requested a sound waits until its library reports that sound accepted, and then until the
/// library reports the withdrawn request accepted, before it requests the next; so the update on which each request is
/// accepted decides when the next queued sound can start.
/// </summary>
/// <typeparam name="TLibrary">A sound library.</typeparam>
/// <typeparam name="TSound">The sound a library is asked for.</typeparam>
/// <remarks>
/// <para>
/// Consulted once at the start of each update, before the sound queues run. The game uses
/// <see cref="ClockedSoundAcceptance{TLibrary, TSound}"/>, the session's <see cref="SoundAcceptanceClock{TLibrary, TSound}"/>, under which a
/// library accepts a request only at its next request check, so a request can wait one or more updates. Verification
/// hosts supply other timings: one accepts every request on the update after it is made, isolating the handlers from
/// the clock, and a movie replay reports, for the updates its recording covers, whether each waiting library moved on.
/// </para>
/// <para>
/// Evidence: the game's sound queues wait on the library's reply before moving on; the SMV recordings show on which
/// update each waiting library moved on, and the clock reproduces those updates.
/// </para>
/// </remarks>
[CompatibilityService("on which update each sound library accepts the sound the game requested, as its sound queues see it")]
public interface ISoundAcceptanceTiming<TLibrary, TSound>
    where TLibrary : struct, Enum
    where TSound : struct, IEquatable<TSound>
{
    /// <summary>What each library reports as accepted to the next update's sound queues.</summary>
    /// <param name="clocked">What each library reported as accepted by the session's clock after the previous update.</param>
    /// <param name="requested">The sound each handler last requested from its library, or none.</param>
    /// <returns>What each library reports as accepted to the update.</returns>
    SoundAcknowledgements<TLibrary, TSound> ForNextUpdate(SoundAcknowledgements<TLibrary, TSound> clocked, SoundAcknowledgements<TLibrary, TSound> requested);
}
