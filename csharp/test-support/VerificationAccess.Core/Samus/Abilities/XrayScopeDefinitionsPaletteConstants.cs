using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Samus.Palettes;
namespace SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Palettes;

/// <summary>Cartridge values of <see cref="XrayScopeDefinitions.Palette"/> that only verification reads.</summary>
internal static class XrayScopeDefinitionsPaletteConstants
{
    /// <summary><c>$91:D727</c>, suit-indexed normal Samus palette pointers.</summary>
    /// <remarks>Alias of <see cref="SamusPaletteDefinitionsCommonConstants.NormalSuitPointers"/>;
    /// the three-word proof is issue #859 / #625.</remarks>
    public const int NormalSuitPointers = SamusPaletteDefinitionsCommonConstants.NormalSuitPointers;
    /// <summary>Bank expanded around a normal-suit palette pointer.</summary>
    public static readonly int PaletteBank = SamusSourceDefinitions.PaletteBank.Value;
    /// <summary>Number of colors copied when restoring the normal suit palette.</summary>
    public const int SuitColorCount = SamusPaletteDefinitions.Common.ColorsPerObjPalette;
    /// <summary><c>$9B:A3C0</c>, X-ray visor palette words.</summary>
    /// <remarks>Alias of <see cref="SamusSourceDefinitions.VisorColors"/>;
    /// the six-word proof is issue #865 / #625.</remarks>
    public static readonly int VisorWords = SamusSourceDefinitions.VisorColors.Value;

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
        public const int SamusCgramIndex = SamusPaletteDefinitions.Common.SamusObjPaletteStart;
        /// <summary>CGRAM color within Samus's palette occupied by the visor.</summary>
        public const int VisorCgramIndex =
            SamusCgramIndex + SamusPaletteDefinitions.Common.VisorColorOffset;
}
