using System.Collections.Immutable;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// The scrolling sky's bands as a frame shows them (#1347): 23 bands that each shift horizontally by their own offset. The cartridge draws the bands with an indirect HDMA table of BG2HOFS words (<c>$88:ADC2</c>); this is
/// that table's source, published as layer state.
/// </summary>
/// <param name="VerticalScroll">The layer-one Y the bands are positioned from; the sky's BG2 vertical scroll, wrapping at 16 bits.</param>
/// <param name="BandOffsets">The whole-pixel horizontal offset in each of the 23 band data slots.</param>
/// <param name="BandsShown">False on a frozen frame, whose terminated table leaves every band unshifted.</param>
public sealed record ScrollingSkyLayer(int VerticalScroll, ImmutableArray<ushort> BandOffsets, bool BandsShown)
{
    /// <summary>
    /// The horizontal scroll of each gameplay line, from physical scanline 32 down: the offset of the band covering
    /// that line's world position, or zero below the sky's bands or while they are not shown.
    /// </summary>
    /// <param name="lineCount">Gameplay lines to resolve.</param>
    public ushort[] GameplayLineScrolls(int lineCount = 192)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(lineCount);
        var result = new ushort[lineCount];
        if (!BandsShown)
            return result;
        for (int line = 0; line < lineCount; line++)
        {
            ushort worldY = unchecked((ushort)(VerticalScroll + ScrollingSkyBandDefinitions.GameplayFirstScanline + line));
            int band = BandAt(worldY);
            // The assembly's fallback entries point at direct-page BG2HOFS ($00B5), which is zero for the sky rooms.
            result[line] = band >= 0 ? BandOffsets[ScrollingSkyBands.GetSection(band).DataSlot] : (ushort)0;
        }
        return result;
    }

    /// <summary>The band covering a world position, or -1 below the sky's bands.</summary>
    private static int BandAt(ushort worldY)
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
