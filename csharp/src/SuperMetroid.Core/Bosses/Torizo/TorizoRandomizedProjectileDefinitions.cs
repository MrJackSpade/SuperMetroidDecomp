namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// Fixed randomized launch definitions shared by Bomb and Golden Torizo, each written for a launch to the right and mirrored
/// for one to the left (#2411).
/// </summary>
internal static class TorizoRandomizedProjectileDefinitions
{
    /// <summary>Bomb Torizo's Chozo-orb launch, the way it faces.</summary>
    internal static TorizoLaunch BombChozoOrb { get; } = new(TorizoProjectileSegment.ChozoOrbMovingLeft, TorizoProjectileSegment.ChozoOrbMovingRight,
        new(27, -40), new EightEightVector<RoomPixels>(SignedEightEight.FromPixelFraction(25, 16), SignedEightEight.FromPixelFraction(-13, 8)));

    /// <summary>Golden Torizo's Chozo-orb launch, the way it faces.</summary>
    internal static TorizoLaunch GoldenChozoOrb { get; } = new(TorizoProjectileSegment.ChozoOrbMovingLeft, TorizoProjectileSegment.ChozoOrbMovingRight,
        new(27, -40), new EightEightVector<RoomPixels>(SignedEightEight.FromPixels(1), SignedEightEight.FromPixelFraction(-7, 4)));

    /// <summary>Golden Torizo's egg launch, the way it faces as it lays the egg.</summary>
    internal static TorizoLaunch GoldenEgg { get; } = new(TorizoProjectileSegment.EggBouncingLeft, TorizoProjectileSegment.EggBouncingRight,
        new(16, -1), new EightEightVector<RoomPixels>(SignedEightEight.FromPixelFraction(1, 2), SignedEightEight.FromPixelFraction(-3, 2)));
}
