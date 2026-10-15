
namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Cartridge values of <see cref="CrateriaEscapeLightningPaletteFxColorDefinitions"/> that only verification reads.</summary>
internal static class CrateriaEscapeLightningPaletteFxColorDefinitionsConstants
{
    /// <summary>Both complete loops last 98 frames.</summary>
    public const int CycleFrames = 98;

    /// <summary>Provides the escape lightning palette cycle period used to check its imported program.</summary>
    extension(CrateriaEscapeLightningPaletteFxColorDefinitions)
    {
        /// <inheritdoc cref="CrateriaEscapeLightningPaletteFxColorDefinitionsConstants.CycleFrames"/>
        internal static int CycleFrames => CrateriaEscapeLightningPaletteFxColorDefinitionsConstants.CycleFrames;
    }
}
