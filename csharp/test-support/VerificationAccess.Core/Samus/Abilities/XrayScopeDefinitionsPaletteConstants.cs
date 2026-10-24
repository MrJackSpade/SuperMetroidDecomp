using SuperMetroid.Core.Samus.Palettes;
namespace SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Palettes;

/// <summary>Cartridge values of <see cref="XrayScopeDefinitions.Palette"/> that only verification reads.</summary>
internal static class XrayScopeDefinitionsPaletteConstants
{
    /// <summary><c>$91:D727</c>, suit-indexed normal Samus palette pointers.</summary>
    /// <remarks>Alias of <see cref="SamusPaletteRomDataCommonConstants.NormalSuitPointers"/>;
    /// the three-word proof is issue #859 / #625.</remarks>
    public const int NormalSuitPointers = SamusPaletteRomDataCommonConstants.NormalSuitPointers;
    /// <summary>Bank expanded around a normal-suit palette pointer.</summary>
    public const int PaletteBank = SamusPaletteRomData.Banks.Palette;
    /// <summary>Number of colors copied when restoring the normal suit palette.</summary>
    public const int SuitColorCount = SamusPaletteRomData.Common.ColorsPerObjPalette;
    /// <summary><c>$9B:A3C0</c>, X-ray visor palette words.</summary>
    /// <remarks>Alias of <see cref="SamusPaletteRomData.Visor.Colors"/>;
    /// the six-word proof is issue #865 / #625.</remarks>
    public const int VisorWords = SamusPaletteRomData.Visor.Colors;

    /// <summary>Provides X-ray palette bank, normal suit pointers, visor words, and suit color count.</summary>
    extension(XrayScopeDefinitions.Palette)
    {
        /// <inheritdoc cref="XrayScopeDefinitionsPaletteConstants.NormalSuitPointers"/>
        internal static int NormalSuitPointers => XrayScopeDefinitionsPaletteConstants.NormalSuitPointers;
        /// <inheritdoc cref="XrayScopeDefinitionsPaletteConstants.PaletteBank"/>
        internal static int PaletteBank => XrayScopeDefinitionsPaletteConstants.PaletteBank;
        /// <inheritdoc cref="XrayScopeDefinitionsPaletteConstants.SuitColorCount"/>
        internal static int SuitColorCount => XrayScopeDefinitionsPaletteConstants.SuitColorCount;
        /// <inheritdoc cref="XrayScopeDefinitionsPaletteConstants.VisorWords"/>
        internal static int VisorWords => XrayScopeDefinitionsPaletteConstants.VisorWords;
    }
        /// <summary>First CGRAM color occupied by Samus's OBJ palette.</summary>
        public const int SamusCgramIndex = SamusPaletteRomData.Common.SamusObjPaletteStart;
        /// <summary>CGRAM color within Samus's palette occupied by the visor.</summary>
        public const int VisorCgramIndex =
            SamusCgramIndex + SamusPaletteRomData.Common.VisorColorOffset;
}
