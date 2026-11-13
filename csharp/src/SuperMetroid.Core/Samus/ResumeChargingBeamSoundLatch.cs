namespace SuperMetroid.Core.Samus;

/// <summary>
/// The post-draw handler's three-state latch that resumes the
/// charging-beam sound one pass after a spin exit or hurt-flash recovery (#1323).
/// </summary>
public enum ResumeChargingBeamSoundLatch
{
    /// <summary>Zero: nothing to resume.</summary>
    Clear,

    /// <summary>One: the next post-draw pass queues the resume sound if X is still held, then clears.</summary>
    ResumeNextPass,

    /// <summary>
    /// A negative latch value. The handler's signed branch rearms to <see cref="ResumeNextPass"/> without any
    /// audio work that pass. Core never writes it; it enters only through an imported state.
    /// </summary>
    SignedLatch,
}
