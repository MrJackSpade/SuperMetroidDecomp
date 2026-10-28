using SuperMetroid.Core.Spatial;
using System.Collections.Immutable;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// BG2's scroll on each gameplay line, 32 through 223, where a room effect scrolls BG2 line by line (#1403): lava and
/// water waves, the scrolling sky's bands, Phantoon's wave, Crocomire's melt. Each axis is scrolled independently, so
/// either may follow the effect while the other keeps BG2's own scroll on every line.
/// </summary>
public readonly struct Bg2LineScrolls
{
    /// <summary>Each gameplay line's scroll; an axis the effect does not drive is unused.</summary>
    private readonly ImmutableArray<Position16<Bg2Pixels>> lines;

    /// <summary>Reads each axis's per-line scrolls; an empty axis keeps BG2's own scroll.</summary>
    /// <param name="horizontal">Empty or exactly 192 horizontal scrolls; index zero is physical line 32.</param>
    /// <param name="vertical">Empty or exactly 192 vertical scrolls, not precomputed source rows.</param>
    /// <exception cref="ArgumentException">A nonempty axis does not hold exactly 192 scrolls.</exception>
    [SpatialComponents("codec: the two axes' line scrolls arrive from independent per-line effects")]
    public Bg2LineScrolls(ReadOnlySpan<ushort> horizontal, ReadOnlySpan<ushort> vertical)
    {
        if ((!horizontal.IsEmpty && horizontal.Length != GameplayLines) || (!vertical.IsEmpty && vertical.Length != GameplayLines))
            throw new ArgumentException("Per-line BG2 scrolls cover exactly 192 gameplay lines.");
        HorizontalShown = !horizontal.IsEmpty;
        VerticalShown = !vertical.IsEmpty;
        if (!HorizontalShown && !VerticalShown)
            return;
        var builder = ImmutableArray.CreateBuilder<Position16<Bg2Pixels>>(GameplayLines);
        for (int line = 0; line < GameplayLines; line++)
            builder.Add(new Position16<Bg2Pixels>(HorizontalShown ? horizontal[line] : (ushort)0, VerticalShown ? vertical[line] : (ushort)0));
        lines = builder.MoveToImmutable();
    }

    /// <summary>The number of gameplay lines a scrolled axis covers.</summary>
    public static int GameplayLines => ScreenGeometry.ScreenHeightPixels - ScreenGeometry.GameplayHudHeightPixels;

    /// <summary>Whether a room effect scrolls BG2 horizontally line by line.</summary>
    public bool HorizontalShown { get; }

    /// <summary>Whether a room effect scrolls BG2 vertically line by line.</summary>
    public bool VerticalShown { get; }

    /// <summary>
    /// BG2's scroll on gameplay line <paramref name="line"/> (0..191): each axis the effect drives at that line, the other
    /// <paramref name="latched"/>'s.
    /// </summary>
    public Position16<Bg2Pixels> At(int line, Position16<Bg2Pixels> latched) => new(
        HorizontalShown ? lines[line].X : latched.X,
        VerticalShown ? lines[line].Y : latched.Y);
}
