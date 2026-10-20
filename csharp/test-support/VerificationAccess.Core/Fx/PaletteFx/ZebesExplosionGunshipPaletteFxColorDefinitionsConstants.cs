
namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Cartridge values of <see cref="ZebesExplosionGunshipPaletteFxColorDefinitions"/> that only verification reads.</summary>
internal static class ZebesExplosionGunshipPaletteFxColorDefinitionsConstants
{
    /// <summary>Each record lasts 24 frames.</summary>
    public const ushort FrameDuration = 0x18;
    /// <summary>The palette-FX definition at <c>$8D:E1E4</c>.</summary>
    public const ushort DefinitionPointer = 0xe1e4;

    /// <summary>Provides the gunship palette definition and cycle period during the Zebes explosion.</summary>
    extension(ZebesExplosionGunshipPaletteFxColorDefinitions)
    {
        /// <summary>The complete gunship reveal lasts 384 frames.</summary>
        internal static int CycleFrames => ZebesExplosionGunshipPaletteFxColorDefinitions.FrameCount * ZebesExplosionGunshipPaletteFxColorDefinitionsConstants.FrameDuration;
        /// <inheritdoc cref="ZebesExplosionGunshipPaletteFxColorDefinitionsConstants.DefinitionPointer"/>
        internal static ushort DefinitionPointer => ZebesExplosionGunshipPaletteFxColorDefinitionsConstants.DefinitionPointer;
    }
    /// <summary>The instruction-list entry at <c>$8D:D6BA</c>.</summary>
    public const ushort ProgramStart = 0xd6ba;
    /// <summary>The reveal writes CGRAM from byte index <c>$00A0</c>.</summary>
    public const ushort ColorByteIndex = 0x00a0;
}
