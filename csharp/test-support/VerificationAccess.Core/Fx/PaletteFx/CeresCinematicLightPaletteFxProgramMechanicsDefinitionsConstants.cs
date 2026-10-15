
namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Cartridge values of <see cref="CeresCinematicLightPaletteFxProgramMechanicsDefinitions"/> that only verification reads.</summary>
internal static class CeresCinematicLightPaletteFxProgramMechanicsDefinitionsConstants
{
    /// <summary><c>PalFxDef_CutsceneCeresNavigationLightsBg</c> at <c>$8D:E1B8</c>.</summary>
    public const ushort BackgroundNavigationLightsDefinitionPointer = 0xe1b8;
    /// <summary>The complete gunship-engine loop lasts two frames.</summary>
    public const int GunshipEngineCycleFrames = 2;
    /// <summary><c>PalFxDef_CutsceneGunshipEngine</c> at <c>$8D:E1A8</c>.</summary>
    public const ushort GunshipEngineDefinitionPointer = 0xe1a8;
    /// <summary>The complete Ceres navigation-light loop lasts 56 frames.</summary>
    public const int NavigationLightsCycleFrames = 56;
    /// <summary><c>PalFxDef_CutsceneCeresNavigationLightsSprite</c> at <c>$8D:E1AC</c>.</summary>
    public const ushort SpriteNavigationLightsDefinitionPointer = 0xe1ac;

    /// <summary>Provides native Ceres navigation light pointers and cycle periods for cinematic palette checks.</summary>
    extension(CeresCinematicLightPaletteFxProgramMechanicsDefinitions)
    {
        /// <inheritdoc cref="CeresCinematicLightPaletteFxProgramMechanicsDefinitionsConstants.BackgroundNavigationLightsDefinitionPointer"/>
        internal static ushort BackgroundNavigationLightsDefinitionPointer => CeresCinematicLightPaletteFxProgramMechanicsDefinitionsConstants.BackgroundNavigationLightsDefinitionPointer;
        /// <inheritdoc cref="CeresCinematicLightPaletteFxProgramMechanicsDefinitionsConstants.GunshipEngineCycleFrames"/>
        internal static int GunshipEngineCycleFrames => CeresCinematicLightPaletteFxProgramMechanicsDefinitionsConstants.GunshipEngineCycleFrames;
        /// <inheritdoc cref="CeresCinematicLightPaletteFxProgramMechanicsDefinitionsConstants.GunshipEngineDefinitionPointer"/>
        internal static ushort GunshipEngineDefinitionPointer => CeresCinematicLightPaletteFxProgramMechanicsDefinitionsConstants.GunshipEngineDefinitionPointer;
        /// <inheritdoc cref="CeresCinematicLightPaletteFxProgramMechanicsDefinitionsConstants.NavigationLightsCycleFrames"/>
        internal static int NavigationLightsCycleFrames => CeresCinematicLightPaletteFxProgramMechanicsDefinitionsConstants.NavigationLightsCycleFrames;
        /// <inheritdoc cref="CeresCinematicLightPaletteFxProgramMechanicsDefinitionsConstants.SpriteNavigationLightsDefinitionPointer"/>
        internal static ushort SpriteNavigationLightsDefinitionPointer => CeresCinematicLightPaletteFxProgramMechanicsDefinitionsConstants.SpriteNavigationLightsDefinitionPointer;
    }
}
