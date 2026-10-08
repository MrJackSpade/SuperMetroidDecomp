namespace SuperMetroid.Core.Game;

/// <summary>Identifies one compiled Ceres cinematic-light palette program.</summary>
public enum CeresCinematicLightPaletteFxProgramOwner
{
    /// <summary>The cutscene gunship engine flicker.</summary>
    GunshipEngine,
    /// <summary>The navigation lights on sprite Ceres.</summary>
    SpriteNavigationLights,
    /// <summary>The navigation lights on background Ceres.</summary>
    BackgroundNavigationLights,
}

/// <summary>Immutable entry metadata for one Ceres cinematic-light palette program.</summary>
public readonly record struct CeresCinematicLightPaletteFxProgramDefinition();
