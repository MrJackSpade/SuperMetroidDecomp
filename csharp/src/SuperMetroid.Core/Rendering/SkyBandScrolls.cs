using System.Collections.Immutable;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Rendering;

/// <summary>The scrolling sky's bands as the display draws them (#1468): bands of BG2 horizontal scroll down the gameplay lines.</summary>
public static class SkyBandScrolls
{
    /// <summary>
    /// The sky's bands of horizontal scroll down the gameplay lines, each moved by <paramref name="shift"/>: from the first
    /// gameplay line, each line shows the offset of the world row it shows; a band ends where its world rows reach the
    /// next band, the end of the sky, or the sixteen-bit wrap. Lines above the gameplay lines show the unshifted base.
    /// </summary>
    /// <param name="sky">The sky the frame published.</param>
    /// <param name="shift">The horizontal shift added to every line, as the room's shake adds to BG2's register after HDMA.</param>
    public static ScrollBands Of(ScrollingSkyLayer sky, int shift)
    {
        Ensure.NotNull(sky);
        var bands = ImmutableArray.CreateBuilder<ScrollBand>();
        int line = ScreenGeometry.GameplayHudHeightPixels;
        while (line < ScreenGeometry.ScreenHeightPixels)
        {
            int worldY = unchecked((ushort)(sky.FirstGameplayRow + line - ScreenGeometry.GameplayHudHeightPixels));
            bands.Add(new(line, Shifted(sky.OffsetAt(worldY), shift)));
            line += ScrollingSkyLayer.NextBandRow(worldY) - worldY;
        }
        return new ScrollBands(Shifted(0, shift), bands.ToImmutable());
    }

    /// <summary><paramref name="offset"/> moved by <paramref name="shift"/>, wrapping as a sixteen-bit scroll.</summary>
    private static int Shifted(int offset, int shift) => unchecked((ushort)(offset + shift));
}
