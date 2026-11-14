using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Fx.Artwork;

/// <summary>
/// The shades the import pre-colours a tileset's effect overlay pictures in for each palette blend a room effect selects
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

    /// <summary>The indigo Maridia water.</summary>
    public static PictureShade IndigoMaridiaWater { get; } = new("room-fx-blend-indigo-maridia-water", 0);

    /// <summary>The water and acid of Ceres and other rooms.</summary>
    public static PictureShade WaterAndAcid { get; } = new("room-fx-blend-water-and-acid", 0);

    /// <summary>Fog.</summary>
    public static PictureShade Fog { get; } = new("room-fx-blend-fog", 0);

    /// <summary>Western Maridia's teal water.</summary>
    public static PictureShade TealMaridiaWater { get; } = new("room-fx-blend-teal-maridia-water", 0);

    /// <summary>Central Maridia's green water.</summary>
    public static PictureShade GreenMaridiaWater { get; } = new("room-fx-blend-green-maridia-water", 0);

    /// <summary>Eastern Maridia's purple water.</summary>
    public static PictureShade PurpleMaridiaWater { get; } = new("room-fx-blend-purple-maridia-water", 0);
}
