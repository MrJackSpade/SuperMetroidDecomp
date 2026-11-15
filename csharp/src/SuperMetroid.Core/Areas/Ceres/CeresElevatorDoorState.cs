namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>The state of the elevator room's blocked door (#1953).</summary>
internal sealed class CeresElevatorDoorState
{
    /// <summary>Where the door stands in its destruction.</summary>
    public CeresElevatorDoorPhase Phase { get; internal set; }

    /// <summary>
    /// Calls left in the door's destruction rumble, 48 when it starts and decremented each call; the call that takes it
    /// below zero ends the destruction, which never counts it again.
    /// </summary>
    public int RumbleCallsLeft { get; internal set; }

    /// <summary>Calls left before the next destruction explosion, decremented each call and reloaded once it runs below zero.</summary>
    public int ExplosionIntervalLeft { get; internal set; }

    /// <summary>
    /// The place in <see cref="CeresDoorRumbleGeometryDefinitions.ExplosionOffsets"/> of the last destruction burst; each
    /// burst takes the place before it, and the last place after the first.
    /// </summary>
    public int LastExplosionPlace { get; internal set; }
}
