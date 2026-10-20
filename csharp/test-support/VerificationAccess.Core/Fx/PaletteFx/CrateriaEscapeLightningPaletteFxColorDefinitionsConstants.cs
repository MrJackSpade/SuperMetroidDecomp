
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
    /// <summary>$8D:FE05/$FF2B duration schedule: three quiet intervals between one-tick flashes.</summary>
    internal static ushort Duration(int frame) => frame switch
    {
        0 => 49,
        5 => 17,
        7 => 24,
        >= 0 when frame < CrateriaEscapeLightningPaletteFxColorDefinitionsConstants.FrameCount => 1,
        _ => throw new ArgumentOutOfRangeException(nameof(frame)),
    };
    /// <summary>Both loops contain eleven timed records.</summary>
    public static int FrameCount => CrateriaEscapeLightningPaletteFxColorDefinitions.All[0].Rows.FrameCount;
}
