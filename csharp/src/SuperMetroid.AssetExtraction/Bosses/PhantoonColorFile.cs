using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Bosses.Phantoon.Artwork;
using SuperMetroid.Core.Bosses.Phantoon;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>Reads and writes the installed Phantoon colour document.</summary>
public static class PhantoonColorFile
{
    /// <summary>Loads and validates Phantoon's health, fade-out, and ship-power RGB5 sources.</summary>
    /// <param name="json">Caller-owned stream containing the color document.</param>
    /// <returns>The compiled color catalog.</returns>
    public static PhantoonColorCatalog Read(Stream json)
    {
        Ensure.NotNull(json);
        PhantoonColorDocument document;
        document = JsonAssetDocument.Read<PhantoonColorDocument>(json, ArtworkJson.Options, "Phantoon color");
        if (document.Version != PhantoonColorFormat.Version)
            throw new InvalidDataException("Phantoon colors require the supported version.");
        if (document.HealthBands is null ||
            document.HealthBands.Length != PhantoonColorDefinitions.HealthBandCount)
            throw new InvalidDataException("Phantoon colors require eight health palettes.");
        return new(
            document.HealthBands.Select((band, index) =>
                Compile(band, PhantoonColorDefinitions.HealthBandColorCount,
                    $"health palette {index}")).ToArray(),
            Compile(document.FadeOut, PhantoonColorDefinitions.FadeOutCount, "fade-out"),
            Compile(document.PowerOn, PhantoonColorDefinitions.PowerOnCount, "power-on"));
    }

    /// <summary>Validates and serializes a Phantoon color document as UTF-8 JSON.</summary>
    /// <param name="document">Document containing every required color source.</param>
    /// <returns>A new caller-owned JSON byte array.</returns>
    public static byte[] Write(PhantoonColorDocument document) =>
        JsonAssetDocument.WriteValidated(document, ArtworkJson.Options, json => _ = Read(json));

    /// <summary>Checks a palette row's length and five-bit channels before converting it to hardware color words.</summary>
    private static RgbColor[] Compile(PaletteRgb5[]? source, int count, string name) =>
        Rgb5Rows.Compile(source, count, $"Phantoon {name}");
}
