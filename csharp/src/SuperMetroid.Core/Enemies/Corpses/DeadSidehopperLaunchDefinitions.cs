namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>Four-phase signed 1/256-pixel launch cycle for the Tourian sidehopper corpse.</summary>
public static class DeadSidehopperLaunchDefinitions
{
    /// <summary>
    /// Post-landing launch velocities indexed by the modulo-four jump phase: X and Y speeds from
    /// the authored X and Y tables.
    /// </summary>
    private static readonly EightEightVector<RoomPixels>[] Launches =
    [
        new(SignedEightEight.FromParts(1, 192), -SignedEightEight.FromParts(2, 0)), new(SignedEightEight.FromParts(1, 32), -SignedEightEight.FromParts(2, 0)),
        new(SignedEightEight.FromParts(1, 32), -SignedEightEight.FromParts(2, 0)), new(SignedEightEight.FromParts(3, 0), -SignedEightEight.FromParts(4, 0)),
    ];

    /// <summary>The number of launches in the cycle; the jump phase advances modulo this count.</summary>
    public static int PhaseCount => Launches.Length;

    /// <summary>The launch velocity for one modulo-four jump phase.</summary>
    public static EightEightVector<RoomPixels> Velocity(int phase) => Launches[phase];
}
