namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>Bank-$8B text-glow pool and palette progression definitions.</summary>
internal static class CinematicTextGlowRomData
{
    /// <summary>$97F8/$9831: descending word-indexed slots $0E through $00.</summary>
    internal const int SlotCount = 8;
    /// <summary>$98D8: calls between successive glyph palette updates.</summary>
    internal const ushort PaletteDuration = 5;
    /// <summary>$98CC: fourth and final two-bit glyph palette.</summary>
    internal const int FinalPalette = 3;
    /// <summary>$988A/$98BD: each cinematic tilemap row occupies 32 words.</summary>
    internal const int RowWidth = 32;
}
