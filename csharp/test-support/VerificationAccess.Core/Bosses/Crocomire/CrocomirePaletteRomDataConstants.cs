namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Cartridge values and helpers of <see cref="CrocomirePaletteRomData"/> that only verification reads.</summary>
internal static class CrocomirePaletteRomDataConstants
{
    /// <summary>First CGRAM color index receiving the body/head restoration.</summary>
    public const int FightBodyDestination = 112;
    /// <summary>First CGRAM color index receiving the initial wall transfer.</summary>
    public const int InitialWallDestination = 160;
    /// <summary>First CGRAM color index receiving the initial projectile transfer.</summary>
    public const int InitialProjectileDestination = 208;
    /// <summary>First CGRAM color index receiving the skeleton-arm palette.</summary>
    public const int SkeletonArmDestination = 144;
    /// <summary>First CGRAM color index receiving the wall-spike palette.</summary>
    public const int WallSpikesDestination = 176;
}
