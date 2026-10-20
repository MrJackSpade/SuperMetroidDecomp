
namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Cartridge values of <see cref="TorizoBellyPaletteFxColorDefinitions"/> that only verification reads.</summary>
internal static class TorizoBellyPaletteFxColorDefinitionsConstants
{
    /// <summary>Frames from the first record through the next first record.</summary>
    public const int CycleFrames = 52;

    /// <summary>Provides the Torizo belly palette cycle period for effect timing checks.</summary>
    extension(TorizoBellyPaletteFxColorDefinitions)
    {
        /// <inheritdoc cref="TorizoBellyPaletteFxColorDefinitionsConstants.CycleFrames"/>
        internal static int CycleFrames => TorizoBellyPaletteFxColorDefinitionsConstants.CycleFrames;
    }
    /// <summary>The byte index of the first Torizo belly color in CGRAM.</summary>
    public const ushort ColorByteIndex = 0x0132;
    /// <summary>Six timed records form one complete belly-color cycle.</summary>
    public static int FrameCount => TorizoBellyPaletteFxColorDefinitions.All[0].Rows.FrameCount;
}
