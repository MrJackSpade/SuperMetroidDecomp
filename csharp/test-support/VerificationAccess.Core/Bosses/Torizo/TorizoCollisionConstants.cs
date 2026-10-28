namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Cartridge values and helpers of <see cref="TorizoCollision"/> that only verification reads.</summary>
internal static class TorizoCollisionConstants
{
    /// <summary>The <paramref name="index"/>th left-facing frame in mirror order.</summary>
    internal static GoldenTorizoCollisionComponent[] LeftFacing(int index) => TorizoCollisionDefinitions.LeftFacingFrames[index];
}
