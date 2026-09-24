using System.Buffers.Binary;
using System.Text.Json;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>One editable 32-by-11 Kraid head frame of ordered SNES BG2 tile words.</summary>
public sealed class KraidHeadTilemapAtlas
{
    private readonly ushort[] words;
    private KraidHeadTilemapAtlas(ushort[] words) => this.words = words;

    public ReadOnlyMemory<ushort> Words => words;

    public static KraidHeadTilemapAtlas Load(Stream json)
    {
        KraidHeadTilemapDocument document;
        try
        {
            document = JsonSerializer.Deserialize<KraidHeadTilemapDocument>(json, JsonOptions)
                ?? throw new InvalidDataException("Kraid head tilemap JSON is empty.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid Kraid head tilemap JSON.", error);
        }
        if (document.Version != KraidHeadTilemapFormat.Version ||
            document.Width != KraidHeadTilemapFormat.Width ||
            document.Height != KraidHeadTilemapFormat.Height ||
            document.Cells is null ||
            document.Cells.Length != KraidBackgroundRomData.HeadTilemapWords)
            throw new InvalidDataException("Kraid head tilemap must have 32 by 11 ordered cells.");

        var words = new ushort[document.Cells.Length];
        for (int index = 0; index < words.Length; index++)
        {
            RoomBackgroundTilemapCell? cell = document.Cells[index];
            if (cell is null ||
                (uint)cell.TileColumn >= RoomBackgroundTilemapFormat.TileColumns ||
                (uint)cell.TileRow >= RoomBackgroundTilemapFormat.TileRows ||
                (uint)cell.Palette >= RoomBackgroundTilemapFormat.PaletteCount)
                throw new InvalidDataException($"Kraid head tilemap cell {index} is invalid.");
            SnesTileFlipFlags flips =
                (cell.FlipX ? SnesTileFlipFlags.Horizontal : 0) |
                (cell.FlipY ? SnesTileFlipFlags.Vertical : 0);
            words[index] = SnesBgTilemapWord.Create(
                cell.TileRow * RoomBackgroundTilemapFormat.TileColumns + cell.TileColumn,
                cell.Palette, cell.Priority, flips).Raw;
        }
        return new KraidHeadTilemapAtlas(words);
    }

    public static byte[] Encode(ReadOnlySpan<byte> native)
    {
        if (native.Length != KraidBackgroundRomData.HeadTilemapWords * sizeof(ushort))
            throw new InvalidDataException("Kraid head tilemap source has wrong length.");
        var cells = new RoomBackgroundTilemapCell[KraidBackgroundRomData.HeadTilemapWords];
        for (int index = 0; index < cells.Length; index++)
        {
            var word = new SnesBgTilemapWord(
                BinaryPrimitives.ReadUInt16LittleEndian(native[(index * 2)..]));
            cells[index] = new RoomBackgroundTilemapCell
            {
                TileColumn = word.CharacterIndex % RoomBackgroundTilemapFormat.TileColumns,
                TileRow = word.CharacterIndex / RoomBackgroundTilemapFormat.TileColumns,
                Palette = word.PaletteIndex,
                Priority = word.HasPriority,
                FlipX = word.FlipHorizontally,
                FlipY = word.FlipVertically,
            };
        }
        byte[] json = JsonSerializer.SerializeToUtf8Bytes(new KraidHeadTilemapDocument
        {
            Version = KraidHeadTilemapFormat.Version,
            Width = KraidHeadTilemapFormat.Width,
            Height = KraidHeadTilemapFormat.Height,
            Cells = cells,
        }, JsonOptions);
        KraidHeadTilemapAtlas roundtrip = Load(new MemoryStream(json, writable: false));
        for (int index = 0; index < roundtrip.words.Length; index++)
            if (roundtrip.words[index] != BinaryPrimitives.ReadUInt16LittleEndian(native[(index * 2)..]))
                throw new InvalidDataException("Kraid head JSON changed a native tile word.");
        return json;
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
    };
}

public sealed record KraidHeadTilemapDocument
{
    public required int Version { get; init; }
    public required int Width { get; init; }
    public required int Height { get; init; }
    public required RoomBackgroundTilemapCell[] Cells { get; init; }
}

public static class KraidHeadTilemapFormat
{
    public const int Version = 1;
    public const int Width = 32;
    public const int Height = 11;
}
