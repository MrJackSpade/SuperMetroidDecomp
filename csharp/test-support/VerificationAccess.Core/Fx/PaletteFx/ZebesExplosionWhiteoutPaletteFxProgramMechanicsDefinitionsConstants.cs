
namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Cartridge values of <see cref="ZebesExplosionWhiteoutPaletteFxProgramMechanicsDefinitions"/> that only verification reads.</summary>
internal static class ZebesExplosionWhiteoutPaletteFxProgramMechanicsDefinitionsConstants
{
    /// <summary>The complete one-shot whiteout lasts 210 frames.</summary>
    public const int CycleFrames = 210;
    /// <summary>The space-whiteout definition at <c>$8D:E1D0</c>.</summary>
    public const ushort SpaceWhiteoutDefinitionPointer = 0xe1d0;
    /// <summary>The wide-background definition at <c>$8D:E1E8</c>.</summary>
    public const ushort WideExplosionBackgroundDefinitionPointer = 0xe1e8;

    /// <summary>Provides space whiteout and wide-explosion background definitions with whiteout cycle timing.</summary>
    extension(ZebesExplosionWhiteoutPaletteFxProgramMechanicsDefinitions)
    {
        /// <inheritdoc cref="ZebesExplosionWhiteoutPaletteFxProgramMechanicsDefinitionsConstants.CycleFrames"/>
        internal static int CycleFrames => ZebesExplosionWhiteoutPaletteFxProgramMechanicsDefinitionsConstants.CycleFrames;
        /// <inheritdoc cref="ZebesExplosionWhiteoutPaletteFxProgramMechanicsDefinitionsConstants.SpaceWhiteoutDefinitionPointer"/>
        internal static ushort SpaceWhiteoutDefinitionPointer => ZebesExplosionWhiteoutPaletteFxProgramMechanicsDefinitionsConstants.SpaceWhiteoutDefinitionPointer;
        /// <inheritdoc cref="ZebesExplosionWhiteoutPaletteFxProgramMechanicsDefinitionsConstants.WideExplosionBackgroundDefinitionPointer"/>
        internal static ushort WideExplosionBackgroundDefinitionPointer => ZebesExplosionWhiteoutPaletteFxProgramMechanicsDefinitionsConstants.WideExplosionBackgroundDefinitionPointer;
    }
}
