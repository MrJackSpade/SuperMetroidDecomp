
namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Cartridge values of <see cref="SamusLoadingSuitPaletteFxColorDefinitions"/> that only verification reads.</summary>
internal static class SamusLoadingSuitPaletteFxColorDefinitionsConstants
{
    /// <summary>Every suit-loading program lasts 265 frames.</summary>
    public const int CycleFrames = 265;

    /// <summary>Provides the palette cycle period for Samus suit loading effects.</summary>
    extension(SamusLoadingSuitPaletteFxColorDefinitions)
    {
        /// <inheritdoc cref="SamusLoadingSuitPaletteFxColorDefinitionsConstants.CycleFrames"/>
        internal static int CycleFrames => SamusLoadingSuitPaletteFxColorDefinitionsConstants.CycleFrames;
    }
}
