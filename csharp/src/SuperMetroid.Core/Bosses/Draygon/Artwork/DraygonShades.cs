using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Draygon.Artwork;

/// <summary>
/// The shades the import pre-colours Draygon's sheets and his room's pictures in (#1512): the colours his opening loads,
/// his material colours and their white hurt flash on his body layer, his room's background and his sprites, and his
/// eight health bands.
/// </summary>
public static class DraygonShades
{
    /// <summary>The colours his opening loads for his goop and the sprites of his room.</summary>
    public static PictureShade Intro { get; } = new("draygon-intro", 0);

    /// <summary>His material colours on his body layer and his room's background.</summary>
    public static PictureShade Background { get; } = new("draygon-background", 0);

    /// <summary>The white hurt flash on his body layer and his room's background.</summary>
    public static PictureShade BackgroundWhite { get; } = new("draygon-background-white", 0);

    /// <summary>His sprites' material colours.</summary>
    public static PictureShade Sprite { get; } = new("draygon-sprite", 0);

    /// <summary>His sprites' white hurt flash.</summary>
    public static PictureShade SpriteWhite { get; } = new("draygon-sprite-white", 0);

    /// <summary>Health band <paramref name="band"/>'s colours, zero through seven, on his body layer and his room's background.</summary>
    public static PictureShade Health(int band) => new("draygon-health", band);
}
