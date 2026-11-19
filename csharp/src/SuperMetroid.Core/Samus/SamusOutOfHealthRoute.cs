namespace SuperMetroid.Core.Samus;

/// <summary>
/// Where a gameplay update's out-of-health check, after the room's own update and before the play time is counted, sends
/// Samus (#2509, #2517).
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

    /// <summary>
    /// Out of health without reserves while main gameplay owns the update and none of its steps started another game
    /// state: Samus's death begins, and the update counts the play time.
    /// </summary>
    Death,

    /// <summary>
    /// Out of health without reserves while another game state owns the update: one an earlier step of the update started
    /// (<see cref="Runtime.GameplayUpdateTransition"/>), or one that runs main gameplay's update behind it. Samus neither
    /// dies nor is refilled, and the update does not count the play time.
    /// </summary>
    DeathWithheld,

    /// <summary>
    /// A message box open at the update's end suspended it before its out-of-health check, which the update the box closes
    /// on runs: nothing is decided, and the update counts no play time.
    /// </summary>
    NotChecked,
}
