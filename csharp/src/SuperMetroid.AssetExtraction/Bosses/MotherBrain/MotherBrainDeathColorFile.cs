using System.Text.Json;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain;

/// <summary>Reads and writes the installed Mother Brain death colour document.</summary>
public static class MotherBrainDeathColorFile
{
    /// <summary>The body and leg fade images, shown in order before the body is cleared.</summary>
    public const int BodyFadeFrameCount = 16;

    /// <summary>The corpse-head fade images, shown in order while the corpse rots away.</summary>
    public const int CorpseFadeFrameCount = 8;

    /// <summary>Validates the installed death-fade RGB5 document and keeps every frame as installed.</summary>
    /// <param name="json">UTF-8 JSON source consumed from its current position and left open.</param>
    /// <returns>Compiled color catalog detached from the deserialized document arrays.</returns>
    /// <exception cref="ArgumentNullException">The source stream is null.</exception>
    /// <exception cref="InvalidDataException">The JSON contains duplicate or unknown properties, an unsupported version, incorrect image dimensions, null colors, or channels outside 0..31.</exception>
    public static MotherBrainDeathColorCatalog Read(Stream json)
    {
        Ensure.NotNull(json);
        MotherBrainDeathColorDocument document;
        document = JsonAssetDocument.Read<MotherBrainDeathColorDocument>(json, ArtworkJson.Options, "Mother Brain death color");
        if (document.Version != MotherBrainDeathColorFormat.Version)
            throw new InvalidDataException(
                "Mother Brain death colors require the supported version.");
        return new(CompileFrames(document.BodyFade,
                BodyFadeFrameCount,
                MotherBrainDeathColorFormat.BodyColorCount, "body fade"),
            CompileFrames(document.LegFade,
                BodyFadeFrameCount,
                MotherBrainDeathColorFormat.BodyColorCount, "leg fade"),
            CompileFrames(document.CorpseFade,
                CorpseFadeFrameCount,
                MotherBrainDeathColorFormat.CorpseColorCount, "corpse fade"));
    }

    /// <summary>Serializes the editable death-color document to indented camel-case UTF-8 JSON.</summary>
    /// <param name="document">Palette-stage colors to serialize; their collections are not retained.</param>
    /// <returns>JSON bytes ready to install under <see cref="MotherBrainDeathColorFormat.FileName"/>.</returns>
    public static byte[] Write(MotherBrainDeathColorDocument document) =>
        JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options);

    /// <summary>Checks the authored stage count and converts each RGB5 row.</summary>
    private static RgbColor[][] CompileFrames(PaletteRgb5[][]? source,
        int frameCount, int colorCount, string name)
    {
        if (source is null || source.Length != frameCount)
            throw new InvalidDataException(
                $"Mother Brain death {name} requires {frameCount} frames.");
        return source.Select((frame, index) =>
            Compile(frame, colorCount, $"{name} frame {index}")).ToArray();
    }

    /// <summary>Checks row size and channel bounds before packing one palette image.</summary>
    private static RgbColor[] Compile(PaletteRgb5[]? source, int count, string name) =>
        Rgb5Rows.Compile(source, count, $"Mother Brain death {name}");
}
