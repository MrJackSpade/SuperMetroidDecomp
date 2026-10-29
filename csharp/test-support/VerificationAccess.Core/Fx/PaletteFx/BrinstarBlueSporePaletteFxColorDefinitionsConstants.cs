
namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Cartridge values of <see cref="BrinstarBlueSporePaletteFxColorDefinitions"/> that only verification reads.</summary>
internal static class BrinstarBlueSporePaletteFxColorDefinitionsConstants
{
    /// <summary>Frames from the first record through the next first record.</summary>
    public const int CycleFrames = 140;

    /// <summary>Provides the blue spore palette cycle period used to check the imported Brinstar effect.</summary>
    extension(BrinstarBlueSporePaletteFxColorDefinitions)
    {
        /// <inheritdoc cref="BrinstarBlueSporePaletteFxColorDefinitionsConstants.CycleFrames"/>
        internal static int CycleFrames => BrinstarBlueSporePaletteFxColorDefinitionsConstants.CycleFrames;
    }
    /// <summary>Fourteen ten-frame records form one complete spore-color cycle.</summary>
    public static int FrameCount => BrinstarBlueSporePaletteFxColorDefinitions.All[0].Rows.FrameCount;
}
