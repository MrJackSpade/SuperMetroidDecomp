namespace SuperMetroid.Core.Audio;

/// <summary>
/// Every stop of Samus's library (<see cref="SoundEffectLibrary.Samus"/>, #2580): a request that plays nothing and cuts
/// off the sound the library is playing. Each plays one note at zero volume, so it is a stop,
/// not a sound. It waits in the library's queue and holds the library until its end, as a sound does.
/// </summary>
/// <remarks>
/// Members are identities. Their values are arbitrary and nothing reads them; the import pipeline alone knows which
/// installed request each member is.
/// </remarks>
public enum SamusSoundStop
{
    /// <summary>Stops whichever sound the library plays; it interrupts any sound of the library.</summary>
    Any,
    /// <summary>Stops the X-ray scope's sound as the scope switches off.</summary>
    Xray,
    /// <summary>Stops the Wave Beam's special Power Bomb attack as one of its particles is removed.</summary>
    WaveSpecialAttack,
    /// <summary>Stops a spin jump's sound as the spin ends.</summary>
    SpinJump,
    /// <summary>Stops a Screw Attack's sound as the spin ends.</summary>
    ScrewAttack,
}
