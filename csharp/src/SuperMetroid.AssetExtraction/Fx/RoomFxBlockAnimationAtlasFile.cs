using SuperMetroid.Core.Fx;

namespace SuperMetroid.AssetExtraction.Fx;

/// <summary>
/// The import's room-FX atlas PNG (#1409): its file name, pixel geometry and the stable order of the character segments
/// it holds. Only the import pipeline reads and writes
/// the file, and composes room sheets from its compiled characters (<see cref="RoomFxBlockAnimationAtlasFormat"/>).
/// </summary>
public static class RoomFxBlockAnimationAtlasFile
{
    /// <summary>Import-data indexed PNG filename of the import's room-FX atlas: the strip of room-FX and related animated characters.</summary>
    public const string FileName = "room-fx-block-animations.png";
    /// <summary>Two planar bitplanes per pixel, producing sixteen bytes for each 8-by-8 character.</summary>
    public const int BitsPerPixel = 2;
    /// <summary>Four palette-index values representable by the two-bit artwork; visible palette colors remain separately owned.</summary>
    public const int ColorCount = 4;
    /// <summary>Required current PNG width in pixels, with every character placed horizontally in stable transfer-segment order.</summary>
    public const int Width = RoomFxBlockAnimationAtlasFormat.TileCount * 8;
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
        IEnumerable<RoomFxAtlasSegment>[] strip =
        [
            Frames(RoomFxBlockAnimationLoops.Require(BlockAnimation.MaridiaSandCeiling)),
            Frames(RoomFxBlockAnimationLoops.Require(BlockAnimation.MaridiaSandFalling)),
            Frames(RoomFxBlockAnimationLoops.Require(BlockAnimation.Lava)),
            Frames(RoomFxBlockAnimationLoops.Require(BlockAnimation.Acid)),
            Frames(RoomFxBlockAnimationLoops.Require(BlockAnimation.Rain)),
            Treadmill(BlockAnimation.WreckedShipTreadmillRightwards),
            [new RoomFxAtlasSegment.TourianStatueStrip()],
            Frames(RoomFxBlockAnimationLoops.Require(BlockAnimation.Spores)),
            Frames(RoomFxBlockAnimationLoops.Require(BlockAnimation.HorizontalSpikes)),
        ];
        foreach (RoomFxAtlasSegment segment in strip.SelectMany(part => part))
        {
            total += segment.ByteCount;
            yield return segment;
        }
        if (total != RoomFxBlockAnimationAtlasFormat.TotalByteCount)
            throw new InvalidOperationException("Compiled room-FX atlas segments do not match the PNG geometry.");

        // Each distinct picture once: the last spike step reuses a picture already present in the strip.
        static IEnumerable<RoomFxAtlasSegment> Frames(BlockAnimation definition) =>
            definition.Images.Select(image => new RoomFxAtlasSegment.Image(image, definition.PictureByteCount));

        // The treadmill's frames, each its own picture.
        static IEnumerable<RoomFxAtlasSegment> Treadmill(BlockAnimation treadmill) =>
            Enumerable.Range(0, RoomFxBlockAnimationAtlasFormat.TreadmillFrameCount).Select(frame =>
                new RoomFxAtlasSegment.Image(treadmill.TreadmillImage(frame), RoomFxBlockAnimationLoops.TreadmillFrameByteCount));
    }
}
