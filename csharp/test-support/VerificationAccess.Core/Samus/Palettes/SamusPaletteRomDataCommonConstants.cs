
namespace SuperMetroid.Core.Samus.Palettes;

/// <summary>Cartridge values of <see cref="SamusPaletteRomData.Common"/> that only verification reads.</summary>
internal static class SamusPaletteRomDataCommonConstants
{
    /// <summary><c>$91:D727</c>, Power/Varia/Gravity normal-palette pointers.</summary>
    /// <remarks>
    /// Issue #859 / #625: the pinned NTSC J/U v1.0 ROM has three little-endian
    /// words <c>$9400,$9520,$9800</c> at byte offsets 0, 2, 4. They select
    /// the authored normal Power, Varia, and Gravity palettes in bank
    /// <c>$9B</c>. Every production suit selector reaches only those even
    /// offsets; Gravity has priority over Varia when both equipment bits
    /// are set. The X-ray and projectile catalogs alias this same physical
    /// table. NormalSuitPalettePointer implements the three semantic suit
    /// cases directly; no stored pointer lookup is retained.
    /// </remarks>
    public const int NormalSuitPointers = 0x91d727;

    /// <summary>Provides normal suit palette pointers used by common Samus color checks.</summary>
    extension(SamusPaletteRomData.Common)
    {
        /// <inheritdoc cref="SamusPaletteRomDataCommonConstants.NormalSuitPointers"/>
        internal static int NormalSuitPointers => SamusPaletteRomDataCommonConstants.NormalSuitPointers;
    }
}
