namespace SuperMetroid.Core.World;

/// <summary>What one run of a room's main did that the gameplay update acts on (#2545).</summary>
public enum RoomMainOutcome
{
    /// <summary>The main moved the game nowhere: the update goes on to its other transitions.</summary>
    None,

    /// <summary>
    /// The Ceres elevator shaft's main took Samus onto the elevator, standing and locked, and the update moves the game to
    /// the Ceres departure.
    /// </summary>
    CeresElevatorTookSamus,
}
