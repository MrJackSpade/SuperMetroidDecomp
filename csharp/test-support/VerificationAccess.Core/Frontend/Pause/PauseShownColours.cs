using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Frontend.Pause.Artwork;

namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>
/// The cartridge-ordered 256-colour image the pause screen's pictures show (#1512): the pause colours the import read,
/// with the highlight shade the map's highlighted sprites name once the cycle has applied a frame while they are drawn, and the reserve arrow's
/// shade, laid over the inks the import pre-colours them on. The screen keeps no such image; verification composes it from the shadings the screen publishes
/// and the colours the import installs for them, to compare against native colour memory.
/// </summary>
internal static class PauseShownColours
{
    /// <summary>The first colour of the sprite colour group the highlight cycle recolours.</summary>
    private const int HighlightFirstColour = 128 + 3 * 16;
    /// <summary>The colour the reserve arrow's darker bevel shows.</summary>
    private const int ArrowShadeColour = 6 * 16 + 6;
    /// <summary>The colour the reserve arrow's lighter bevel shows.</summary>
    private const int ArrowLightColour = 6 * 16 + 11;

    /// <summary>The colours <paramref name="pause"/>'s pictures show now.</summary>
    /// <exception cref="InvalidOperationException">The pause screen's presentation was not read from an installation.</exception>
    internal static RgbColor[] Of(PauseMenuState pause)
    {
        Ensure.NotNull(pause);
        AreaMapPresentation maps = pause.screen.Artwork;
        RgbColor[] colours = ImportAreaMapPresentation.PalettesOf(maps).Pause.ToArray();
        // Only the map's highlighted sprites show the cycle (#1544): the equipment page's selector and reserve strip,
        // which the cartridge draws in the same colour group, show their own colours.
        if (pause.paletteAnimation.ShownFrame is int frame && pause.CaptureRenderSnapshot().SpriteDraws.OfType<ImageSpriteDraw>()
                .Any(draw => draw.Shading.Shades.Contains(MapSpriteShades.Highlight(frame))))
            InstalledAreaMapPresentation.HighlightColoursOf(maps).Colours(frame).CopyTo(colours.AsSpan(HighlightFirstColour));
        PictureShading arrow = pause.ReserveArrowShading;
        if (!arrow.IsStandard)
        {
            PauseReserveArrowColours arrowColours = InstalledAreaMapPresentation.PauseOf(maps).ReserveUi.ArrowColours;
            PictureShade shade = arrow.Shades.Single();
            ReserveArrowColors shown = shade == PauseReserveArrowShades.Solid ? arrowColours.Solid : arrowColours.Pulse(shade.Step);
            colours[ArrowShadeColour] = shown.Color6;
            colours[ArrowLightColour] = shown.Color11;
        }
        return colours;
    }
}
