using SuperMetroid.Core.Game;

/// <summary>Cartridge values of <see cref="ZebesExplosionGunshipPaletteFxProgramMechanicsDefinitions"/> that only verification reads.</summary>
internal static class ZebesExplosionGunshipPaletteFxProgramMechanicsDefinitionsConstants
{
    /// <summary>The complete gunship reveal lasts 384 frames.</summary>
    public const int CycleFrames = ZebesExplosionGunshipPaletteFxProgramMechanicsDefinitions.FrameCount * ZebesExplosionGunshipPaletteFxProgramMechanicsDefinitions.FrameDuration;
    /// <summary>The palette-FX definition at <c>$8D:E1E4</c>.</summary>
    public const ushort DefinitionPointer = 0xe1e4;

    /// <summary>Provides the gunship palette definition and cycle period during the Zebes explosion.</summary>
    extension(ZebesExplosionGunshipPaletteFxProgramMechanicsDefinitions)
    {
        /// <inheritdoc cref="ZebesExplosionGunshipPaletteFxProgramMechanicsDefinitionsConstants.CycleFrames"/>
        internal static int CycleFrames => ZebesExplosionGunshipPaletteFxProgramMechanicsDefinitionsConstants.CycleFrames;
        /// <inheritdoc cref="ZebesExplosionGunshipPaletteFxProgramMechanicsDefinitionsConstants.DefinitionPointer"/>
        internal static ushort DefinitionPointer => ZebesExplosionGunshipPaletteFxProgramMechanicsDefinitionsConstants.DefinitionPointer;
    }
}
