using System.Text.Json;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Enemies.Animals;
using SuperMetroid.AssetExtraction.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>Reads and writes the installed Dachora colour document.</summary>
public static class DachoraColorFile
{
    /// <summary>Loads the supported camel-case JSON schema, rejecting duplicate or unknown properties, incorrect palette/frame counts, null colors, and RGB5 channels outside 0..31.</summary>
    /// <param name="json">Readable JSON stream, left open after loading.</param>
    /// <returns>Validated default, speed, and shine colors without cartridge reads or ownership of native animation timers.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="InvalidDataException">The JSON, schema version, palette dimensions, or color values are invalid.</exception>
    public static DachoraColorCatalog Read(Stream json)
    {
        Ensure.NotNull(json);
        DachoraColorDocument document;
        document = JsonAssetDocument.Read<DachoraColorDocument>(json, ArtworkJson.Options, "Dachora color");
        if (document.Version != DachoraColorFormat.Version)
            throw new InvalidDataException("Dachora colors require the supported version.");
        return new(Compile(document.Normal, "normal"),
            CompileFrames(document.Speed, "speed"),
            CompileFrames(document.Shine, "shine"));
    }

    /// <summary>Serializes the editable palettes as indented camel-case UTF-8 JSON.</summary>
    /// <param name="document">Supported-version default palette and four-frame speed/shine sequences.</param>
    /// <returns>JSON bytes ready to store as the Dachora color resource.</returns>
    public static byte[] Write(DachoraColorDocument document) =>
        JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options);

    /// <summary>Requires four animated frames and validates each sixteen-color row.</summary>
    private static RgbColor[][] CompileFrames(PaletteRgb5[][]? source, string name)
    {
        if (source is null || source.Length != DachoraColorDefinitions.AnimatedFrameCount)
            throw new InvalidDataException(
                $"Dachora {name} requires {DachoraColorDefinitions.AnimatedFrameCount} frames.");
        return source.Select((frame, index) => Compile(frame, $"{name} frame {index}"))
            .ToArray();
    }

    /// <summary>Checks a full Dachora palette row and converts five-bit channels to hardware color words.</summary>
    private static RgbColor[] Compile(PaletteRgb5[]? source, string name)
    {
        if (source is null || source.Length != DachoraColorFormat.ColorsPerFrame)
            throw new InvalidDataException(
                $"Dachora {name} requires {DachoraColorFormat.ColorsPerFrame} RGB5 colors.");
        var compiled = new RgbColor[source.Length];
        for (int color = 0; color < source.Length; color++)
        {
            PaletteRgb5? rgb = source[color];
            if (rgb is null || (uint)rgb.Red > 31 || (uint)rgb.Green > 31 ||
                (uint)rgb.Blue > 31)
                throw new InvalidDataException(
                    $"Dachora {name} color {color} requires RGB5 channels 0..31.");
            compiled[color] = rgb.ToRgbColor();
        }
        return compiled;
    }
}
