namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The acknowledgements the game's sound handlers read during an update. A handler waits until its library acknowledges
/// the request it sent, and then the zero that follows, so when a request is acknowledged decides when the next sound
/// can be sent. The game reads the <see cref="SoundAcceptanceClock"/>'s own acknowledgements
/// (<see cref="ClockedSoundAcknowledgements"/>); a replay whose recording shows different acknowledgement timing supplies
/// the recorded outcome instead.
/// </summary>
/// <remarks>
/// Consulted once at the start of each update, before the sound handlers run. Evidence: the sound handlers
/// (<c>HandleSounds</c>, <c>$82:89EF</c>) and the SMV recordings' acknowledgement traces.
/// </remarks>
[CompatibilityService("the acknowledgements an update's sound handlers read")]
public interface ISoundAcknowledgementTiming
{
    /// <summary>The acknowledgements the next update's sound handlers read.</summary>
    /// <param name="clocked">The acknowledgements the session's clock reported after the previous update.</param>
    /// <param name="requests">The requests the sound handlers last sent to the three libraries (music is zero).</param>
    /// <returns>The acknowledgements to present to the update; only the three libraries' are read.</returns>
    SoundAcknowledgements ForNextUpdate(SoundAcknowledgements clocked, SoundAcknowledgements requests);
}
