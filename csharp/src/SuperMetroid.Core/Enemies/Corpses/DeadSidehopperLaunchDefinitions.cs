namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>Four-phase signed 8.8 launch cycle for the Tourian sidehopper corpse.</summary>
public static class DeadSidehopperLaunchDefinitions
{
    /// <summary>
    /// Post-landing launch velocities indexed by the modulo-four jump phase: X words from
    /// $A9:D959 and Y words from $A9:D951.
    /// </summary>
    private static readonly EightEightVector<RoomPixels>[] Launches =
    [
        new(SignedEightEight.FromSignedRaw(0x01c0), SignedEightEight.FromSignedRaw(-0x0200)), new(SignedEightEight.FromSignedRaw(0x0120), SignedEightEight.FromSignedRaw(-0x0200)),
        new(SignedEightEight.FromSignedRaw(0x0120), SignedEightEight.FromSignedRaw(-0x0200)), new(SignedEightEight.FromSignedRaw(0x0300), SignedEightEight.FromSignedRaw(-0x0400)),
    ];

    /// <summary>The number of launches in the cycle; the jump phase advances modulo this count.</summary>
    public static int PhaseCount => Launches.Length;

    /// <summary>The launch velocity for one modulo-four jump phase.</summary>
    public static EightEightVector<RoomPixels> Velocity(int phase) => Launches[phase];
}
