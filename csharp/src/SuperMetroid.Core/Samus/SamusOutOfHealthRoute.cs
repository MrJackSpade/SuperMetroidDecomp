namespace SuperMetroid.Core.Samus;

/// <summary>
/// Where a gameplay update's out-of-health check, after the room's own update and before the play time is counted, sends
/// Samus (#2509).
/// </summary>
public enum SamusOutOfHealthRoute
{
    /// <summary>Samus has health left: the update ends ordinarily and counts the play time.</summary>
    HasHealth,

    /// <summary>
    /// Out of health with energy in the AUTO reserves: the refill begins, over whatever else the update started, and the
    /// update counts the play time.
    /// </summary>
    ReserveRefill,

    /// <summary>Out of health without reserves while main gameplay owns the update: Samus's death begins, and the update counts the play time.</summary>
    Death,

    /// <summary>
    /// Out of health without reserves while the time-up owns the update, because the escape countdown ran out earlier in it
    /// or the update runs behind the time-up's white-out: Samus neither dies nor is refilled, and the update does not count
    /// the play time.
    /// </summary>
    DeathWithheld,
}
