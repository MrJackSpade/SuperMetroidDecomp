
namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Cartridge values of <see cref="RedBrinstarGlowPaletteFxColorDefinitions"/> that only verification reads.</summary>
internal static class RedBrinstarGlowPaletteFxColorDefinitionsConstants
{
    /// <summary>Frames from the first record through the next first record.</summary>
    public const int CycleFrames = 140;
    /// <summary>Red Brinstar background-glow palette-FX definition at $8D:F77D.</summary>
    public const ushort DefinitionPointer = 0xf77d;

    /// <summary>Provides Red Brinstar glow palette definition and cycle period for environmental checks.</summary>
    extension(RedBrinstarGlowPaletteFxColorDefinitions)
    {
        /// <inheritdoc cref="RedBrinstarGlowPaletteFxColorDefinitionsConstants.CycleFrames"/>
        internal static int CycleFrames => RedBrinstarGlowPaletteFxColorDefinitionsConstants.CycleFrames;
        /// <inheritdoc cref="RedBrinstarGlowPaletteFxColorDefinitionsConstants.DefinitionPointer"/>
        internal static ushort DefinitionPointer => RedBrinstarGlowPaletteFxColorDefinitionsConstants.DefinitionPointer;
    }
    /// <summary>The byte index of the first Red Brinstar glow color in CGRAM.</summary>
    public const ushort ColorByteIndex = 0x00c8;
}
