using System.Text.Json;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>The four-row Japanese subtitle staging map, as ordered editable BG tile references.</summary>
public sealed class IntroFinalLineTilemap
{
    private readonly ushort[]? suppliedWords;

    private IntroFinalLineTilemap(ushort[] words)
    {
        for (int index = 0; index < words.Length; index++)
        {
            if (words[index] == CalculateWord(index)) continue;
            suppliedWords = words;
            break;
        }
    }

    /// <summary>Native word order: 32 columns in each of four consecutive rows.</summary>
    public ReadOnlyMemory<ushort> Words
    {
        get
        {
            if (suppliedWords is not null) return suppliedWords;
            var output = new ushort[IntroFinalLineTilemapFormat.CellCount];
            for (int index = 0; index < output.Length; index++) output[index] = CalculateWord(index);
            return output;
        }
    }

    private static ushort CalculateWord(int index)
    {
        int column = index % IntroFinalLineTilemapFormat.Columns;
        int row = index / IntroFinalLineTilemapFormat.Columns;
        int margin = IntroFinalLineTilemapFormat.MarginColumns;
        int interiorColumns = IntroFinalLineTilemapFormat.Columns - 2 * margin;
        if (column < margin || column >= margin + interiorColumns) return IntroFinalLineTilemapFormat.MarginWord;
        // Native $8B:8D84 places lower halves 0300 bytes after upper halves; each group contains two text lines.
        int tileStripRow = 2 * (row & 1) + (row >> 1);
        return unchecked((ushort)(IntroFinalLineTilemapFormat.FirstInteriorWord + tileStripRow * interiorColumns + column - margin));
    }
    /// <summary>Validates and compiles 128 ordered character references into the four-row BG3 subtitle map corresponding to $8B:A72B-A82A; exact stock words remain formula-derived.</summary>
    /// <param name="json">Caller-owned stream containing the supported editable tile-reference document.</param>
    /// <returns>The subtitle staging map, including editable margins, palette, priority, and flip attributes.</returns>
    /// <exception cref="InvalidDataException">The document schema, cell count, or character/palette selectors are invalid.</exception>
    public static IntroFinalLineTilemap Load(Stream json)
    {
        ArgumentNullException.ThrowIfNull(json);
        IntroFinalLineTilemapDocument document = JsonAssetDocument.Read<IntroFinalLineTilemapDocument>(
            json, MapPresentationFormat.JsonOptions, "opening divider tilemap");
        if (document.Version != IntroFinalLineTilemapFormat.Version ||
            document.Cells is not { Length: IntroFinalLineTilemapFormat.CellCount })
            throw new InvalidDataException("Opening divider tilemap requires 128 ordered cells.");

        var words = new ushort[IntroFinalLineTilemapFormat.CellCount];
        for (int index = 0; index < words.Length; index++)
        {
            RoomBackgroundTilemapCell? cell = document.Cells[index];
            if (cell is null ||
                (uint)cell.TileColumn >= RoomBackgroundTilemapFormat.TileColumns ||
                (uint)cell.TileRow >= RoomBackgroundTilemapFormat.TileRows ||
                (uint)cell.Palette >= RoomBackgroundTilemapFormat.PaletteCount)
                throw new InvalidDataException($"Opening divider cell {index} has an invalid tile or palette.");
            SnesTileFlipFlags flips =
                (cell.FlipX ? SnesTileFlipFlags.Horizontal : 0) |
                (cell.FlipY ? SnesTileFlipFlags.Vertical : 0);
            words[index] = SnesBgTilemapWord.Create(
                cell.TileRow * RoomBackgroundTilemapFormat.TileColumns + cell.TileColumn,
                cell.Palette, cell.Priority, flips).Raw;
        }
        return new IntroFinalLineTilemap(words);
    }

    /// <summary>Serializes the tile-reference document as UTF-8 JSON and validates it through <see cref="Load"/> before writing any destination bytes.</summary>
    /// <param name="json">Caller-owned destination stream.</param>
    /// <param name="document">Complete four-row map to validate and serialize.</param>
    public static void Write(Stream json, IntroFinalLineTilemapDocument document)
    {
        ArgumentNullException.ThrowIfNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}

/// <summary>Editable subtitle-staging tile references for BG3 rows 24-27; glyph pixels, translated narration, and reveal timing are separate resources or runtime state.</summary>
public sealed record IntroFinalLineTilemapDocument
{
    /// <summary>Schema revision, which loading requires to equal <see cref="IntroFinalLineTilemapFormat.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Exactly 128 non-null cells in four rows of 32, including margins; character-sheet columns/rows are 0..31 and BG palette selectors are 0..7.</summary>
    public required RoomBackgroundTilemapCell[] Cells { get; init; }
}

/// <summary>File identity and dimensions for the cartridge's four-row Japanese subtitle staging map.</summary>
public static class IntroFinalLineTilemapFormat
{
    /// <summary>Supported revision of the ordered editable subtitle-map schema.</summary>
    public const int Version = 1;
    /// <summary>Destination cells per BG3 row, comprising 24 stock subtitle columns and four stock margin columns on each side.</summary>
    public const int Columns = 32;
    /// <summary>$8B:A72B-A82A: two selected subtitle lines, each composed of an upper and lower glyph half.</summary>
    public const int Rows = 2 * 2;
    /// <summary>Required tile-reference count, 128, corresponding to the $0100-byte native map copied into BG3 rows 24-27.</summary>
    public const int CellCount = Columns * Rows;
    /// <summary>$8B:8DEA/A68F: 0600 bytes of 2bpp glyph staging form four displayed tile rows, yielding 24 columns.</summary>
    internal const int InteriorColumns = IntroCinematicRomData.Vram.JapaneseBlankCharactersByteCount /
        (IntroFontAtlasFormat.BitsPerPixel * IntroFontAtlasFormat.TileSize * Rows);
    /// <summary>$8B:A72B-A82A: center the subtitle staging width in the 32-column BG page.</summary>
    internal const int MarginColumns = (Columns - InteriorColumns) / 2;
    /// <summary>$8B:A6B7: margins share the surrounding Japanese blank tile/palette/priority word.</summary>
    internal static ushort MarginWord => IntroCinematicRomData.Text.JapaneseBlank.Raw;
    /// <summary>$8B:A72B: first subtitle tile follows its VRAM staging destination relative to the font base; palette four and priority are selected subtitle style.</summary>
    internal static ushort FirstInteriorWord => SnesBgTilemapWord.Create(
        (IntroCinematicRomData.Vram.JapaneseBlankCharactersDestinationByte - IntroCinematicRomData.Vram.FontOneDestinationByte) /
        (IntroFontAtlasFormat.BitsPerPixel * IntroFontAtlasFormat.TileSize), SubtitlePalette, true, default).Raw;
    /// <summary>$8B:A733: chosen palette four for the Japanese subtitle glyph staging area; this display policy is retained as selected typesetting, not prose, pixels or timing.</summary>
    private const int SubtitlePalette = 4;
    /// <summary>Editable JSON filename for the opening cinematic's four-row subtitle staging map, not a prose or animation-sequence file.</summary>
    public const string FileName = "intro-final-text-divider.json";
}
