namespace SuperMetroid.Core.Samus;

/// <summary>
/// WRAM <c>ResumeChargingBeamSFXFlag</c> (<c>$0DC0</c>): the post-draw handler's three-state latch that resumes the
/// charging-beam sound one pass after a spin exit or hurt-flash recovery (#1323).
/// </summary>
public enum ResumeChargingBeamSoundLatch
{
    /// <summary>Native zero: nothing to resume.</summary>
    Clear,

    /// <summary>Native one: the next post-draw pass queues the resume sound if X is still held, then clears.</summary>
    ResumeNextPass,

    /// <summary>
    /// A native word with bit 15 set. The handler's signed branch rearms to <see cref="ResumeNextPass"/> without any
    /// audio work that pass. Core never writes it; it enters only through an imported native state.
    /// </summary>
    SignedLatch,
}
