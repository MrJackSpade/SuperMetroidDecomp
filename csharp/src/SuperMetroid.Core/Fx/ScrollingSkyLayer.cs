using System.Collections.Immutable;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// The scrolling sky's bands as a frame shows them (#1347): 23 bands that each shift horizontally by their own offset. The game draws the bands with a per-line table of the background horizontal scrolls; this is
/// that table's source, published as layer state, from which the display describes the screen's bands (#1468).
/// </summary>
/// <param name="VerticalScroll">The camera Y the bands are positioned from; the sky's background vertical scroll, wrapping at 16 bits.</param>
/// <param name="BandOffsets">The whole-pixel horizontal offset in each of the 23 band data slots.</param>
/// <param name="BandsShown">False on a frozen frame, whose terminated table leaves every band unshifted.</param>
public sealed record ScrollingSkyLayer(int VerticalScroll, ImmutableArray<int> BandOffsets, bool BandsShown)
{
    /// <summary>The world rows a row position covers before it wraps to row 0.</summary>
    private const int WorldRows = ushort.MaxValue + 1;

    /// <summary>The world row the first gameplay screen row shows, before it wraps past row 65535.</summary>
    public int FirstGameplayRow => VerticalScroll + ScrollingSkyBandDefinitions.GameplayFirstScanline;

    /// <summary>
    /// The horizontal offset world row <paramref name="worldY"/> shows: its band's offset, or none below the sky's bands or
    /// while they are not shown. The table's fallback entries read the layer's own horizontal scroll, which is zero for
    /// the sky rooms.
    /// </summary>
    /// <param name="worldY">A world row, 0 through 65535.</param>
    public int OffsetAt(int worldY) =>
        BandsShown && BandAt(worldY) is { } band ? BandOffsets[ScrollingSkyBands.GetSection(band).DataSlot] : 0;

    /// <summary>
    /// The world row after <paramref name="worldY"/> where the band covering it ends: the next band's top, the end of the
    /// sky, or the wrap past row 65535 below the sky's bands.
    /// </summary>
    /// <param name="worldY">A world row, 0 through 65535.</param>
    public static int NextBandRow(int worldY) => BandAt(worldY) switch
    {
        null => worldY < ScrollingSkyBands.GetSection(0).TopPosition ? ScrollingSkyBands.GetSection(0).TopPosition : WorldRows,
        { } band when band + 1 < ScrollingSkyBandDefinitions.SectionCount => ScrollingSkyBands.GetSection(band + 1).TopPosition,
        _ => ScrollingSkyBandDefinitions.WorldEndPosition,
    };

    /// <summary>The band covering a world position, or null outside the sky's bands.</summary>
    private static int? BandAt(int worldY)
    {
        for (int index = 0; index < ScrollingSkyBandDefinitions.SectionCount; index++)
        {
            int nextTop = index + 1 < ScrollingSkyBandDefinitions.SectionCount
                ? ScrollingSkyBands.GetSection(index + 1).TopPosition
                : ScrollingSkyBandDefinitions.WorldEndPosition;
            if (worldY >= ScrollingSkyBands.GetSection(index).TopPosition && worldY < nextTop)
                return index;
        }
        return null;
    }
}
