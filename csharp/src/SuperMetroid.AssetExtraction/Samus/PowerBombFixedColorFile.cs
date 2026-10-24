using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Palettes;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Reads and writes the installed Power Bomb fixed colour document.</summary>
public static class PowerBombFixedColorFile
{
    /// <summary>Validates the installed fixed-color JSON and compiles independent edits to the two native RGB5 sequences without changing phase or radius mechanics.</summary>
    /// <param name="json">UTF-8 JSON source consumed from its current position and left open.</param>
    /// <returns>Compiled component triplets detached from the document arrays, with stock colors resolved from reviewed definitions where available.</returns>
    /// <exception cref="ArgumentNullException">The source stream is null.</exception>
    /// <exception cref="InvalidDataException">The JSON contains duplicate or unknown properties, an unsupported version, incorrect sequence lengths, null colors, or channels outside 0..31.</exception>
    public static PowerBombFixedColorCatalog Read(Stream json)
    {
        Ensure.NotNull(json);
        PowerBombFixedColorDocument document;
        document = JsonAssetDocument.Read<PowerBombFixedColorDocument>(json, ArtworkJson.Options, "Power Bomb fixed-color");
        if (document.Version != PowerBombFixedColorFormat.Version)
            throw new InvalidDataException("Power Bomb fixed colors require the supported version.");
        return new(Compile(document.PreExplosion,
                SamusPaletteDefinitions.PowerBomb.PreExplosionColorCount, "pre-explosion"),
            Compile(document.Explosion,
                SamusPaletteDefinitions.PowerBomb.ExplosionColorCount, "explosion"));
    }

    /// <summary>Serializes the editable sequences to indented camel-case UTF-8 JSON and validates the resulting bytes through <see cref="Read"/>.</summary>
    /// <param name="document">The two complete RGB5 color sequences to serialize; the writer does not retain their collections.</param>
    /// <returns>Validated JSON bytes for <see cref="PowerBombFixedColorFormat.FileName"/>.</returns>
    /// <exception cref="InvalidDataException">The serialized document fails schema, sequence-length, or RGB5-channel validation.</exception>
    public static byte[] Write(PowerBombFixedColorDocument document) =>
        JsonAssetDocument.WriteValidated(document, ArtworkJson.Options, json => _ = Read(json));

    /// <summary>Checks a sequence's length and RGB5 channels before packing its colours.</summary>
    private static Bgr555[] Compile(PaletteRgb5[]? source, int count, string name)
    {
        if (source is null || source.Length != count)
            throw new InvalidDataException($"Power Bomb {name} requires {count} RGB5 colors.");
        var colors = new Bgr555[source.Length];
        for (int index = 0; index < source.Length; index++)
        {
            PaletteRgb5? color = source[index];
            if (color is null || (uint)color.Red > 31 ||
                (uint)color.Green > 31 || (uint)color.Blue > 31)
                throw new InvalidDataException($"Power Bomb {name} color {index} requires components from zero through 31.");
            colors[index] = new Bgr555(color.Red, color.Green, color.Blue);
        }
        return colors;
    }
}
