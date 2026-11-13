using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Frontend.Pause.Artwork;

namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>
/// The cartridge-ordered 256-colour image the pause screen's pictures show (#1512): the pause colours the import read,
/// with the highlight shade the map's highlighted sprites name while they are drawn, and the reserve arrow's
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

    /// <summary>The colours of the sprite colour group the highlight cycle recolours.</summary>
    private const int HighlightColours = 16;

    /// <summary>The highlight frame <paramref name="pause"/>'s highlighted map sprites show, or null when none is drawn.</summary>
    private static int? ShownHighlight(PauseMenuState pause)
    {
        int frame = pause.mapHighlight.Frame;
        return pause.CaptureRenderSnapshot().SpriteDraws.OfType<ImageSpriteDraw>()
            .Any(draw => Ensure.NotNull(draw.Shading).Shades.Contains(MapSpriteShades.Highlight(frame)))
            ? frame : null;
    }

    /// <summary>
    /// The native colours <see cref="Of"/> composes that the pause screen's pictures show: every colour, but for the
    /// highlight cycle's colour group while no highlighted map sprite is drawn.
    /// </summary>
    /// <remarks>
    /// The cartridge cycles that group whenever the pause screen runs, and the equipment page's selector and reserve
    /// strip drew in it and glowed with the map's highlight. That sharing was a presentation quirk (#1544): they show
    /// their own colours, so the native colours of the group say nothing about them.
    /// <para>
    /// The reserve arrow's two colours are never compared either. The cartridge writes them only on the updates the
    /// equipment page takes input in the reserve rows, so they hold whatever was last written: the solid look the page's
    /// setup wrote, or a pulse step from before a fade, a page change or the cursor's move. That latched look was a
    /// presentation quirk (#1597): the port's arrow shows the cursor's current state, which the reserve arrow's own checks
    /// confirm, so the arrow's native colours say nothing about it.
    /// </para>
    /// </remarks>
    internal static IReadOnlyList<int> ComparedIn(PauseMenuState pause)
    {
        Ensure.NotNull(pause);
        bool highlighted = ShownHighlight(pause) is not null;
        return [.. Enumerable.Range(0, SnesCgram.ColorCount)
            .Where(colour => highlighted || colour is < HighlightFirstColour or >= HighlightFirstColour + HighlightColours)
            .Where(colour => colour is not (ArrowShadeColour or ArrowLightColour))];
    }

    /// <summary>The colours <paramref name="pause"/>'s pictures show now.</summary>
    /// <exception cref="InvalidOperationException">The pause screen's presentation was not read from an installation.</exception>
    internal static RgbColor[] Of(PauseMenuState pause)
    {
        Ensure.NotNull(pause);
        AreaMapPresentation maps = pause.screen.Artwork;
        // The pause colours are import knowledge (#1544): the game reads no palette document, so they come from the import's read.
        RgbColor[] colours = InstalledFixtures.ImportMaps.Palettes.Pause.ToArray();
        // Only the map's highlighted sprites show the cycle (#1544): the equipment page's selector and reserve strip,
        // which the cartridge draws in the same colour group, show their own colours.
        if (ShownHighlight(pause) is int frame)
            InstalledFixtures.ImportOf(maps).HighlightColours.Colours(frame).CopyTo(colours.AsSpan(HighlightFirstColour));
        PictureShading arrow = pause.ReserveArrowShading;
        if (!arrow.IsStandard)
        {
            PauseReserveArrowColours arrowColours = InstalledFixtures.ImportOf(maps).Pause.ReserveUi.ArrowColours;
            PictureShade shade = arrow.Shades.Single();
            ReserveArrowColors shown = shade == PauseReserveArrowShades.Solid ? arrowColours.Solid : arrowColours.Pulse(shade.Step);
            colours[ArrowShadeColour] = shown.Color6;
            colours[ArrowLightColour] = shown.Color11;
        }
        return colours;
    }
}
