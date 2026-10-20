
namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Cartridge values of <see cref="TourianGlowPaletteFxColorDefinitions"/> that only verification reads.</summary>
internal static class TourianGlowPaletteFxColorDefinitionsConstants
{
    /// <summary>The unused Tourian 4 clone definition.</summary>
    public const ushort CloneDefinitionPointer = 0xf7a5;
    /// <summary>Frames from the first record through the next first record.</summary>
    public const int CycleFrames = 110;
    /// <summary>The live Tourian 2 definition.</summary>
    public const ushort LiveDefinitionPointer = 0xf7a1;

    /// <summary>Provides live and cloned Tourian glow definitions and their cycle period.</summary>
    extension(TourianGlowPaletteFxColorDefinitions)
    {
        /// <inheritdoc cref="TourianGlowPaletteFxColorDefinitionsConstants.CloneDefinitionPointer"/>
        internal static ushort CloneDefinitionPointer => TourianGlowPaletteFxColorDefinitionsConstants.CloneDefinitionPointer;
        /// <inheritdoc cref="TourianGlowPaletteFxColorDefinitionsConstants.CycleFrames"/>
        internal static int CycleFrames => TourianGlowPaletteFxColorDefinitionsConstants.CycleFrames;
        /// <inheritdoc cref="TourianGlowPaletteFxColorDefinitionsConstants.LiveDefinitionPointer"/>
        internal static ushort LiveDefinitionPointer => TourianGlowPaletteFxColorDefinitionsConstants.LiveDefinitionPointer;
    }
    /// <summary>The first destination byte in CGRAM.</summary>
    public const ushort ColorByteIndex = 0x00e8;
    /// <summary>Colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
    public static PaletteFxColorKey Color(int frame, int color) => TourianGlowPaletteFxColorDefinitions.Rows.Color(frame, color);
    /// <summary>Eleven ten-frame records form one complete glow cycle.</summary>
    public static int FrameCount => TourianGlowPaletteFxColorDefinitions.Rows.FrameCount;
}
