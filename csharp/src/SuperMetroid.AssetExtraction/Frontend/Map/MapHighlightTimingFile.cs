using System.Text.Json;
using SuperMetroid.AssetExtraction.Frontend.Map.Artwork;
using SuperMetroid.Core.Frontend.Map.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>
/// The installed map highlight cycle (#1512, #1693): how long each frame holds, which is all the game binds of the cycle.
/// Each frame's highlight colour, which the import pre-colours the map sprite sheet's highlight shades from, is import data
/// (<see cref="MapPaletteCycleFile"/>).
/// </summary>
public static class MapHighlightTimingFile
{
    /// <summary>JSON filename of the installed cycle, which pause and file-select map highlight animation step through.</summary>
    public const string FileName = "map-highlight-cycle.json";

    /// <summary>
    /// Required cycle-document schema revision: 2 stated each frame's look as its highlight colour (#1637); 3 holds only each
    /// frame's hold, the colours being import data (#1693).
    /// </summary>
    public const int Version = 3;

    /// <summary>The installed cycle of the extracted cycle document <paramref name="artwork"/>: each frame's hold, in frame order.</summary>
    /// <exception cref="InvalidDataException">The extracted document does not decode.</exception>
    internal static byte[] Extract(byte[] artwork)
    {
        MapPaletteCycle cycle = MapPaletteCycleFile.Read(new MemoryStream(artwork, writable: false));
        return JsonSerializer.SerializeToUtf8Bytes(new MapHighlightTimingDocument
        {
            Version = Version,
            DurationTicks = [.. Enumerable.Range(0, cycle.Timing.FrameCount).Select(cycle.Timing.Duration)],
        }, ArtworkJson.Options);
    }

    /// <summary>Reads and validates the installed cycle: the frame count and each frame's hold the game steps the highlight through.</summary>
    /// <param name="json">Caller-owned readable JSON stream, consumed from its current position without being disposed.</param>
    /// <exception cref="InvalidDataException">The JSON, version, frame count or a hold is invalid.</exception>
    public static MapHighlightCycle Read(Stream json)
    {
        MapHighlightTimingDocument document = JsonAssetDocument.Read<MapHighlightTimingDocument>(
            json, ArtworkJson.Options, "map highlight cycle");
        if (document.Version != Version || document.DurationTicks is null ||
            document.DurationTicks.Length is < 1 or > MapPaletteCycleFile.MaximumFrames)
            throw new InvalidDataException($"Map highlight cycle requires version {Version} and 1-{MapPaletteCycleFile.MaximumFrames} frames.");
        for (int frame = 0; frame < document.DurationTicks.Length; frame++)
            if (document.DurationTicks[frame] is < 1 or > MapPaletteCycleFile.MaximumDuration)
                throw new InvalidDataException($"Map highlight frame {frame} requires 1-{MapPaletteCycleFile.MaximumDuration} ticks.");
        return new MapHighlightCycle([.. document.DurationTicks]);
    }
}
