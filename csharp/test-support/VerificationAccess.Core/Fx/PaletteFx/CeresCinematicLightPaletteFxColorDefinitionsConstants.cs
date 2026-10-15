
namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Cartridge values of <see cref="CeresCinematicLightPaletteFxColorDefinitions"/> that only verification reads.</summary>
internal static class CeresCinematicLightPaletteFxColorDefinitionsConstants
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
    extension(CeresCinematicLightPaletteFxColorDefinitions)
    {
        /// <inheritdoc cref="CeresCinematicLightPaletteFxColorDefinitionsConstants.BackgroundNavigationLightsDefinitionPointer"/>
        internal static ushort BackgroundNavigationLightsDefinitionPointer => CeresCinematicLightPaletteFxColorDefinitionsConstants.BackgroundNavigationLightsDefinitionPointer;
        /// <inheritdoc cref="CeresCinematicLightPaletteFxColorDefinitionsConstants.GunshipEngineCycleFrames"/>
        internal static int GunshipEngineCycleFrames => CeresCinematicLightPaletteFxColorDefinitionsConstants.GunshipEngineCycleFrames;
        /// <inheritdoc cref="CeresCinematicLightPaletteFxColorDefinitionsConstants.GunshipEngineDefinitionPointer"/>
        internal static ushort GunshipEngineDefinitionPointer => CeresCinematicLightPaletteFxColorDefinitionsConstants.GunshipEngineDefinitionPointer;
        /// <inheritdoc cref="CeresCinematicLightPaletteFxColorDefinitionsConstants.NavigationLightsCycleFrames"/>
        internal static int NavigationLightsCycleFrames => CeresCinematicLightPaletteFxColorDefinitionsConstants.NavigationLightsCycleFrames;
        /// <inheritdoc cref="CeresCinematicLightPaletteFxColorDefinitionsConstants.SpriteNavigationLightsDefinitionPointer"/>
        internal static ushort SpriteNavigationLightsDefinitionPointer => CeresCinematicLightPaletteFxColorDefinitionsConstants.SpriteNavigationLightsDefinitionPointer;
    }
}
