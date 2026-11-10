namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Native Spazer composition coordinates the beam tile rasterization places its strips by.</summary>
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
}
