
namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Cartridge values of <see cref="ZebesExplosionGunshipPaletteFxColorDefinitions"/> that only verification reads.</summary>
internal static class ZebesExplosionGunshipPaletteFxColorDefinitionsConstants
{
    /// <summary>The complete gunship reveal lasts 384 frames.</summary>
    public const int CycleFrames = ZebesExplosionGunshipPaletteFxColorDefinitions.FrameCount * ZebesExplosionGunshipPaletteFxColorDefinitions.FrameDuration;
    /// <summary>The palette-FX definition at <c>$8D:E1E4</c>.</summary>
    public const ushort DefinitionPointer = 0xe1e4;

    /// <summary>Provides the gunship palette definition and cycle period during the Zebes explosion.</summary>
    extension(ZebesExplosionGunshipPaletteFxColorDefinitions)
    {
        /// <inheritdoc cref="ZebesExplosionGunshipPaletteFxColorDefinitionsConstants.CycleFrames"/>
        internal static int CycleFrames => ZebesExplosionGunshipPaletteFxColorDefinitionsConstants.CycleFrames;
        /// <inheritdoc cref="ZebesExplosionGunshipPaletteFxColorDefinitionsConstants.DefinitionPointer"/>
        internal static ushort DefinitionPointer => ZebesExplosionGunshipPaletteFxColorDefinitionsConstants.DefinitionPointer;
    }
}
