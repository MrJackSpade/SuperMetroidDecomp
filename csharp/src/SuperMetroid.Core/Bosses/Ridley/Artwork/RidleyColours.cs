using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Bosses.Ridley.Artwork;

/// <summary>Typed view of Ridley's colours (#630, #1405): runs of gameplay's background and sprite colour sets.</summary>
public readonly struct RidleyColours(GameplayColours colours)
{
    /// <summary>Ceres door/container and Baby palettes.</summary>
    public ColourRun CeresStart => RidleyColourDefinitions.CeresStart.In(colours.SpriteSets);
    /// <summary>Ceres eye colours.</summary>
    public ColourRun CeresEyeFade => RidleyColourDefinitions.CeresEyeFade.In(colours.SpriteSets);
    /// <summary>Ceres body fade, OBJ palette 1 copy.</summary>
    public ColourRun CeresBodyFadeShadow => RidleyColourDefinitions.CeresBodyFadeShadow.In(colours.SpriteSets);
    /// <summary>Ceres body fade, OBJ palette 7.</summary>
    public ColourRun CeresBodyFade => RidleyColourDefinitions.CeresBodyFade.In(colours.SpriteSets);
    /// <summary>Shared health shade.</summary>
    public ColourRun Health => RidleyColourDefinitions.Health.In(colours.SpriteSets);
    /// <summary>Ceres alarm text.</summary>
    public ColourRun CeresAlarm => RidleyColourDefinitions.CeresAlarm.In(colours.BackgroundSets);
    /// <summary>Ceres getaway backdrop.</summary>
    public ColourRun CeresRetreatBackground => RidleyColourDefinitions.CeresRetreatBackground.In(colours.BackgroundSets);
    /// <summary>Ceres shared retreat shade, BG copy.</summary>
    public ColourRun CeresRetreatSharedBackground => RidleyColourDefinitions.CeresRetreatSharedBackground.In(colours.BackgroundSets);
    /// <summary>Ceres shared retreat shade, OBJ copy.</summary>
    public ColourRun CeresRetreatSharedObject => RidleyColourDefinitions.CeresRetreatSharedObject.In(colours.SpriteSets);
    /// <summary>Ceres Baby shade.</summary>
    public ColourRun CeresBaby => RidleyColourDefinitions.CeresBaby.In(colours.SpriteSets);
    /// <summary>Ceres Mode 7 zoom shade.</summary>
    public ColourRun CeresMode7Zoom => RidleyColourDefinitions.CeresMode7Zoom.In(colours.BackgroundSets);
    /// <summary>Norfair initial palettes.</summary>
    public ColourRun NorfairInitial => RidleyColourDefinitions.NorfairInitial.In(colours.SpriteSets);
    /// <summary>Norfair reveal fade.</summary>
    public ColourRun NorfairReveal => RidleyColourDefinitions.NorfairReveal.In(colours.BackgroundSets);
}
