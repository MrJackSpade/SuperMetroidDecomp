using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Frontend.Map.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>
/// Reads and writes the map highlight cycle's colours (#1512): each frame's hold and highlight colour, as extracted. It is
/// import data the installation never holds (#1693), which the import pre-colours the map sprite sheet's highlight shades
/// from; the installed cycle holds each frame's hold alone (<see cref="MapHighlightTimingFile"/>).
/// </summary>
public static class MapPaletteCycleFile
{
    /// <summary>JSON filename, in the component's import data, of each highlight frame's hold and highlight colour.</summary>
    public const string FileName = "map-highlight-colours.json";

    /// <summary>Required cycle-document schema revision: 2 states each frame's look as its highlight colour, not a palette row (#1637).</summary>
    public const int Version = 2;

    /// <summary>Maximum selected frame count, 255, preserving the animation owner's byte-sized frame cursor.</summary>
    public const int MaximumFrames = 255;

    /// <summary>Maximum authored frame hold, 254 ticks; zero is invalid and a duration of 255 marks stream termination rather than a hold.</summary>
    public const int MaximumDuration = 254;

    /// <summary>Compiles an editable ordered highlight-colour-and-duration document into an immutable map-highlight cycle.</summary>
    /// <param name="json">Caller-owned readable JSON stream, consumed from its current position without being disposed.</param>
    /// <returns>A cycle owning copied native-precision colors and holds; it contains no playback timer, CGRAM destination, or loop-sound command.</returns>
    /// <remarks>Every frame's hold and highlight colour are kept as installed.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="InvalidDataException">The JSON, version, frame count, tick durations, or highlight colour components are invalid.</exception>
    public static MapPaletteCycle Read(Stream json)
    {
        MapPaletteCycleDocument document = JsonAssetDocument.Read<MapPaletteCycleDocument>(
            json, ArtworkJson.Options, "map highlight cycle");
        if (document.Version != MapPaletteCycleFile.Version || document.Frames is null ||
            document.Frames.Length is < 1 or > MapPaletteCycleFile.MaximumFrames)
            throw new InvalidDataException($"Map highlight cycle requires version {MapPaletteCycleFile.Version} and 1-255 frames.");
        var durations = new int[document.Frames.Length];
        var highlights = new RgbColor[document.Frames.Length];
        for (int frame = 0; frame < durations.Length; frame++)
        {
            var entry = document.Frames[frame];
            if (entry is null || entry.DurationTicks is < 1 or > MapPaletteCycleFile.MaximumDuration)
                throw new InvalidDataException($"Map highlight frame {frame} requires 1-254 ticks.");
            durations[frame] = entry.DurationTicks;
            var rgb = entry.Highlight;
            if (rgb is null || (uint)rgb.Red > 31 || (uint)rgb.Green > 31 || (uint)rgb.Blue > 31)
                throw new InvalidDataException($"Map highlight frame {frame}'s highlight colour requires RGB components from 0 to 31.");
            highlights[frame] = rgb.ToRgbColor();
        }
        return new(durations, highlights);
    }

    /// <summary>Serializes a complete highlight cycle as UTF-8 document bytes to the destination.</summary>
    /// <param name="json">Caller-owned writable stream, written at its current position without being disposed.</param>
    /// <param name="document">Ordered selected frames satisfying <see cref="Read"/>'s version, duration, count, and highlight colour requirements.</param>
    public static void Write(Stream json, MapPaletteCycleDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options);
        json.Write(bytes);
    }
}
