namespace SuperMetroid.Verification.Audio;

/// <summary>
/// Accepts every sound the handlers requested on the next update, ignoring the session's acceptance clock, so a check
/// sees the sound handlers' own sequencing without the clock's waits.
/// </summary>
/// <remarks>
/// The handlers' current request for a library is exactly what they last asked that library for: every request,
/// replacement and withdrawal updates it.
/// </remarks>
internal sealed class NextUpdateSoundAcceptance : ISoundAcceptanceTiming<SoundEffectLibrary, SoundEffectId>
{
    /// <summary>The stateless timing.</summary>
    internal static NextUpdateSoundAcceptance Instance { get; } = new();

    /// <summary>Prevents instances other than <see cref="Instance"/>.</summary>
    private NextUpdateSoundAcceptance() { }

    /// <summary>Returns the handlers' latest requests as accepted, ignoring the clock.</summary>
    public SoundAcknowledgements<SoundEffectLibrary, SoundEffectId> ForNextUpdate(SoundAcknowledgements<SoundEffectLibrary, SoundEffectId> clocked, SoundAcknowledgements<SoundEffectLibrary, SoundEffectId> requested) =>
        requested;
}
