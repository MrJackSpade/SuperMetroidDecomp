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

    public static void Write(Stream json, IntroFinalLineTilemapDocument document)
    {
        ArgumentNullException.ThrowIfNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}

public sealed record IntroFinalLineTilemapDocument
{
    public required int Version { get; init; }
    public required RoomBackgroundTilemapCell[] Cells { get; init; }
}

/// <summary>File identity and dimensions for the cartridge's four-row Japanese subtitle staging map.</summary>
public static class IntroFinalLineTilemapFormat
{
    public const int Version = 1;
    public const int Columns = 32;
    /// <summary>$8B:A72B-A82A: two selected subtitle lines, each composed of an upper and lower glyph half.</summary>
    public const int Rows = 2 * 2;
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
    public const string FileName = "intro-final-text-divider.json";
}
