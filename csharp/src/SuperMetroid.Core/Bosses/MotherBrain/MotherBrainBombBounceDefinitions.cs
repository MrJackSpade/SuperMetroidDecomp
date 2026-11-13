namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The Mother Brain bomb's Y acceleration while it falls and bounces. Before its first bounce it falls at the ordinary
/// gravity of 7; each later bounce's gravity grows by sixteen times the bounce number, capped at 64 per bounce. The bomb
/// expires once it lands from its last bounce.
/// </summary>
internal static class MotherBrainBombBounceDefinitions
{
    /// <summary>The ordinary fall gravity, used before the first bounce.</summary>
    internal const int FallAcceleration = 7;

    /// <summary>The drag taken from the bomb's sideways speed on every update of its first fall, stopping at rest.</summary>
    internal static readonly SignedEightEight FallDrag = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(2);

    /// <summary>The gravity after each bounce, from the first to the eighth and last.</summary>
    internal static readonly int[] BounceAccelerations = [16, 32, 64, 112, 176, 240, 304, 368];
}
