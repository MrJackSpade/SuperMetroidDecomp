namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>The first glyph of each trail's consecutive characters.</summary>
internal static class ProjectileTrailGlyphDefinitions
{
    /// <summary>$90:B4CB/B52D uses consecutive ice glyphs beginning at tile $38, palette6, priority2.</summary>
    internal const int IceFirstTile = 0x38;
    /// <summary>$90:B58F uses four consecutive wave glyphs beginning at tile $3C, palette5, priority2.</summary>
    internal const int WaveFirstTile = 0x3c;
    /// <summary>$90:B5A1 uses four consecutive missile glyphs beginning at tile $48, palette5, priority2.</summary>
    internal const int MissileFirstTile = 0x48;
}
