using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Draygon.Artwork;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// Draygon's colours (#1512, #1544, #1597): whether a hurt update last showed his material colours or their white flash,
/// and his health band, which his material colours show and his white flash covers. His body layer shows the import's
/// shades of them, and his sprites show his opening's colours and his sprite shades. His room and the Evirs he spawns show
/// their own colours, which his hurt updates do not change.
/// </summary>
internal sealed class DraygonColours : IGameplayShadingSource
{
    /// <summary>What his last hurt update showed (#2560): his room's colours before any, then his material colours or their white flash.</summary>
    private DraygonBodyColours shown;

    /// <summary>His health band, or null before any is shown.</summary>
    private DraygonHealthBand? band;

    /// <summary>The shading his sprites show: his opening's colours, with his material colours or white flash over them.</summary>
    internal PictureShading Sprites => shown switch
    {
        DraygonBodyColours.Room => PictureShading.Shaded(DraygonShades.Intro),
        DraygonBodyColours.Material => PictureShading.Shaded(DraygonShades.Intro).With(DraygonShades.Sprite),
        DraygonBodyColours.WhiteFlash => PictureShading.Shaded(DraygonShades.Intro).With(DraygonShades.SpriteWhite),
        _ => throw new InvalidOperationException($"Undefined Draygon colours {shown}."),
    };

    /// <summary>The shading his goop draws in: his opening's colours, which his hurt updates never change.</summary>
    internal static PictureShading Goop { get; } = PictureShading.Shaded(DraygonShades.Intro);

    /// <summary>
    /// The shading the Evirs of his opening and his burial draw in: his material colours, which they keep whatever his hurt
    /// updates show (#1544).
    /// </summary>
    internal static PictureShading Evirs { get; } = PictureShading.Shaded(DraygonShades.Sprite);

    /// <summary>Shows a hurt update's <paramref name="colours"/>: his material colours with his band, or their white flash.</summary>
    internal void ShowHurt(DraygonBodyColours colours) => shown = colours;

    /// <summary>Shows <paramref name="shown"/>'s colours with his material colours; his white flash covers them.</summary>
    internal void ShowBand(DraygonHealthBand shown) => band = Ensure.NotNull(shown);

    /// <inheritdoc />
    public void ShadeGameplay(GameplayShadingBuilder frame)
    {
        Ensure.NotNull(frame);
        if (shown == DraygonBodyColours.WhiteFlash)
        {
            frame.Shade(GameplayPictures.BossBody, DraygonShades.BodyWhite);
            return;
        }
        if (shown == DraygonBodyColours.Material)
            frame.Shade(GameplayPictures.BossBody, DraygonShades.Body);
        if (band is { } shownBand)
            frame.Shade(GameplayPictures.BossBody, DraygonShades.Health(shownBand));
    }
}
