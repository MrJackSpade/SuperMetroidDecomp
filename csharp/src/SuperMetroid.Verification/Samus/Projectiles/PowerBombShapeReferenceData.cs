namespace SuperMetroid.Verification.Samus.Projectiles;

/// <summary>Native bank $88 shape-table addresses used as independent comparison data.</summary>
internal static class PowerBombShapeReferenceData
{
    /// <summary>$88:A266, PowerBombExplosion_ShapeDefinitionTable_Unscaled_width: 32 bottom-to-center bytes.</summary>
    internal const int Width = 0x88a266;
    /// <summary>$88:A286, PowerBombExplosion_ShapeDefinitionTable_Unscaled_topOffset: 32 inclusive boundaries.</summary>
    internal const int TopOffset = 0x88a286;
    /// <summary>$88:9246, the seventeen pre-scaled white window shapes, 192 bytes each.</summary>
    internal const int PreScaledWhite = 0x889246;
    /// <summary>$88:9F06, the four pre-scaled yellow window shapes that follow the white ones.</summary>
    internal const int PreScaledYellow = 0x889f06;
    /// <summary>The lines of one pre-scaled shape, from its middle line outward.</summary>
    internal const int ShapeLines = 192;
}
