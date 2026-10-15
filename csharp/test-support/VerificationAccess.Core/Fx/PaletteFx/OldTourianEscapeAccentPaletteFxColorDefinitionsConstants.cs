
namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Cartridge values of <see cref="OldTourianEscapeAccentPaletteFxColorDefinitions"/> that only verification reads.</summary>
internal static class OldTourianEscapeAccentPaletteFxColorDefinitionsConstants
{
    /// <summary>Both complete loops last 64 frames.</summary>
    public const int CycleFrames = 64;

    /// <summary>Provides the Old Tourian escape accent palette cycle period.</summary>
    extension(OldTourianEscapeAccentPaletteFxColorDefinitions)
    {
        /// <inheritdoc cref="OldTourianEscapeAccentPaletteFxColorDefinitionsConstants.CycleFrames"/>
        internal static int CycleFrames => OldTourianEscapeAccentPaletteFxColorDefinitionsConstants.CycleFrames;
    }
}
