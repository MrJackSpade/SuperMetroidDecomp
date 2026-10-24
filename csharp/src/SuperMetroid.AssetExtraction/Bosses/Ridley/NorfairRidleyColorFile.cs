using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Bosses.Ridley.Artwork;
using SuperMetroid.Core.Bosses.Ridley;

namespace SuperMetroid.AssetExtraction.Bosses.Ridley;

/// <summary>Reads and writes the installed Norfair Ridley colour document.</summary>
public static class NorfairRidleyColorFile
{
    /// <summary>Uses stable camel-case JSON and rejects unknown authoring properties.</summary>
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

    /// <summary>Reads the versioned color document, rejecting duplicate or unknown properties, missing palette entries, and RGB channels outside 0..31, then compiles the colors to SNES RGB5 words.</summary>
    /// <param name="json">UTF-8 JSON stream containing the initial palette and all reveal rows.</param>
    /// <returns>The validated editable color catalog.</returns>
    public static NorfairRidleyColorCatalog Read(Stream json)
    {
        NorfairRidleyColorDocument document;
        document = JsonAssetDocument.Read<NorfairRidleyColorDocument>(json, Options, "Norfair Ridley color");

        if (document.Version != NorfairRidleyColorFormat.Version ||
            document.Initial is null ||
            document.Initial.Length != NorfairRidleyPaletteRomData.InitialColorCount ||
            document.Reveal is null ||
            document.Reveal.Length != NorfairRidleyPaletteRomData.RevealRowCount)
            throw new InvalidDataException("Norfair Ridley colors require the initial palette and all reveal rows.");

        Bgr555[] initial = Compile(document.Initial, "initial");
        var reveal = new Bgr555[document.Reveal.Length][];
        for (int row = 0; row < reveal.Length; row++)
        {
            if (document.Reveal[row] is null ||
                document.Reveal[row].Length != NorfairRidleyPaletteRomData.RevealColorCount)
                throw new InvalidDataException($"Norfair Ridley reveal row {row} requires fourteen colors.");
            reveal[row] = Compile(document.Reveal[row], $"reveal row {row}");
        }
        return new(initial, reveal);
    }

    /// <summary>Serializes a color document as indented camel-case UTF-8 JSON and validates the result through <see cref="Read"/> before returning it.</summary>
    /// <param name="document">Palette document to serialize; it must satisfy the loader's version, dimensions, and RGB5 constraints.</param>
    /// <returns>Validated JSON bytes suitable for the catalog asset file.</returns>
    public static byte[] Write(NorfairRidleyColorDocument document) =>
        JsonAssetDocument.WriteValidated(document, Options, json => _ = Read(json));

    /// <summary>Validates each RGB5 channel and packs one authored row into SNES color words.</summary>
    private static Bgr555[] Compile(PaletteRgb5[] colors, string label)
    {
        var words = new Bgr555[colors.Length];
        for (int color = 0; color < words.Length; color++)
        {
            PaletteRgb5? rgb = colors[color];
            if (rgb is null || (uint)rgb.Red > 31 ||
                (uint)rgb.Green > 31 || (uint)rgb.Blue > 31)
                throw new InvalidDataException($"Norfair Ridley {label} color {color} requires RGB5 channels.");
            words[color] = rgb.ToBgr555();
        }
        return words;
    }
}
