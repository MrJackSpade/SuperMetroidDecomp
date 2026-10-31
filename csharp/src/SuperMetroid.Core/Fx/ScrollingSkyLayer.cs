using System.Collections.Immutable;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// The scrolling sky's bands as a frame shows them (#1347): 23 bands that each shift horizontally by their own offset. The cartridge draws the bands with an indirect per-line table of BG2HOFS words (<c>$88:ADC2</c>); this is
/// that table's source, published as layer state, from which the display describes the screen's bands (#1468).
/// </summary>
/// <param name="VerticalScroll">The layer-one Y the bands are positioned from; the sky's BG2 vertical scroll, wrapping at 16 bits.</param>
/// <param name="BandOffsets">The whole-pixel horizontal offset in each of the 23 band data slots.</param>
/// <param name="BandsShown">False on a frozen frame, whose terminated table leaves every band unshifted.</param>
public sealed record ScrollingSkyLayer(int VerticalScroll, ImmutableArray<ushort> BandOffsets, bool BandsShown)
{
    /// <summary>The world rows a sixteen-bit row position covers before it wraps.</summary>
    private const int WorldRows = ushort.MaxValue + 1;

    /// <summary>The world row the first gameplay scanline shows, before it wraps at sixteen bits.</summary>
    public int FirstGameplayRow => VerticalScroll + ScrollingSkyBandDefinitions.GameplayFirstScanline;

    /// <summary>
    /// The horizontal offset world row <paramref name="worldY"/> shows: its band's offset, or none below the sky's bands or
    /// while they are not shown. The assembly's fallback entries point at direct-page BG2HOFS ($00B5), which is zero for
    /// the sky rooms.
    /// </summary>
    /// <param name="worldY">A world row, 0 through 65535.</param>
    public int OffsetAt(int worldY) =>
        BandsShown && BandAt(worldY) is >= 0 and var band ? BandOffsets[ScrollingSkyBands.GetSection(band).DataSlot] : 0;

    /// <summary>
    /// The world row after <paramref name="worldY"/> where the band covering it ends: the next band's top, the end of the
    /// sky, or the wrap of the sixteen-bit row below the sky's bands.
    /// </summary>
    /// <param name="worldY">A world row, 0 through 65535.</param>
    public static int NextBandRow(int worldY) => BandAt(worldY) switch
    {
        < 0 => worldY < ScrollingSkyBands.GetSection(0).TopPosition ? ScrollingSkyBands.GetSection(0).TopPosition : WorldRows,
        var band when band + 1 < ScrollingSkyBandDefinitions.SectionCount => ScrollingSkyBands.GetSection(band + 1).TopPosition,
        _ => ScrollingSkyBandDefinitions.WorldEndPosition,
    };

    /// <summary>The band covering a world position, or -1 below the sky's bands.</summary>
    private static int BandAt(int worldY)
    {
        for (int index = 0; index < ScrollingSkyBandDefinitions.SectionCount; index++)
        {
            int nextTop = index + 1 < ScrollingSkyBandDefinitions.SectionCount
                ? ScrollingSkyBands.GetSection(index + 1).TopPosition
                : ScrollingSkyBandDefinitions.WorldEndPosition;
            if (worldY >= ScrollingSkyBands.GetSection(index).TopPosition && worldY < nextTop)
                return index;
        }
        return -1;
    }
}
