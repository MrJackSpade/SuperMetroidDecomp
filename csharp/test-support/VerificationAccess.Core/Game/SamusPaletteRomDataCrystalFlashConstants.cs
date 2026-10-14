using SuperMetroid.Core.Game;

/// <summary>Cartridge values of <see cref="SamusPaletteRomData.CrystalFlash"/> that only verification reads.</summary>
internal static class SamusPaletteRomDataCrystalFlashConstants
{
    /// <summary><c>$90:C3C9</c>, twelve beam-loadout palette pointers.</summary>
    /// <remarks>
    /// This is the same physical table as
    /// <see cref="SamusProjectileRomData.Beams.PalettePointers"/>:
    /// Ice, then Plasma, then Wave, then Spazer, then Power priority
    /// exactly selects one of five bank-$90 palettes for indices 0..11.
    /// Crystal Flash restoration uses the masked equipped-beam word to
    /// read one of these pointers before copying sixteen colors.
    /// Investigation: #625 / #900.
    /// </remarks>
    public const int BeamPalettePointers = 0x90c3c9;

    /// <summary>Provides Crystal Flash beam palette pointers for effect color comparisons.</summary>
    extension(SamusPaletteRomData.CrystalFlash)
    {
        /// <inheritdoc cref="SamusPaletteRomDataCrystalFlashConstants.BeamPalettePointers"/>
        internal static int BeamPalettePointers => SamusPaletteRomDataCrystalFlashConstants.BeamPalettePointers;
    }
}
