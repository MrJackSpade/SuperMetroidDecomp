using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Fx.Artwork;

/// <summary>
/// The shades the import pre-colours a tileset's effect overlay pictures in for each palette blend a room-FX record selects
/// (#1512): the three blend colours its liquid, fog or rain shows, or the cleared third of the empty blend.
/// </summary>
public static class RoomFxBlendShades
{
    /// <summary>The empty blend, which clears the third blend colour to black.</summary>
    public static PictureShade None { get; } = new("room-fx-blend-none", 0);

    /// <summary>Lava and acid.</summary>
    public static PictureShade Lava { get; } = new("room-fx-blend-lava", 0);

    /// <summary>The Landing Site's rain.</summary>
    public static PictureShade LandingSiteRain { get; } = new("room-fx-blend-landing-site-rain", 0);

    /// <summary>The first Maridia water.</summary>
    public static PictureShade MaridiaWaterA { get; } = new("room-fx-blend-maridia-water-a", 0);

    /// <summary>The water and acid of Ceres and other rooms.</summary>
    public static PictureShade WaterAndAcid { get; } = new("room-fx-blend-water-and-acid", 0);

    /// <summary>Fog.</summary>
    public static PictureShade Fog { get; } = new("room-fx-blend-fog", 0);

    /// <summary>Western Maridia's water.</summary>
    public static PictureShade MaridiaWaterB { get; } = new("room-fx-blend-maridia-water-b", 0);

    /// <summary>Central Maridia's water.</summary>
    public static PictureShade MaridiaWaterC { get; } = new("room-fx-blend-maridia-water-c", 0);

    /// <summary>Eastern Maridia's water.</summary>
    public static PictureShade MaridiaWaterD { get; } = new("room-fx-blend-maridia-water-d", 0);
}
