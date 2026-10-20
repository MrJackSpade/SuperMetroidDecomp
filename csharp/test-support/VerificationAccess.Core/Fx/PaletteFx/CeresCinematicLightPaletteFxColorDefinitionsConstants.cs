
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
    /// <summary><c>PalFxInstList_CutsceneGunshipEngine</c> at <c>$8D:C87A</c>.</summary>
    /// <remarks>
    /// The $8D:E1A8 definition selects a bounded two-record control loop.
    /// $C87A sets CGRAM word index $00BE; timed records start at
    /// $C87E + 6 * frame for frame 0..1. Each record has duration one,
    /// one live BGR555 color word, and a $C595 wait. The $C61E goto at
    /// $C88A targets $C87E, yielding a two-frame cycle without entering
    /// the adjacent $C88E program. All eight mechanics words match the
    /// pinned NTSC J/U v1.0 ROM; the two colors stay presentation-owned.
    /// </remarks>
    public const ushort GunshipEngineProgramStart = 0xc87a;
    /// <summary>The gunship engine writes CGRAM word index <c>$00BE</c>.</summary>
    public const ushort GunshipEngineColorIndex = 0x00be;
    /// <summary><c>PalFxInstList_CutsceneCeresNavigationLightsSprite</c> at <c>$8D:C88E</c>.</summary>
    public const ushort SpriteNavigationLightsProgramStart = 0xc88e;
    /// <summary>The sprite-Ceres navigation lights write CGRAM word index <c>$01DA</c>.</summary>
    public const ushort SpriteNavigationLightsColorIndex = 0x01da;
    /// <summary><c>PalFxInstList_CutsceneCeresNavigationLightsBg</c> at <c>$8D:C906</c>.</summary>
    public const ushort BackgroundNavigationLightsProgramStart = 0xc906;
    /// <summary>The background-Ceres navigation lights write CGRAM word index <c>$00DA</c>.</summary>
    public const ushort BackgroundNavigationLightsColorIndex = 0x00da;
    /// <summary>The gunship-engine loop contains two timed records.</summary>
    public const int GunshipEngineFrameCount = 2;
    /// <summary>The shared Ceres navigation-light loop contains fourteen timed records.</summary>
    public const int NavigationLightsFrameCount = 14;
}
