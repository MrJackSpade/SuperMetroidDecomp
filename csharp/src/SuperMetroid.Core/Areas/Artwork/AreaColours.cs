using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Areas.Artwork;

/// <summary>Typed view of the area set pieces' colours (#630, #1395, #1405): runs of gameplay's background and sprite colour sets.</summary>
public readonly struct AreaColours(GameplayColours colours)
{
    /// <summary>Tourian statue base.</summary>
    public ColourRun TourianStatueBase => AreaColourDefinitions.TourianStatueBase.In(colours.SpriteSets);
    /// <summary>Tourian statue.</summary>
    public ColourRun TourianStatue => AreaColourDefinitions.TourianStatue.In(colours.SpriteSets);
    /// <summary>Tourian statue eye.</summary>
    public ColourRun TourianStatueEye => AreaColourDefinitions.TourianStatueEye.In(colours.SpriteSets);
}
