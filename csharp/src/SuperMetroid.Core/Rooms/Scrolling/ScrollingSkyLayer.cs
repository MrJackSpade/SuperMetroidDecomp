using System.Collections.Immutable;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>
/// The scrolling sky as a frame shows it (#1347): its tile rows, and 23 bands that each shift horizontally by their
/// own offset. The cartridge draws the bands with an indirect HDMA table of BG2HOFS words (<c>$88:ADC2</c>); this is
/// that table's source, published as layer state.
/// </summary>
/// <param name="Tilemap">The land or ocean sky rows.</param>
/// <param name="VerticalScroll">The layer-one Y the bands are positioned from; the sky's BG2 vertical scroll, wrapping at 16 bits.</param>
/// <param name="BandOffsets">The whole-pixel horizontal offset in each of the 23 band data slots.</param>
/// <param name="BandsShown">False on a frozen frame, whose terminated table leaves every band unshifted.</param>
public sealed record ScrollingSkyLayer(SkyTilemap Tilemap, int VerticalScroll, ImmutableArray<ushort> BandOffsets, bool BandsShown)
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
            ushort worldY = unchecked((ushort)(VerticalScroll + RoomFxRomData.ScrollingSky.GameplayFirstScanline + line));
            int band = BandAt(worldY);
            // The assembly's fallback entries point at direct-page BG2HOFS ($00B5), which is zero for the sky rooms.
            result[line] = band >= 0 ? BandOffsets[RoomFxRomData.ScrollingSky.GetSection(band).DataSlot] : (ushort)0;
        }
        return result;
    }

    /// <summary>The band covering a world position, or -1 below the sky's bands.</summary>
    private static int BandAt(ushort worldY)
    {
        for (int index = 0; index < RoomFxRomData.ScrollingSky.SectionCount; index++)
        {
            int nextTop = index + 1 < RoomFxRomData.ScrollingSky.SectionCount
                ? RoomFxRomData.ScrollingSky.GetSection(index + 1).TopPosition
                : RoomFxRomData.ScrollingSky.WorldEndPosition;
            if (worldY >= RoomFxRomData.ScrollingSky.GetSection(index).TopPosition && worldY < nextTop)
                return index;
        }
        return -1;
    }
}
