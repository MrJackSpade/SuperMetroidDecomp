
namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Cartridge values of <see cref="ZebesExplosionWhiteoutPaletteFxColorDefinitions"/> that only verification reads.</summary>
internal static class ZebesExplosionWhiteoutPaletteFxColorDefinitionsConstants
{
    /// <summary>The complete one-shot whiteout lasts 210 frames.</summary>
    public const int CycleFrames = 210;
    /// <summary>The space-whiteout definition at <c>$8D:E1D0</c>.</summary>
    public const ushort SpaceWhiteoutDefinitionPointer = 0xe1d0;
    /// <summary>The wide-background definition at <c>$8D:E1E8</c>.</summary>
    public const ushort WideExplosionBackgroundDefinitionPointer = 0xe1e8;

    /// <summary>Provides space whiteout and wide-explosion background definitions with whiteout cycle timing.</summary>
    extension(ZebesExplosionWhiteoutPaletteFxColorDefinitions)
    {
        /// <inheritdoc cref="ZebesExplosionWhiteoutPaletteFxColorDefinitionsConstants.CycleFrames"/>
        internal static int CycleFrames => ZebesExplosionWhiteoutPaletteFxColorDefinitionsConstants.CycleFrames;
        /// <inheritdoc cref="ZebesExplosionWhiteoutPaletteFxColorDefinitionsConstants.SpaceWhiteoutDefinitionPointer"/>
        internal static ushort SpaceWhiteoutDefinitionPointer => ZebesExplosionWhiteoutPaletteFxColorDefinitionsConstants.SpaceWhiteoutDefinitionPointer;
        /// <inheritdoc cref="ZebesExplosionWhiteoutPaletteFxColorDefinitionsConstants.WideExplosionBackgroundDefinitionPointer"/>
        internal static ushort WideExplosionBackgroundDefinitionPointer => ZebesExplosionWhiteoutPaletteFxColorDefinitionsConstants.WideExplosionBackgroundDefinitionPointer;
    }
}
