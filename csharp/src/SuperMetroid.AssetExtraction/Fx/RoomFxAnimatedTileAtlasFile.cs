using SuperMetroid.Core.Fx;

namespace SuperMetroid.AssetExtraction.Fx;

/// <summary>
/// The installed room-FX atlas PNG (#1409): its file name, pixel geometry and the stable order of the character segments
/// it holds. Only the import pipeline reads and writes
/// the file, and composes room sheets from its compiled characters (<see cref="RoomFxAnimatedTileAtlasFormat"/>).
/// </summary>
public static class RoomFxAnimatedTileAtlasFile
{
    /// <summary>Installed indexed PNG filename for the append-only strip of room-FX and related animated characters.</summary>
    public const string FileName = "room-fx-animated-tiles.png";
    /// <summary>Two planar bitplanes per pixel, producing sixteen bytes for each 8-by-8 character.</summary>
    public const int BitsPerPixel = 2;
    /// <summary>Four palette-index values representable by the two-bit artwork; visible palette colors remain separately owned.</summary>
    public const int ColorCount = 4;
    /// <summary>Required current PNG width in pixels, with every character placed horizontally in stable transfer-segment order.</summary>
    public const int Width = RoomFxAnimatedTileAtlasFormat.TileCount * 8;
    /// <summary>Required PNG height in pixels: one 8-pixel character row.</summary>
    public const int Height = 8;

    /// <summary>
    /// Stable PNG strip order: existing simple frames, treadmills, the statue strip, spores, then spikes.
    /// </summary>
    public static IEnumerable<RoomFxAtlasSegment> Segments => EnumerateSegments();

    /// <summary>Yields atlas segments in stable PNG order and checks that their combined byte count matches the installed geometry.</summary>
    private static IEnumerable<RoomFxAtlasSegment> EnumerateSegments()
    {
        int total = 0;
        foreach (RoomFxBlockAnimationLoop definition in RoomFxAnimatedTileLoops.All)
            if (definition.AnimatedObject is not (BlockAnimation.Spores or BlockAnimation.HorizontalSpikes))
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
        foreach (var segment in Frames(RoomFxAnimatedTileLoops.Require(BlockAnimation.Spores)))
        {
            total += segment.ByteCount;
            yield return segment;
        }
        foreach (var segment in Frames(RoomFxAnimatedTileLoops.Require(BlockAnimation.HorizontalSpikes)))
        {
            total += segment.ByteCount;
            yield return segment;
        }
        if (total != RoomFxAnimatedTileAtlasFormat.TotalByteCount)
            throw new InvalidOperationException("Compiled room-FX atlas segments do not match the PNG geometry.");

        // Each distinct picture once: the last spike step reuses a picture already present in the strip.
        static IEnumerable<RoomFxAtlasSegment> Frames(RoomFxBlockAnimationLoop definition) =>
            definition.Images.Select(image => new RoomFxAtlasSegment.Image(image, definition.PictureByteCount));
    }
}
