namespace SuperMetroid.Core.Game;

/// <summary>Bank-$A4 Crocomire RGB5 sources and exact native CGRAM transfer geometry.</summary>
public static class CrocomirePaletteRomData
{
    /// <summary>First eight colors of the body/head BG palette at $A4:B89D, restored after hurt flashes.</summary>
    public const int FightBodySource = 0xa4b89d;
    /// <summary>Number of RGB555 words restored to the fight body/head palette.</summary>
    public const int FightBodyCount = 8;
    /// <summary>First CGRAM color index receiving the body/head restoration.</summary>
    public const int FightBodyDestination = 112;

    /// <summary>
    /// Initial wall transfer at $A4:B8BD. The initializer copies seventeen words,
    /// so its final word is also the next source palette's first word.
    /// </summary>
    public const int InitialWallSource = 0xa4b8bd;
    /// <summary>Number of RGB555 words copied by the overlapping initial wall transfer.</summary>
    public const int InitialWallCount = 17;
    /// <summary>First CGRAM color index receiving the initial wall transfer.</summary>
    public const int InitialWallDestination = 160;

    /// <summary>
    /// Initial projectile transfer at $A4:B8DD. Its seventeenth word is also
    /// the skeleton-arm source palette's first word.
    /// </summary>
    public const int InitialProjectileSource = 0xa4b8dd;
    /// <summary>Number of RGB555 words copied by the overlapping initial projectile transfer.</summary>
    public const int InitialProjectileCount = 17;
    /// <summary>First CGRAM color index receiving the initial projectile transfer.</summary>
    public const int InitialProjectileDestination = 208;

    /// <summary>Skeleton-arm OBJ palette at $A4:B8FD, loaded during wall break.</summary>
    public const int SkeletonArmSource = 0xa4b8fd;
    /// <summary>Number of RGB555 words in the complete skeleton-arm palette.</summary>
    public const int SkeletonArmCount = 16;
    /// <summary>First CGRAM color index receiving the skeleton-arm palette.</summary>
    public const int SkeletonArmDestination = 144;

    /// <summary>Wall-spike OBJ palette at $A4:B91D, loaded at the rumble terminator.</summary>
    public const int WallSpikesSource = 0xa4b91d;
    /// <summary>Number of RGB555 words in the complete wall-spike palette.</summary>
    public const int WallSpikesCount = 16;
    /// <summary>First CGRAM color index receiving the wall-spike palette.</summary>
    public const int WallSpikesDestination = 176;
}
