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
    /// <summary>Whether a hurt update has shown his material colours; before, his room's colours show.</summary>
    private bool hurtShown;

    /// <summary>Whether the last hurt update showed the white flash.</summary>
    private bool white;

    /// <summary>His health band, or null before any is shown.</summary>
    private int? band;

    /// <summary>The shading his sprites show: his opening's colours, with his material colours or white flash over them.</summary>
    internal PictureShading Sprites => !hurtShown
        ? PictureShading.Shaded(DraygonShades.Intro)
        : PictureShading.Shaded(DraygonShades.Intro).With(white ? DraygonShades.SpriteWhite : DraygonShades.Sprite);

    /// <summary>
    /// The shading his goop draws in: his opening's colours. Its initialization selects the second sprite row, which only
    /// his opening's colour load writes, unlike his own row, which his hurt updates rewrite.
    /// </summary>
    internal static PictureShading Goop { get; } = PictureShading.Shaded(DraygonShades.Intro);

    /// <summary>
    /// The shading the Evirs of his opening and his burial draw in: his material colours, which they keep whatever his hurt
    /// updates show (#1544).
    /// </summary>
    internal static PictureShading Evirs { get; } = PictureShading.Shaded(DraygonShades.Sprite);

    /// <summary>Shows a hurt update: his material colours with his band, or their white flash when <paramref name="whiteFrame"/>.</summary>
    internal void ShowHurt(bool whiteFrame)
    {
        hurtShown = true;
        white = whiteFrame;
    }

    /// <summary>Shows health band <paramref name="shown"/>'s colours with his material colours; his white flash covers them.</summary>
    internal void ShowBand(int shown) => band = shown;

    /// <inheritdoc />
    public void ShadeGameplay(GameplayShadingBuilder frame)
    {
        Ensure.NotNull(frame);
        if (hurtShown && white)
        {
            frame.Shade(GameplayPictures.BossBody, DraygonShades.BackgroundWhite);
            return;
        }
        if (hurtShown)
            frame.Shade(GameplayPictures.BossBody, DraygonShades.Background);
        if (band is { } shownBand)
            frame.Shade(GameplayPictures.BossBody, DraygonShades.Health(shownBand));
    }
}
