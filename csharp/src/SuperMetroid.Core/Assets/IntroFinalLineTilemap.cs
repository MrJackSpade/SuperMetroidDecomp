using System.Text.Json;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>The four-row illustrated-page divider, as ordered editable BG tile references.</summary>
public sealed class IntroFinalLineTilemap
{
    private readonly ushort[] words;

    private IntroFinalLineTilemap(ushort[] words) => this.words = words;

    /// <summary>Native word order: 32 columns in each of four consecutive rows.</summary>
    public ReadOnlyMemory<ushort> Words => words;

    public static IntroFinalLineTilemap Load(Stream json)
    {
        ArgumentNullException.ThrowIfNull(json);
        IntroFinalLineTilemapDocument document;
        try
        {
            document = JsonSerializer.Deserialize<IntroFinalLineTilemapDocument>(json,
                MapPresentationFormat.JsonOptions)
                ?? throw new InvalidDataException("Opening divider tilemap JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid opening divider tilemap JSON.", error);
        }
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
    public const string FileName = "intro-final-text-divider.json";
}
