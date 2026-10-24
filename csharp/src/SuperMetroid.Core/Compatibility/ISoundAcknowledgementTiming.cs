namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The acknowledgements the game's sound handlers read during an update. A handler waits until its library acknowledges
/// the request it sent, and then the nothing that follows, so when a request is acknowledged decides when the next sound
/// can be sent. The game reads the <see cref="SoundAcceptanceClock{TSound}"/>'s own acknowledgements
/// (<see cref="ClockedSoundAcknowledgements{TSound}"/>); a replay whose recording shows different acknowledgement timing
/// supplies the recorded outcome instead.
/// </summary>
/// <typeparam name="TSound">The sound a library is asked for.</typeparam>
/// <remarks>
/// Consulted once at the start of each update, before the sound handlers run. Evidence: the sound handlers
/// (<c>HandleSounds</c>, <c>$82:89EF</c>) and the SMV recordings' acknowledgement traces.
/// </remarks>
[CompatibilityService("the acknowledgements an update's sound handlers read")]
public interface ISoundAcknowledgementTiming<TSound>
    where TSound : struct, IEquatable<TSound>
{
    /// <summary>The acknowledgements the next update's sound handlers read.</summary>
    /// <param name="clocked">The acknowledgements the session's clock reported after the previous update.</param>
    /// <param name="requests">The requests the sound handlers last sent to the three libraries, as acknowledgements of them.</param>
    /// <returns>The acknowledgements to present to the update.</returns>
    SoundAcknowledgements<TSound> ForNextUpdate(SoundAcknowledgements<TSound> clocked, SoundAcknowledgements<TSound> requests);
}
