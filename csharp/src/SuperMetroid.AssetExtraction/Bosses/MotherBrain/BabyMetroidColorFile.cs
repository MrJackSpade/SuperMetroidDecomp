using System.Text.Json;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain;

/// <summary>Reads and writes the installed Baby Metroid colour document.</summary>
public static partial class BabyMetroidColorFile
{
    /// <summary>Loads <c>Baby Metroid-colors.json</c>, requiring version 1, eight four-color pulse frames, three sixteen-color targets, and RGB5 channels from 0 through 31.</summary>
    /// <param name="json">Caller-owned JSON stream consumed from its current position and left open; unknown and duplicate properties are rejected.</param>
    /// <returns>Every installed pulse phase and target, kept as installed.</returns>
    public static BabyMetroidColorCatalog Read(Stream json)
    {
        Ensure.NotNull(json);
        BabyMetroidColorDocument document;
        document = JsonAssetDocument.Read<BabyMetroidColorDocument>(json, ArtworkJson.Options, "Shitroid color");
        if (document.Version != BabyMetroidColorFormat.Version)
            throw new InvalidDataException("Shitroid colors require the supported version.");
        if (document.Normal is null ||
            document.Normal.Length != BabyMetroidColorDefinitions.NormalFrameCount)
            throw new InvalidDataException("Shitroid normal cycle requires eight frames.");
        return new(document.Normal.Select((frame, index) =>
                Compile(frame, BabyMetroidColorDefinitions.NormalColorsPerFrame,
                    $"normal frame {index}")).ToArray(),
            Compile(document.Sidehopper, BabyMetroidColorDefinitions.TargetColorCount, "sidehopper"),
            Compile(document.BabyMetroid, BabyMetroidColorDefinitions.TargetColorCount, "Shitroid"),
            Compile(document.DeadSidehopper, BabyMetroidColorDefinitions.TargetColorCount,
                "dead sidehopper"));
    }

    /// <summary>Serializes the editable color document as indented camel-case UTF-8 JSON.</summary>
    public static byte[] Write(BabyMetroidColorDocument document) =>
        JsonSerializer.SerializeToUtf8Bytes(document, global::SuperMetroid.AssetExtraction.AssetExtractionJson.Info<global::SuperMetroid.AssetExtraction.Bosses.MotherBrain.BabyMetroidColorDocument>(ArtworkJson.Options));

    /// <summary>Checks one authored color row for its required length and RGB5 channel range before packing it.</summary>
    private static RgbColor[] Compile(PaletteRgb5[]? source, int required, string name) =>
        Rgb5Rows.Compile(source, required, $"Shitroid {name}");
}
