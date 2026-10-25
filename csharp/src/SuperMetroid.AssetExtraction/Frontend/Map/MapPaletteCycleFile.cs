using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Frontend.Map.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>Reads and writes the installed map highlight cycle document.</summary>
public static class MapPaletteCycleFile
{
    /// <summary>Compiles an editable ordered color-and-duration document into an immutable map-highlight cycle.</summary>
    /// <param name="json">Caller-owned readable JSON stream, consumed from its current position without being disposed.</param>
    /// <returns>A cycle owning copied native-precision colors and holds; it contains no playback timer, CGRAM destination, or loop-sound command.</returns>
    /// <remarks>Stock extraction reads timing at $82:C10C and colors at $82:A987. A fourteen-frame cycle shares matching reverse phases while retaining independently edited reverse colors exactly; other lengths preserve the supplied frame sequence directly.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="InvalidDataException">The JSON, version, frame count, tick durations, sixteen-color frame shape, or RGB5 components are invalid.</exception>
    public static MapPaletteCycle Read(Stream json)
    {
        MapPaletteCycleDocument document = JsonAssetDocument.Read<MapPaletteCycleDocument>(
            json, ArtworkJson.Options, "map highlight cycle");
        if (document.Version != MapPaletteCycleFormat.Version || document.Frames is null ||
            document.Frames.Length is < 1 or > MapPaletteCycleFormat.MaximumFrames)
            throw new InvalidDataException("Map highlight cycle requires version 1 and 1-255 frames.");
        var durations = new byte[document.Frames.Length];
        var colors = new RgbColor[document.Frames.Length][];
        for (int frame = 0; frame < durations.Length; frame++)
        {
            var entry = document.Frames[frame];
            if (entry is null || entry.DurationTicks is < 1 or > MapPaletteCycleFormat.MaximumDuration ||
                entry.Colors is null || entry.Colors.Length != MapPaletteCycleFormat.ColorCount)
                throw new InvalidDataException($"Map highlight frame {frame} requires 1-254 ticks and sixteen colors.");
            durations[frame] = (byte)entry.DurationTicks;
            colors[frame] = new RgbColor[entry.Colors.Length];
            for (int color = 0; color < entry.Colors.Length; color++)
            {
                var rgb = entry.Colors[color];
                if (rgb is null || (uint)rgb.Red > 31 || (uint)rgb.Green > 31 || (uint)rgb.Blue > 31)
                    throw new InvalidDataException($"Map highlight frame {frame}, color {color} requires RGB components from 0 to 31.");
                colors[frame][color] = rgb.ToRgbColor();
            }
        }
        return new(durations, colors);
    }

    /// <summary>Serializes and validates a complete highlight cycle before writing any UTF-8 document bytes to the destination.</summary>
    /// <param name="json">Caller-owned writable stream, written at its current position without being disposed.</param>
    /// <param name="document">Ordered selected frames satisfying <see cref="Read"/>'s version, duration, count, and color requirements.</param>
    /// <exception cref="InvalidDataException">The document is null or fails highlight-cycle validation.</exception>
    public static void Write(Stream json, MapPaletteCycleDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options);
        _ = Read(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}
