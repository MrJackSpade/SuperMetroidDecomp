using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Draygon.Artwork;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// Draygon's colours (#1512): whether a hurt update last showed his material colours or their white flash, and the health
/// band last shown, with whether it was shown over a white flash. His body layer and his room's pictures, which share the
/// background colours, show the import's shades of them; his sprites show his opening's colours and his sprite shades.
/// </summary>
internal sealed class DraygonColours : IGameplayShadingSource
{
    /// <summary>The pictures that share his background colours: his room's and his body layer's.</summary>
    private static readonly GameplayPictures[] Shared = [GameplayPictures.Room, GameplayPictures.BossBody];

    /// <summary>Whether a hurt update has shown his material colours; before, his room's colours show.</summary>
    private bool hurtShown;

    /// <summary>Whether the last hurt update showed the white flash.</summary>
    private bool white;

    /// <summary>The health band last shown, or null before any.</summary>
    private int? band;

    /// <summary>Whether the band was shown after the last hurt update's white flash, so it shows over it.</summary>
    private bool bandOverWhite;

    /// <summary>The shading his sprites show: his opening's colours, with his material colours or white flash over them.</summary>
    internal PictureShading Sprites => !hurtShown
        ? PictureShading.Shaded(DraygonShades.Intro)
        : PictureShading.Shaded(DraygonShades.Intro).With(white ? DraygonShades.SpriteWhite : DraygonShades.Sprite);

    /// <summary>
    /// The shading his goop draws in: his opening's colours. Its initialization selects the second sprite row, which only
    /// his opening's colour load writes, unlike his own row, which his hurt updates rewrite.
    /// </summary>
    internal static PictureShading Goop { get; } = PictureShading.Shaded(DraygonShades.Intro);

    /// <summary>Shows a hurt update: his material colours with his band, or their white flash when <paramref name="whiteFrame"/>.</summary>
    internal void ShowHurt(bool whiteFrame)
    {
        hurtShown = true;
        white = whiteFrame;
        bandOverWhite = false;
    }

    /// <summary>Shows health band <paramref name="shown"/>'s colours.</summary>
    internal void ShowBand(int shown)
    {
        band = shown;
        bandOverWhite = hurtShown && white;
    }

    /// <inheritdoc />
    public void ShadeGameplay(GameplayShadingBuilder frame)
    {
        Ensure.NotNull(frame);
        foreach (GameplayPictures pictures in Shared)
        {
            if (hurtShown && white)
            {
                frame.Shade(pictures, DraygonShades.BackgroundWhite);
                if (bandOverWhite && band is { } over)
                    frame.Shade(pictures, DraygonShades.Health(over));
                continue;
            }
            if (hurtShown)
                frame.Shade(pictures, DraygonShades.Background);
            if (band is { } shownBand)
                frame.Shade(pictures, DraygonShades.Health(shownBand));
        }
    }
}
