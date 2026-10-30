using System.Collections.Immutable;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// The background's scroll on each gameplay line, 32 through 223, where a room effect scrolls it line by line (#1403): lava and
/// water waves and the scrolling sky's bands as tables, Phantoon's wave as a described <see cref="ScrollWave"/> and
/// Crocomire's melt as a described <see cref="ScrollStretch"/> (#1468). Each axis is scrolled independently, so either
/// may follow the effect while the other keeps the background's own scroll on every line.
/// </summary>
public readonly struct BackgroundLineScrolls
{
    /// <summary>Each gameplay line's scroll; an axis the effect does not drive is unused.</summary>
    private readonly ImmutableArray<Position16<BackgroundPixels>> lines;

    /// <summary>Reads each axis's per-line scrolls; an empty axis keeps the background's own scroll.</summary>
    /// <param name="horizontal">Empty or exactly 192 horizontal scrolls; index zero is screen line 32.</param>
    /// <param name="vertical">Empty or exactly 192 vertical scrolls, not precomputed source rows.</param>
    /// <exception cref="ArgumentException">A nonempty axis does not hold exactly 192 scrolls.</exception>
    [SpatialComponents("codec: the two axes' line scrolls arrive from independent per-line effects")]
    public BackgroundLineScrolls(ReadOnlySpan<ushort> horizontal, ReadOnlySpan<ushort> vertical)
    {
        if ((!horizontal.IsEmpty && horizontal.Length != GameplayLines) || (!vertical.IsEmpty && vertical.Length != GameplayLines))
            throw new ArgumentException("Per-line background scrolls cover exactly 192 gameplay lines.");
        HorizontalShown = !horizontal.IsEmpty;
        VerticalShown = !vertical.IsEmpty;
        if (!HorizontalShown && !VerticalShown)
            return;
        var builder = ImmutableArray.CreateBuilder<Position16<BackgroundPixels>>(GameplayLines);
        for (int line = 0; line < GameplayLines; line++)
            builder.Add(new Position16<BackgroundPixels>(HorizontalShown ? horizontal[line] : (ushort)0, VerticalShown ? vertical[line] : (ushort)0));
        lines = builder.MoveToImmutable();
    }

    /// <summary>The number of gameplay lines a scrolled axis covers.</summary>
    public static int GameplayLines => ScreenGeometry.ScreenHeightPixels - ScreenGeometry.GameplayHudHeightPixels;

    /// <summary>Whether a room effect scrolls the background horizontally line by line.</summary>
    public bool HorizontalShown { get; }

    /// <summary>Whether a room effect scrolls the background vertically line by line.</summary>
    public bool VerticalShown { get; }

    /// <summary>
    /// The sine wave that ripples the background sideways (#1468), or null; it replaces the horizontal table, so the two
    /// are never both given.
    /// </summary>
    /// <exception cref="ArgumentException">A horizontal table is also given.</exception>
    public ScrollWave? HorizontalWave
    {
        get;
        init => field = value is not null && HorizontalShown
            ? throw new ArgumentException("A background's horizontal scroll follows a table or a wave, not both.", nameof(value))
            : value;
    }

    /// <summary>
    /// The stretch that sags the background downward through a band (#1468), or null; it replaces the vertical table, so
    /// the two are never both given.
    /// </summary>
    /// <exception cref="ArgumentException">A vertical table is also given.</exception>
    public ScrollStretch? VerticalStretch
    {
        get;
        init => field = value is not null && VerticalShown
            ? throw new ArgumentException("A background's vertical scroll follows a table or a stretch, not both.", nameof(value))
            : value;
    }

    /// <summary>
    /// The background's scroll on gameplay line <paramref name="line"/> (0..191) before any described shape displaces it:
    /// each axis's table at that line, a wave's or a stretch's base scroll, otherwise <paramref name="latched"/>'s. The
    /// backend displaces the line by <see cref="HorizontalWave"/> and <see cref="VerticalStretch"/> as it draws.
    /// </summary>
    public Position16<BackgroundPixels> At(int line, Position16<BackgroundPixels> latched) => new(
        HorizontalWave is { } wave ? checked((ushort)wave.BaseScroll) : HorizontalShown ? lines[line].X : latched.X,
        VerticalStretch is { } stretch ? checked((ushort)stretch.BaseScroll) : VerticalShown ? lines[line].Y : latched.Y);
}
