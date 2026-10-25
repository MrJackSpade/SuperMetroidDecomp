using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain;

namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain;

/// <summary>Reads and writes the installed Mother Brain death colour document.</summary>
public static class MotherBrainDeathColorFile
{
    /// <summary>Validates and compiles the death-fade and exploded-door RGB5 document, retaining edited images independently of the health-palette presentation.</summary>
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
                MotherBrainDeathDefinitions.BodyFadeFrameCount,
                MotherBrainDeathDefinitions.BodyColorCount, "body fade"),
            CompileFrames(document.LegFade,
                MotherBrainDeathDefinitions.BodyFadeFrameCount,
                MotherBrainDeathDefinitions.BodyColorCount, "leg fade"),
            CompileFrames(document.CorpseFade,
                MotherBrainDeathDefinitions.CorpseFadeFrameCount,
                MotherBrainDeathDefinitions.CorpseColorCount, "corpse fade"),
            Compile(document.ExplodedDoor, MotherBrainDeathDefinitions.BodyColorCount,
                "exploded door"));
    }

    /// <summary>Serializes the editable death-color document to indented camel-case UTF-8 JSON and validates the resulting bytes through <see cref="Read"/>.</summary>
    /// <param name="document">Palette-stage and door colors to serialize; their collections are not retained.</param>
    /// <returns>Validated JSON bytes ready to install under <see cref="MotherBrainDeathColorFormat.FileName"/>.</returns>
    /// <exception cref="InvalidDataException">The serialized document fails schema, dimension, or RGB5-channel validation.</exception>
    public static byte[] Write(MotherBrainDeathColorDocument document) =>
        JsonAssetDocument.WriteValidated(document, ArtworkJson.Options, json => _ = Read(json));

    /// <summary>Checks the authored stage count and converts each RGB5 row.</summary>
    private static Bgr555[][] CompileFrames(PaletteRgb5[][]? source,
        int frameCount, int colorCount, string name)
    {
        if (source is null || source.Length != frameCount)
            throw new InvalidDataException(
                $"Mother Brain death {name} requires {frameCount} frames.");
        return source.Select((frame, index) =>
            Compile(frame, colorCount, $"{name} frame {index}")).ToArray();
    }

    /// <summary>Checks row size and channel bounds before packing one palette image.</summary>
    private static Bgr555[] Compile(PaletteRgb5[]? source, int count, string name) =>
        Rgb5Rows.Compile(source, count, $"Mother Brain death {name}");
}
