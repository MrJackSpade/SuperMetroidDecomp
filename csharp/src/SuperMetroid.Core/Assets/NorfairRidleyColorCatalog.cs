using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable initial OBJ colors and fifteen rows of fourteen arena-reveal BG colors for Lower Norfair Ridley.</summary>
public sealed class NorfairRidleyColorCatalog
{
    /// <summary>Canonical selected RGB5 colors and ordered rows, independent of JSON encoding.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create("NorfairRidleyColorCatalog-v1", content =>
        {
            Span<ushort> initialWords = stackalloc ushort[NorfairRidleyPaletteRomData.InitialColorCount];
            for (int color = 0; color < initialWords.Length; color++) initialWords[color] = initial.ColorAt(color);
            content.AppendWords("initial", initialWords);
            ushort[][] revealWords = new ushort[NorfairRidleyPaletteRomData.RevealRowCount][];
            for (int row = 0; row < revealWords.Length; row++)
            {
                revealWords[row] = new ushort[NorfairRidleyPaletteRomData.RevealColorCount];
                for (int color = 0; color < revealWords[row].Length; color++) revealWords[row][color] = reveal.ColorAt(row, color);
            }
            content.AppendWordFrames("reveal", revealWords);
        });

    /// <summary>Initial OBJ palette values used by <see cref="ApplyInitial"/>.</summary>
    private readonly NorfairRidleyInitialPaintDefinitions initial;
    /// <summary>Fourteen-color rows applied to the arena background during the reveal.</summary>
    private readonly NorfairRidleyRevealPaintDefinitions reveal;

    /// <summary>Creates a catalog from compiled initial palette words and ordered reveal rows.</summary>
    /// <param name="initial">The 32 RGB5 colors installed in the OBJ palette region.</param>
    /// <param name="reveal">The fifteen rows of fourteen RGB5 arena colors.</param>
    private NorfairRidleyColorCatalog(ushort[] initial, ushort[][] reveal)
    {
        this.initial = new(initial);
        this.reveal = new(reveal);
    }

    /// <summary>Strict camel-case JSON settings shared by catalog loading and writing.</summary>
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

    /// <summary>Installs the 32 initial RGB5 colors in CGRAM entries 160..191 (OBJ palettes 2 and 3), matching the native copy from <c>$A6:E1CF</c>; subsequent palette clearing is the caller's responsibility.</summary>
    /// <param name="cgram">Destination color memory to update.</param>
    public void ApplyInitial(SnesCgram cgram)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        for (int color = 0; color < NorfairRidleyPaletteRomData.InitialColorCount; color++)
            cgram.SetColor(NorfairRidleyPaletteRomData.InitialCgramIndex + color, initial.ColorAt(color));
    }

    /// <summary>Copies one arena-reveal row to CGRAM entries 113..126 (BG palette 7, colors 1..14), matching <c>FadeInRidleyRoomBackground</c> at <c>$A6:A4D6</c>; does not advance the reveal timer or process its terminator.</summary>
    /// <param name="cgram">Destination color memory to update.</param>
    /// <param name="row">Zero-based reveal row, from 0 through 14; row 15 is the runtime terminator, not a color row.</param>
    public void ApplyReveal(SnesCgram cgram, int row)
    {
        NorfairRidleyRevealPaintDefinitions.ValidateRow(row);
        ArgumentNullException.ThrowIfNull(cgram);
        for (int color = 0; color < NorfairRidleyPaletteRomData.RevealColorCount; color++)
            cgram.SetColor(NorfairRidleyPaletteRomData.RevealCgramIndex + color, reveal.ColorAt(row, color));
    }

    /// <summary>Reads the versioned color document, rejecting duplicate or unknown properties, missing palette entries, and RGB channels outside 0..31, then compiles the colors to SNES RGB5 words.</summary>
    /// <param name="json">UTF-8 JSON stream containing the initial palette and all reveal rows.</param>
    /// <returns>The validated editable color catalog.</returns>
    public static NorfairRidleyColorCatalog Load(Stream json)
    {
        NorfairRidleyColorDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            RejectDuplicates(parsed.RootElement);
            document = parsed.RootElement.Deserialize<NorfairRidleyColorDocument>(Options)
                ?? throw new InvalidDataException("Norfair Ridley colors are null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid Norfair Ridley color JSON.", error);
        }

        if (document.Version != NorfairRidleyColorFormat.Version ||
            document.Initial is null ||
            document.Initial.Length != NorfairRidleyPaletteRomData.InitialColorCount ||
            document.Reveal is null ||
            document.Reveal.Length != NorfairRidleyPaletteRomData.RevealRowCount)
            throw new InvalidDataException("Norfair Ridley colors require the initial palette and all reveal rows.");

        ushort[] initial = Compile(document.Initial, "initial");
        var reveal = new ushort[document.Reveal.Length][];
        for (int row = 0; row < reveal.Length; row++)
        {
            if (document.Reveal[row] is null ||
                document.Reveal[row].Length != NorfairRidleyPaletteRomData.RevealColorCount)
                throw new InvalidDataException($"Norfair Ridley reveal row {row} requires fourteen colors.");
            reveal[row] = Compile(document.Reveal[row], $"reveal row {row}");
        }
        return new(initial, reveal);
    }

    /// <summary>Serializes a color document as indented camel-case UTF-8 JSON and validates the result through <see cref="Load"/> before returning it.</summary>
    /// <param name="document">Palette document to serialize; it must satisfy the loader's version, dimensions, and RGB5 constraints.</param>
    /// <returns>Validated JSON bytes suitable for the catalog asset file.</returns>
    public static byte[] Write(NorfairRidleyColorDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, Options);
        _ = Load(new MemoryStream(bytes, writable: false));
        return bytes;
    }

    /// <summary>Converts one validated-shape palette row from RGB5 channel objects to packed SNES color words.</summary>
    /// <param name="colors">Color objects to pack in their authored order.</param>
    /// <param name="label">Palette portion or row name included in validation errors.</param>
    /// <returns>Packed RGB555 words corresponding to the input entries.</returns>
    /// <exception cref="InvalidDataException">An entry is null or any RGB5 channel exceeds five bits.</exception>
    private static ushort[] Compile(PaletteRgb5[] colors, string label)
    {
        var words = new ushort[colors.Length];
        for (int color = 0; color < words.Length; color++)
        {
            PaletteRgb5? rgb = colors[color];
            if (rgb is null || (uint)rgb.Red > 31 ||
                (uint)rgb.Green > 31 || (uint)rgb.Blue > 31)
                throw new InvalidDataException($"Norfair Ridley {label} color {color} requires RGB5 channels.");
            words[color] = (ushort)(rgb.Red | rgb.Green << 5 | rgb.Blue << 10);
        }
        return words;
    }

    /// <summary>Rejects duplicate JSON property names before the document is deserialized.</summary>
    /// <param name="value">Root JSON value whose objects are checked recursively.</param>
    private static void RejectDuplicates(JsonElement value) =>
        JsonAssetDocument.RejectDuplicateProperties(value, StringComparer.Ordinal,
            name => new InvalidDataException("Duplicate Norfair Ridley color property."));
}

/// <summary>Editable JSON schema for Lower Norfair Ridley's initial OBJ palette and ordered arena-background reveal colors; reveal timing and the terminating step are not asset data.</summary>
public sealed record NorfairRidleyColorDocument
{
    /// <summary>Schema version, which must equal <see cref="NorfairRidleyColorFormat.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>The 32 initial RGB5 colors in native <c>$A6:E1CF-E20E</c> order, including each palette's color-zero entry, copied to CGRAM entries 160..191.</summary>
    public required PaletteRgb5[] Initial { get; init; }
    /// <summary>Fifteen ordered reveal rows, each containing fourteen RGB5 colors for BG palette 7 colors 1..14; the caller applies them at the native three-dispatcher-call cadence.</summary>
    public required PaletteRgb5[][] Reveal { get; init; }
}

/// <summary>Editable visual rows; the reveal's cadence and zero terminator stay in code.</summary>
public static class NorfairRidleyColorFormat
{
    /// <summary>Asset filename used to install Lower Norfair Ridley's editable initial and arena-reveal colors.</summary>
    public const string FileName = "norfair-ridley-colors.json";
    /// <summary>Supported JSON schema revision for the 32-color initial palette and fifteen fourteen-color reveal rows.</summary>
    public const int Version = 1;
}
