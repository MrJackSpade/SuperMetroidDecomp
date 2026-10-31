namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>Six authored spawn records used by gunship liftoff dust.</summary>
public static class GunshipDustDefinitions
{
    /// <summary>$86:A2C4, InitAI_EnemyProjectile_GunshipLiftoffDustClouds: dust is eighty pixels below Samus.</summary>
    public const ushort YOffset = 80;

    /// <summary>
    /// $86:A2D6..A2ED, .Xoffsets and .InstListPointers in
    /// InitAI_EnemyProjectile_GunshipLiftoffDustClouds. Even parameters 0/2/4 select
    /// right-side clouds; 6/8/A select left-side clouds, each starting its typed program (#641).
    /// </summary>
    internal static readonly Dictionary<int, GunshipDustPlacement> Placements = new()
    {
        [0] = new GunshipDustPlacement(64, GunshipDustProgram.RightInner),
        [2] = new GunshipDustPlacement(72, GunshipDustProgram.RightMiddle),
        [4] = new GunshipDustPlacement(80, GunshipDustProgram.RightOuter),
        [6] = new GunshipDustPlacement(-64, GunshipDustProgram.LeftInner),
        [8] = new GunshipDustPlacement(-72, GunshipDustProgram.LeftMiddle),
        [10] = new GunshipDustPlacement(-80, GunshipDustProgram.LeftOuter),
    };
}
