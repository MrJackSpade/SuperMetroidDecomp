using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Bosses.Ridley.Artwork;

/// <summary>The colour runs the <see cref="RidleyColours"/> view names (#1405).</summary>
public static class RidleyColourDefinitions
{
    /// <summary>Ceres door/container and Baby palettes.</summary>
    public static readonly SpriteColourRun CeresStart = new(2, 0, 32);

    /// <summary>Ceres eye colours.</summary>
    public static readonly SpriteColourRun CeresEyeFade = new(7, 12, 3);

    /// <summary>Ceres body fade, OBJ palette 1 copy.</summary>
    public static readonly SpriteColourRun CeresBodyFadeShadow = new(1, 1, 11);

    /// <summary>Ceres body fade, OBJ palette 7.</summary>
    public static readonly SpriteColourRun CeresBodyFade = new(7, 1, 11);

    /// <summary>Shared health shade.</summary>
    public static readonly SpriteColourRun Health = new(7, 1, 14);

    /// <summary>Ceres alarm text.</summary>
    public static readonly BackgroundColourRun CeresAlarm = new(6, 1, 3);

    /// <summary>Ceres getaway backdrop.</summary>
    public static readonly BackgroundColourRun CeresRetreatBackground = new(5, 1, 15);

    /// <summary>Ceres shared retreat shade, BG copy.</summary>
    public static readonly BackgroundColourRun CeresRetreatSharedBackground = new(2, 1, 8);

    /// <summary>Ceres shared retreat shade, OBJ copy.</summary>
    public static readonly SpriteColourRun CeresRetreatSharedObject = new(7, 1, 8);

    /// <summary>Ceres Baby shade.</summary>
    public static readonly SpriteColourRun CeresBaby = new(3, 1, 15);

    /// <summary>Ceres Mode 7 zoom shade.</summary>
    public static readonly BackgroundColourRun CeresFloorZoom = new(5, 1, 15);

    /// <summary>Norfair initial palettes.</summary>
    public static readonly SpriteColourRun NorfairInitial = new(2, 0, 32);

    /// <summary>Norfair reveal fade.</summary>
    public static readonly BackgroundColourRun NorfairReveal = new(7, 1, 14);
}
