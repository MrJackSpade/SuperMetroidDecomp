namespace SuperMetroid.Verification.Samus.Projectiles;

/// <summary>Native bank $88 shape-table addresses used as independent comparison data.</summary>
internal static class PowerBombShapeReferenceData
{
    /// <summary>$88:A266, PowerBombExplosion_ShapeDefinitionTable_Unscaled_width: 32 bottom-to-center bytes.</summary>
    internal const int Width = 0x88a266;
    /// <summary>$88:A286, PowerBombExplosion_ShapeDefinitionTable_Unscaled_topOffset: 32 inclusive boundaries.</summary>
    internal const int TopOffset = 0x88a286;
}
