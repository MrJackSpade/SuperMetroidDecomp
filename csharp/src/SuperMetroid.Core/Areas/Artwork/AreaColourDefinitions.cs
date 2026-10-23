using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Areas.Artwork;

/// <summary>The colour runs the <see cref="AreaColours"/> view names (#1405).</summary>
public static class AreaColourDefinitions
{
    /// <summary>Tourian statue base.</summary>
    public static readonly SpriteColourRun TourianStatueBase = new(7, 0, 16);

    /// <summary>Tourian statue.</summary>
    public static readonly SpriteColourRun TourianStatue = new(2, 0, 16);

    /// <summary>Tourian statue eye.</summary>
    public static readonly SpriteColourRun TourianStatueEye = new(7, 9, 4);
}
