using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Reads and writes the installed Samus suit colour document.</summary>
public static class SamusSuitColorFile
{
    /// <summary>Loads and validates the three complete normal-suit palettes from JSON.</summary>
    /// <param name="json">Caller-owned stream containing the suit-color document.</param>
    /// <returns>The compiled suit-color catalog.</returns>
    public static SamusSuitColorCatalog Read(Stream json)
    {
        Ensure.NotNull(json);
        SamusSuitColorDocument document;
        document = JsonAssetDocument.Read<SamusSuitColorDocument>(json, ArtworkJson.Options, "Samus suit color");
        if (document.Version != SamusSuitColorFormat.Version)
            throw new InvalidDataException("Samus suit colors require the supported version.");
        return new(Compile(document.Power, "Power"), Compile(document.Varia, "Varia"),
            Compile(document.Gravity, "Gravity"));
    }

    /// <summary>Validates and serializes a suit-color document as UTF-8 JSON.</summary>
    /// <param name="document">Document containing all three 16-color palettes.</param>
    /// <returns>A new caller-owned JSON byte array.</returns>
    public static byte[] Write(SamusSuitColorDocument document) =>
        JsonAssetDocument.WriteValidated(document, ArtworkJson.Options, json => _ = Read(json));

    /// <summary>Validates all sixteen RGB5 slots, including color zero, and packs them into native words.</summary>
    private static RgbColor[] Compile(PaletteRgb5[]? source, string name)
    {
        if (source is null || source.Length != SamusSuitColorFormat.ColorsPerSuit)
            throw new InvalidDataException($"Samus {name} suit requires sixteen RGB5 colors.");
        var result = new RgbColor[source.Length];
        for (int index = 0; index < source.Length; index++)
        {
            PaletteRgb5? color = source[index];
            if (color is null || (uint)color.Red > 31 || (uint)color.Green > 31 ||
                (uint)color.Blue > 31)
                throw new InvalidDataException($"Samus {name} color {index} requires RGB components 0..31.");
            result[index] = color.ToRgbColor();
        }
        return result;
    }
}
