namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>The state of the elevator room's blocked door (#1953).</summary>
internal sealed class CeresElevatorDoorState
{
    /// <summary>Where the door stands in its destruction.</summary>
    public CeresElevatorDoorPhase Phase { get; internal set; }

    /// <summary>
    /// Updates left in the door's destruction rumble, the one it ends on included: 49 when it starts, and the call that takes
    /// it to zero ends the destruction, which never counts it again (#2281).
    /// </summary>
    public int RumbleUpdatesLeft { get; internal set; }

    /// <summary>Updates left before the next destruction explosion, the one it bursts on included; reloaded as it bursts (#2281).</summary>
    public int ExplosionIntervalLeft { get; internal set; }

    /// <summary>
    /// The place in <see cref="CeresDoorRumbleGeometryDefinitions.ExplosionOffsets"/> of the last destruction burst; each
    /// burst takes the place before it, and the last place after the first.
    /// </summary>
    public int LastExplosionPlace { get; internal set; }
}
