namespace SuperMetroid.Core.Samus.Palettes;

/// <summary>Cartridge values of <see cref="SamusPaletteDefinitions.CrystalFlash"/> that only verification reads.</summary>
internal static class SamusPaletteDefinitionsCrystalFlashConstants
{
    /// <summary><c>$90:C3C9</c>, twelve beam-loadout palette pointers.</summary>
    /// <remarks>
    /// This is the same physical table as the beam palette extraction reads:
    /// Ice, then Plasma, then Wave, then Spazer, then Power priority
    /// exactly selects one of five bank-$90 palettes for indices 0..11.
    /// Crystal Flash restoration uses the masked equipped-beam word to
    /// read one of these pointers before copying sixteen colors.
    /// Investigation: #625 / #900.
    /// </remarks>
    public const int BeamPalettePointers = 0x90c3c9;

    /// <summary>Provides Crystal Flash beam palette pointers for effect color comparisons.</summary>
    extension(SamusPaletteDefinitions.CrystalFlash)
    {
        /// <inheritdoc cref="SamusPaletteDefinitionsCrystalFlashConstants.BeamPalettePointers"/>
        internal static int BeamPalettePointers => SamusPaletteDefinitionsCrystalFlashConstants.BeamPalettePointers;
    }
        /// <summary>First CGRAM color of OBJ palette six.</summary>
        public const int BodyCgramStart = 0xe0;
        /// <summary>First CGRAM color of the bubble portion in OBJ palette six.</summary>
        public const int BubbleCgramStart = 0xea;
}
