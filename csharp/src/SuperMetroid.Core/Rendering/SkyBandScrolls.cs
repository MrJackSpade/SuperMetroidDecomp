using System.Collections.Immutable;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Rendering;

/// <summary>The scrolling sky's bands as the display draws them (#1468, #1687): bands of the background's horizontal scroll down the gameplay lines.</summary>
public static class SkyBandScrolls
{
    /// <summary>
    /// The sky's bands of horizontal scroll down the gameplay lines, each moved by <paramref name="shift"/>: each gameplay
    /// line shows the room row the camera's row plus the line reaches, at that row's offset; a screen band ends where its
    /// rows reach the end of the sky band or the next sky band's top, and the rows below the last sky band show unshifted
    /// to the bottom of the screen. Lines above the gameplay lines show the unshifted base.
    /// </summary>
    /// <param name="sky">The sky the frame published.</param>
    /// <param name="shift">The horizontal shift added to every line, as the room's shake adds to the background's scroll after each band's value.</param>
    public static ScrollBands Of(ScrollingSkyLayer sky, int shift)
    {
        Ensure.NotNull(sky);
        var bands = ImmutableArray.CreateBuilder<ScrollBand>();
        int line = ScreenGeometry.GameplayHudHeightPixels;
        while (line < ScreenGeometry.ScreenHeightPixels)
        {
            int row = sky.CameraRow + line;
            bands.Add(new(line, sky.OffsetAt(row) + shift));
            if ((sky.BandAt(row)?.EndRow ?? sky.NextBandTop(row)) is not { } nextRow)
                break;
            line += nextRow - row;
        }
        return new ScrollBands(shift, bands.ToImmutable());
    }
}
