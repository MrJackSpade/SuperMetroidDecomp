using SuperMetroid.AssetExtraction.Enemies.Artwork;
using System.Text.Json;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Bosses.Phantoon.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>Reads and writes the installed Phantoon colour document.</summary>
public static class PhantoonColorFile
{
    /// <summary>Loads and validates Phantoon's health, fade-out, and ship-power RGB5 sources.</summary>
    /// <param name="json">Caller-owned stream containing the color document.</param>
    /// <returns>The installed color catalog, every set kept as installed.</returns>
    public static PhantoonColorCatalog Read(Stream json)
    {
        Ensure.NotNull(json);
        PhantoonColorDocument document;
        document = JsonAssetDocument.Read<PhantoonColorDocument>(json, ArtworkJson.Options, "Phantoon color");
        if (document.Version != PhantoonColorFormat.Version)
            throw new InvalidDataException("Phantoon colors require the supported version.");
        if (document.HealthBands is null ||
            document.HealthBands.Count != PhantoonColorDefinitions.HealthBandCount)
            throw new InvalidDataException("Phantoon colors require eight health palettes.");
        if (document.PowerOn is null || document.PowerOn.Count != PhantoonColorFormat.PoweredPartKeys.Count)
            throw new InvalidDataException("Phantoon colors require the colours of every part the power-on fades.");
        return new(
            PhantoonHealthBandDefinitions.HighestFirst.ToDictionary(band => band, band =>
                Compile(document.HealthBands.TryGetValue(band.Key, out PaletteRgb5[]? colours) ? colours : null,
                    PhantoonColorFormat.HealthBandColorCount, $"{band} health palette")),
            Compile(document.FadeOut, PhantoonColorFormat.FadeOutCount, "fade-out"),
            PhantoonColorFormat.PoweredPartKeys.ToDictionary(part => part.Key, part =>
                Compile(document.PowerOn.TryGetValue(part.Value, out PaletteRgb5[]? colours) ? colours : null,
                    PhantoonColorFormat.PoweredPartColourCount, $"power-on {part.Value}")));
    }

    /// <summary>Serializes a Phantoon color document as UTF-8 JSON.</summary>
    /// <param name="document">Document containing every required color source.</param>
    /// <returns>A new caller-owned JSON byte array.</returns>
    public static byte[] Write(PhantoonColorDocument document) =>
        JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options);

    /// <summary>Checks a palette row's length and five-bit channels before converting it to hardware color words.</summary>
    private static RgbColor[] Compile(PaletteRgb5[]? source, int count, string name) =>
        Rgb5Rows.Compile(source, count, $"Phantoon {name}");
}
