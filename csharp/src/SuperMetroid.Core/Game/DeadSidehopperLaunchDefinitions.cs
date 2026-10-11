namespace SuperMetroid.Core.Game;

/// <summary>Four-phase signed 8.8 launch cycle for the Tourian sidehopper corpse.</summary>
public static class DeadSidehopperLaunchDefinitions
{
    /// <summary>
    /// Post-landing launch velocities indexed by the modulo-four jump phase: X words from
    /// $A9:D959 and Y words from $A9:D951.
    /// </summary>
    private static readonly EightEightVector<RoomPixels>[] Launches =
    [
        new(new SnesSignedEightEight(0x01c0), new SnesSignedEightEight(0xfe00)), new(new SnesSignedEightEight(0x0120), new SnesSignedEightEight(0xfe00)),
        new(new SnesSignedEightEight(0x0120), new SnesSignedEightEight(0xfe00)), new(new SnesSignedEightEight(0x0300), new SnesSignedEightEight(0xfc00)),
    ];

    /// <summary>The launch velocity for one modulo-four jump phase.</summary>
    public static EightEightVector<RoomPixels> Velocity(int phase) => Launches[phase];
}
