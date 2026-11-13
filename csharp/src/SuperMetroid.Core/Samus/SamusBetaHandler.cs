namespace SuperMetroid.Core.Samus;

/// <summary>
/// The second half of Samus's per-frame work, which Samus's control setups select: movement, animation and the energy checks.
/// <see cref="TitleDemo"/> and <see cref="Fatal"/> are what a frame runs instead while the attract demo or the death
/// sequence owns Samus. Exactly one is selected at a time.
/// </summary>
internal enum SamusBetaHandler
{
    /// <summary>
    /// Ordinary play: movement, the timer handler, animation, pose transitions, periodic damage and the low-energy check.
    /// </summary>
    Normal,

    /// <summary>
    /// Another locked state, such as riding an elevator: no low-energy check.
    /// </summary>
    Locked,

    /// <summary>Locked by a script, as the stationary lock does: no animation or pose transitions.</summary>
    ScriptLocked,

    /// <summary>
    /// Held by Mother Brain's rainbow beam: no movement at all and no low-energy check, and reserve recovery leaves it in
    /// place.
    /// </summary>
    RainbowBeam,

    /// <summary>At a refill or map station: Samus neither moves nor animates.</summary>
    RefillStation,

    /// <summary>During a suit pickup's transformation: nothing runs.</summary>
    SuitPickup,

    /// <summary>The attract demo: without periodic damage or energy checks.</summary>
    TitleDemo,

    /// <summary>
    /// Entering or leaving the gunship: its own low-health check and no escape-timer handling.
    /// </summary>
    Gunship,

    /// <summary>The death sequence, which runs neither half.</summary>
    Fatal,
}
