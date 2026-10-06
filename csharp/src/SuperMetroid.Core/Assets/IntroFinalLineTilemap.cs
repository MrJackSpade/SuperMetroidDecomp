using System.Text.Json;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>The four-row illustrated-page divider, as ordered editable BG tile references.</summary>
public sealed class IntroFinalLineTilemap
{
    private readonly ushort[]? suppliedWords;
    private readonly ushort blankWord, firstInteriorWord;

    private IntroFinalLineTilemap(ushort[] words)
    {
        blankWord = words[0];
        firstInteriorWord = words[IntroFinalLineTilemapFormat.UnresolvedMarginColumns];
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

    private ushort CalculateWord(int index)
    {
        int column = index % IntroFinalLineTilemapFormat.Columns;
        int row = index / IntroFinalLineTilemapFormat.Columns;
        int margin = IntroFinalLineTilemapFormat.UnresolvedMarginColumns;
        int interiorColumns = IntroFinalLineTilemapFormat.Columns - 2 * margin;
        if (column < margin || column >= margin + interiorColumns) return blankWord;
        // Paired screen rows select the upper/lower halves of each two-row tile strip.
        int tileStripRow = 2 * (row & 1) + (row >> 1);
        return unchecked((ushort)(firstInteriorWord + tileStripRow * interiorColumns + column - margin));
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

/// <summary>File identity and dimensions for the cartridge's four-row text divider.</summary>
public static class IntroFinalLineTilemapFormat
{
    public const int Version = 1;
    public const int Columns = 32;
    public const int Rows = 4;
    public const int CellCount = Columns * Rows;
    /// <summary>$8B:A72B-A82A subtitle tilemap: four blank columns at each side; this chosen margin remains required.</summary>
    internal const int UnresolvedMarginColumns = 4;
    public const string FileName = "intro-final-text-divider.json";
}
