using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Draygon.Artwork;

/// <summary>
/// The shades the import pre-colours Draygon's sheets in (#1512): his opening's look, his body's colours and their
/// white hurt flash on his body and his sprites, and his health bands.
/// </summary>
public static class DraygonShades
{
    /// <summary>The look his goop and his room's sprites show during his opening.</summary>
    public static PictureShade Intro { get; } = new("draygon-intro", 0);

    /// <summary>His body's material colours (#2221).</summary>
    public static PictureShade Body { get; } = new("draygon-body", 0);

    /// <summary>His body's white hurt flash (#2221).</summary>
    public static PictureShade BodyWhite { get; } = new("draygon-body-white", 0);

    /// <summary>His sprites' material colours.</summary>
    public static PictureShade Sprite { get; } = new("draygon-sprite", 0);

    /// <summary>His sprites' white hurt flash.</summary>
    public static PictureShade SpriteWhite { get; } = new("draygon-sprite-white", 0);

    /// <summary><paramref name="band"/>'s colours on his body layer (#2266).</summary>
    public static PictureShade Health(DraygonHealthBand band) => new($"draygon-health-{Ensure.NotNull(band).Key}", 0);
}
