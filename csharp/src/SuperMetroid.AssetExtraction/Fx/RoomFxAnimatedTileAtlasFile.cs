using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.Artwork;

namespace SuperMetroid.AssetExtraction.Fx;

/// <summary>
/// The installed room-FX atlas PNG (#1409): its file name, pixel geometry and the stable order of the character segments
/// it holds, and the widths of the older sheets an override may still supply. Only the import pipeline reads and writes
/// the file; the game holds the compiled characters (<see cref="RoomFxAnimatedTileAtlasFormat"/>).
/// </summary>
public static class RoomFxAnimatedTileAtlasFile
{
    /// <summary>Installed indexed PNG filename for the append-only strip of room-FX and related animated characters.</summary>
    public const string FileName = "room-fx-animated-tiles.png";
    /// <summary>Two planar bitplanes per pixel, producing sixteen bytes for each 8-by-8 character.</summary>
    public const int BitsPerPixel = 2;
    /// <summary>Four palette-index values representable by the two-bit artwork; visible palette colors remain separately owned.</summary>
    public const int ColorCount = 4;
    /// <summary>Legacy PNG width in pixels for the original 89-character prefix, admitted only with a seekable stream and current-stock tail fallback.</summary>
    public const int LegacyWidth = RoomFxAnimatedTileAtlasFormat.LegacyTileCount * 8;
    /// <summary>Legacy PNG width in pixels for the simple-FX/treadmill prefix, whose missing statue and later characters are inherited from current stock.</summary>
    public const int PreStatueWidth = RoomFxAnimatedTileAtlasFormat.PreStatueTileCount * 8;
    /// <summary>Legacy PNG width in pixels for the prefix through statue characters, whose missing spore and spike tail is inherited from current stock.</summary>
    public const int PreSporesWidth = RoomFxAnimatedTileAtlasFormat.PreSporesTileCount * 8;
    /// <summary>Legacy PNG width in pixels for the prefix through spore characters, whose missing spike tail is inherited from current stock.</summary>
    public const int PreSpikesWidth = RoomFxAnimatedTileAtlasFormat.PreSpikesTileCount * 8;
    /// <summary>Required current PNG width in pixels, with every character placed horizontally in stable transfer-segment order.</summary>
    public const int Width = RoomFxAnimatedTileAtlasFormat.TileCount * 8;
    /// <summary>Required PNG height in pixels: one 8-pixel character row.</summary>
    public const int Height = 8;

    /// <summary>
    /// Stable PNG strip order: existing simple frames, treadmills, the statue strip, spores, then spikes. Never insert new
    /// frames into the historical prefix or older replacements would shift unrelated art.
    /// </summary>
    public static IEnumerable<RoomFxAtlasSegment> Segments => EnumerateSegments();

    /// <summary>Yields atlas segments in stable PNG order and checks that their combined byte count matches the installed geometry.</summary>
    private static IEnumerable<RoomFxAtlasSegment> EnumerateSegments()
    {
        int total = 0;
        foreach (RoomFxAnimatedTileLoop definition in RoomFxAnimatedTileLoops.All)
            if (definition.AnimatedObject is not (AnimatedTileObject.Spores or AnimatedTileObject.HorizontalSpikes))
                foreach (var segment in Frames(definition))
                {
                    total += segment.ByteCount;
                    yield return segment;
                }
        WreckedShipTreadmillLoop treadmill =
            WreckedShipTreadmills.ForDirection(WreckedShipTreadmillDirection.Rightwards);
        for (int frame = 0; frame < RoomFxAnimatedTileAtlasFormat.TreadmillFrameCount; frame++)
        {
            total += RoomFxAnimatedTileLoops.TreadmillFrameByteCount;
            yield return new RoomFxAtlasSegment.Image(treadmill.Image(frame), RoomFxAnimatedTileLoops.TreadmillFrameByteCount);
        }
        var statues = new RoomFxAtlasSegment.TourianStatueStrip();
        total += statues.ByteCount;
        yield return statues;
        foreach (var segment in Frames(RoomFxAnimatedTileLoops.Require(AnimatedTileObject.Spores)))
        {
            total += segment.ByteCount;
            yield return segment;
        }
        foreach (var segment in Frames(RoomFxAnimatedTileLoops.Require(AnimatedTileObject.HorizontalSpikes)))
        {
            total += segment.ByteCount;
            yield return segment;
        }
        if (total != RoomFxAnimatedTileAtlasFormat.TotalByteCount)
            throw new InvalidOperationException("Compiled room-FX atlas segments do not match the PNG geometry.");

        // Each distinct picture once: the last spike step reuses a picture already present in the strip.
        static IEnumerable<RoomFxAtlasSegment> Frames(RoomFxAnimatedTileLoop definition) =>
            definition.Images.Select(image => new RoomFxAtlasSegment.Image(image, definition.FrameByteCount));
    }
}
