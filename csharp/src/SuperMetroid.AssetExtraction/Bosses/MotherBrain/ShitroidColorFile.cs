using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain;

namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain;

/// <summary>Reads and writes the installed Shitroid colour document.</summary>
public static class ShitroidColorFile
{
    /// <summary>Loads <c>shitroid-colors.json</c>, requiring version 1, eight four-color pulse frames, three sixteen-color targets, and RGB5 channels from 0 through 31.</summary>
    /// <param name="json">Caller-owned JSON stream consumed from its current position and left open; unknown and duplicate properties are rejected.</param>
    /// <returns>Compiled colors retaining independently edited frames and targets exactly, sharing the calculated stock pulse only when all samples agree.</returns>
    public static ShitroidColorCatalog Read(Stream json)
    {
        Ensure.NotNull(json);
        ShitroidColorDocument document;
        document = JsonAssetDocument.Read<ShitroidColorDocument>(json, ArtworkJson.Options, "Shitroid color");
        if (document.Version != ShitroidColorFormat.Version)
            throw new InvalidDataException("Shitroid colors require the supported version.");
        if (document.Normal is null ||
            document.Normal.Length != ShitroidColorDefinitions.NormalFrameCount)
            throw new InvalidDataException("Shitroid normal cycle requires eight frames.");
        return new(document.Normal.Select((frame, index) =>
                Compile(frame, ShitroidColorDefinitions.NormalColorsPerFrame,
                    $"normal frame {index}")).ToArray(),
            Compile(document.Sidehopper, ShitroidColorDefinitions.TargetColorCount, "sidehopper"),
            Compile(document.Shitroid, ShitroidColorDefinitions.TargetColorCount, "Shitroid"),
            Compile(document.DeadSidehopper, ShitroidColorDefinitions.TargetColorCount,
                "dead sidehopper"));
    }

    /// <summary>Serializes the editable color document as indented camel-case UTF-8 JSON and validates the serialized schema, dimensions, and channel bounds before returning it.</summary>
    public static byte[] Write(ShitroidColorDocument document) =>
        JsonAssetDocument.WriteValidated(document, ArtworkJson.Options, json => _ = Read(json));

    /// <summary>Checks one authored color row for its required length and RGB5 channel range before packing it.</summary>
    private static Bgr555[] Compile(PaletteRgb5[]? source, int required, string name) =>
        Rgb5Rows.Compile(source, required, $"Shitroid {name}");
}
