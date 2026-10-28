namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Shared native Spazer composition coordinates for sprite geometry and tile rasterization.</summary>
/// <remarks>
/// Diagonal origins remain required source inputs under InstalledProjectileSprites.frames
/// in issue1165. Sharing them does not derive or exempt them. Tile-mask consumers must
/// not independently infer these origins from the same mask they generate with them.
/// </remarks>
internal static class SpazerCompositionGeometryDefinitions
{
    /// <summary>$93:D10E/D6EA/D842 use small8x8 OBJ cells in the ordinary gameplay size mode.</summary>
    internal const int TileSize = 8;

    /// <summary>$93:D110/D115: the first diagonal strip comprises two adjacent small OBJ cells.</summary>
    internal const int DiagonalStripWidth = 2 * TileSize;

    /// <summary>$93:D110: tile32's first H-flipped diagonal pair starts at X=-14. This chosen origin remains REQUIRED under InstalledProjectileSprites.frames.</summary>
    internal const int DiagonalFirstPairOriginX = -14;

    /// <summary>$93:D112: that first diagonal pair starts at Y=0. This chosen origin remains REQUIRED under InstalledProjectileSprites.frames.</summary>
    internal const int DiagonalFirstPairOriginY = 0;

    /// <summary>$93:D6EE/D846: horizontal tile30 has Y=-4, centering its8px cell on the projectile. Painted rows3/4 have pixel centers-0.5/+0.5.</summary>
    internal const int HorizontalStripOriginY = -TileSize / 2;

    /// <summary>$93:D10E..D842: how each seed direction mirrors its cells.</summary>
    internal static readonly IReadOnlyDictionary<SpazerSeedDirection, TileFlip> SeedFlips =
        new Dictionary<SpazerSeedDirection, TileFlip>
        {
            [SpazerSeedDirection.UpRight] = TileFlip.Horizontal,
            [SpazerSeedDirection.DownRight] = TileFlip.Horizontal | TileFlip.Vertical,
            [SpazerSeedDirection.DownLeft] = TileFlip.Vertical,
            [SpazerSeedDirection.UpLeft] = TileFlip.None,
            [SpazerSeedDirection.Down] = TileFlip.Vertical,
            [SpazerSeedDirection.Left] = TileFlip.None,
            [SpazerSeedDirection.Up] = TileFlip.None,
            [SpazerSeedDirection.Right] = TileFlip.Horizontal,
        };
}
